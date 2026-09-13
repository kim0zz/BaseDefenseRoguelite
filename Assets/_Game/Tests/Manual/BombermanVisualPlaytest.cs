#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>Manually invoked integration smoke test in an empty Play Mode scene. Excluded from builds.</summary>
public sealed class BombermanVisualPlaytest : MonoBehaviour
{
    private readonly List<string> _report = new();
    private Gamepad _pad;
    private PlayerCharacter _player;
    private BombermanModelView _view;
    private string _output;
    public void Begin(string output) { _output = output; StartCoroutine(Run()); }
    private void Check(bool pass, string description)
    {
        _report.Add((pass ? "PASS " : "FAIL ") + description);
        Debug.Log(_report[^1]);
        File.WriteAllLines(Path.Combine(_output, "mad-doctor-tests.txt"), _report);
    }
    private IEnumerator Run()
    {
        var bootstrap = FindAnyObjectByType<CombatBootstrap>();
        _pad = InputSystem.AddDevice<Gamepad>();
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "Doctor_Test_Player";go.transform.position = Vector3.up;
        Destroy(go.GetComponent<Collider>());
        _player = go.AddComponent<PlayerCharacter>();_player.Initialize(0, PlayerInputMode.Gamepad, _pad);
        bootstrap.SetupPlayer(go, 0);
        _view = go.GetComponentInChildren<BombermanModelView>();
        yield return new WaitForSeconds(.05f);
        Check(_view != null && !go.GetComponent<Renderer>().enabled, "Bomberman prefab replaces capsule");
        if (_view == null) yield break;
        var skin = _view.GetComponentInChildren<SkinnedMeshRenderer>();
        Check(skin.sharedMesh.GetIndexCount(0) / 3 == 1448 && skin.sharedMaterials.Length == 1, "1448 triangles, one material");
        Check(go.GetComponentsInChildren<Collider>().Length == 0, "No extra colliders on visual/player (existing player convention)");
        Check(skin.sharedMaterial.mainTexture != null && skin.sharedMaterial.mainTexture.width == 64, "Palette texture assigned (UV and colors separately verified in rendered captures)");
        var arm = _view.GetComponentsInChildren<Transform>().First(t => t.name == "Arm.R");
        var thigh = _view.GetComponentsInChildren<Transform>().First(t => t.name == "Thigh.R");
        var idleArm = arm.localRotation;var idleThigh = thigh.localRotation;
        Check(_view.CurrentVisualAction == "Idle", "Idle state");
        Capture("idle");
        var before = go.transform.position;
        InputSystem.QueueStateEvent(_pad, new GamepadState { leftStick = Vector2.right, rightStick = Vector2.up });
        yield return new WaitForSeconds(.21f);
        Check(_view.CurrentVisualAction == "Run" && Vector3.Distance(before, go.transform.position) > .3f, "Movement drives Run");
        Check(Quaternion.Angle(idleThigh, thigh.localRotation) > 5f, "Run changes actual leg bones");
        InputSystem.QueueStateEvent(_pad, new GamepadState { rightStick = Vector2.up }.WithButton(GamepadButton.West));
        var sawThrow = false;var sawProjectile = false;var armMoved = false;
        for(var t=0f;t<1.8f;t+=Time.deltaTime)
        {
            sawThrow |= _view.CurrentVisualAction == "Throw";
            sawProjectile |= FindObjectsByType<Projectile>().Length > 0;
            armMoved |= Quaternion.Angle(idleArm, arm.localRotation) > 20f;
            yield return null;
        }
        InputSystem.QueueStateEvent(_pad, new GamepadState());
        Check(sawThrow && sawProjectile, "AA Throw and actual projectile spawn");
        Check(armMoved, "Throw drives upper-body bones");
        yield return new WaitForSeconds(1.6f);
        var skills=go.GetComponent<PlayerSkillController>();
        string[] names={"Place","Detonate","Kick"};
        for(int slot=0;slot<3;slot++)
        {
            if(slot==2)
            {
                skills.ResetActiveCooldowns();skills.BufferCastFromAttackRecovery(0);
                yield return new WaitForSeconds(.5f);
            }
            skills.ResetActiveCooldowns();skills.BufferCastFromAttackRecovery(slot);
            bool saw=false;bool sawCooldown=false;bool sawKick=false;float maxAngle=0;
            for(var t=0f;t<1.2f;t+=Time.deltaTime)
            {
                sawCooldown |= skills.GetCooldownRemaining(slot)>0;
                sawKick |= FindObjectsByType<DeployableMotor>().Any(m=>m.State==DeployableMotorState.Kicked);
                if(_view.CurrentVisualAction==names[slot])
                {
                    if(!saw) Capture(names[slot]);
                    saw=true;maxAngle=Mathf.Max(maxAngle,Quaternion.Angle(idleThigh,thigh.localRotation));
                }
                yield return null;
            }
            Check(saw && sawCooldown, "Skill "+slot+" "+names[slot]+" and gameplay cooldown");
            if(slot==0)Check(DeployableRegistry.GetNormalCount(go)>0,"Q places an owned bomb");
            if(slot==1)Check(DeployableRegistry.GetNormalCount(go)==0,"E detonates the owned bomb");
            if(slot==2)Check(maxAngle>30f && sawKick,"R moves the leg and kicks the owned bomb");
        }
        skills.ConfigureSlot(3,SkillContentFactory.CreateBombermanNalot());
        skills.ResetActiveCooldowns();skills.BufferCastFromAttackRecovery(3);
        var sawCast=false;
        for(var t=0f;t<1.5f;t+=Time.deltaTime){sawCast|=_view.CurrentVisualAction=="Cast";yield return null;}
        Check(sawCast,"Active ultimate uses Cast animation (configured only in test)");
        var health=go.GetComponent<Health>();health.TakeDamage(1);
        yield return new WaitForSeconds(.04f);
        var block=new MaterialPropertyBlock();skin.GetPropertyBlock(block);
        Check(block.GetColor("_BaseColor").g<.5f,"Damage flashes skinned model");
        var pos=go.transform.position;health.ForceDeath();
        yield return new WaitForSeconds(.8f);
        Check(_view.CurrentVisualAction=="Death" && !health.IsAlive && !_player.IsCombatEnabled,"Death animation and gameplay disable");
        Check(Vector3.Distance(pos,go.transform.position)<.001f,"Animation does not move gameplay root");
        Capture("death");
        yield return new WaitForSeconds(20f);
        Check(health.IsAlive && _player.IsCombatEnabled && _view.CurrentVisualAction=="Idle","Normal 20-second respawn restores Idle and combat");
        var other=GameObject.CreatePrimitive(PrimitiveType.Capsule);other.name="OtherClass_Test";other.transform.position=new Vector3(20,1,0);
        var otherPlayer=other.AddComponent<PlayerCharacter>();otherPlayer.Initialize(1,PlayerInputMode.Gamepad,_pad);bootstrap.SetupPlayer(other,1);
        yield return null;
        Check(other.GetComponentInChildren<BombermanModelView>()==null && other.GetComponent<Renderer>().enabled,"Other class retains its existing visual");
        InputSystem.RemoveDevice(_pad);_pad=null;
        _report.Add("DONE — isolated Play Mode integration, no changes to skill balance.");File.WriteAllLines(Path.Combine(_output,"mad-doctor-tests.txt"),_report);
    }
    private void Capture(string label)
    {
        var cameraObject=new GameObject("TestCaptureCamera");var cam=cameraObject.AddComponent<Camera>();
        cam.transform.position=_player.transform.position+new Vector3(3,2,5);cam.transform.LookAt(_player.transform.position+Vector3.up*.15f);
        cam.orthographic=true;cam.orthographicSize=1.7f;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.1f,.15f,.2f);
        var rt=new RenderTexture(700,800,24);var previous=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
        var tex=new Texture2D(700,800,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,700,800),0,0);tex.Apply();
        File.WriteAllBytes(Path.Combine(_output,"doctor-test-"+label+".png"),tex.EncodeToPNG());
        cam.targetTexture=null;RenderTexture.active=previous;Destroy(rt);Destroy(tex);Destroy(cameraObject);
    }
    private void OnDestroy(){if(_pad!=null)InputSystem.RemoveDevice(_pad);}
}
#endif
