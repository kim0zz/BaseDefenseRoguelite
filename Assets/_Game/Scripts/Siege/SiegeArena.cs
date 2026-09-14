using System.Collections.Generic;
using UnityEngine;

/// <summary>Jedno pole walki; po wyłomie cały front przechodzi na dziedziniec.</summary>
[DefaultExecutionOrder(-200)]
public class SiegeArena : MonoBehaviour
{
    public static SiegeArena Instance { get; private set; }
    public SiegeArenaConfig Config;
    private Health gate;
    private Health core;
    public Health Gate { get { EnsureInitialized(); return gate; } private set => gate = value; }
    public Health Core { get { EnsureInitialized(); return core; } private set => core = value; }
    public bool IsInner { get; private set; }
    public bool IsRetreating { get; set; }
    public IDamageable DefenseTarget => !IsInner && Gate != null && Gate.IsAlive ? Gate : Core;
    public Vector2 Depth => IsInner ? Config.InnerDepth : Config.OuterDepth;
    public Vector3 Center => new(0, 0, (Depth.x + Depth.y) * .5f);
    readonly List<Material> materials = new();
    readonly Dictionary<Color, Material> materialCache = new();
    readonly List<Renderer> entrances = new();
    bool ownsConfig;
    Transform entranceRoot;
    MapPlayArea playArea;
    Color stone = new(.18f,.24f,.29f);
    Color trim = new(.37f,.47f,.49f);
    bool initialized;

    void Awake() => EnsureInitialized();

    public void EnsureInitialized()
    {
        if (initialized) { Instance = this; return; }
        Instance = this;
        if (Config == null)
        {
            Config = ScriptableObject.CreateInstance<SiegeArenaConfig>();
            ownsConfig = true;
        }
        Build();
        initialized = true;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        foreach (var material in materials) if (material != null) DestroyOwned(material);
        if (ownsConfig && Config != null) DestroyOwned(Config);
        initialized = false;
    }

    void Build()
    {
        if (initialized) return;
        Box("Fundament", new(0,-.45f,0), new(Config.HalfWidth*2+6,.8f,40), new(.085f,.13f,.17f));
        for (int x=-6;x<=6;x++)
            for (int z=-8;z<=9;z++)
                Box("Plyta", new(x*2, -.025f,z*2), new(1.94f,.12f,1.94f),
                    (x+z)%2==0 ? new(.22f,.29f,.31f) : new(.245f,.315f,.33f));
        for (int side=-1;side<=1;side+=2)
        {
            Box("Mur boczny", new(side*(Config.HalfWidth+1),.7f,0),new(1.2f,1.4f,35),stone);
            for (int z=-15;z<18;z+=4)
                Box("Blanki",new(side*(Config.HalfWidth+1),1.6f,z),new(1.4f,.65f,1.2f),trim);
            Box("Mur bramny",new(side*9,1.1f,Config.GateZ),new(10,2.2f,1.5f),stone);
            Box("Wieza bramna",new(side*4.5f,1.8f,Config.GateZ),new(2,3.6f,2.5f),trim);
            Box("Korona wiezy",new(side*4.5f,3.7f,Config.GateZ),new(2.6f,.45f,3),stone);
            Box("Swiatlo bramy",new(side*4.5f,4,Config.GateZ),new(.6f,.2f,1.2f),new(.95f,.59f,.19f));
        }
        var gateObject = Box("BRAMA",new(0,1.3f,Config.GateZ),new(7,2.6f,1.2f),new(.50f,.27f,.12f),true);
        gate = gateObject.AddComponent<Health>(); gate.Configure(Config.GateHealth);
        gateObject.AddComponent<ForcedMovementResistanceProfile>().Configure(ForcedMovementResistanceCategory.Structure);
        var coreObject = Box("SERCE TWIERDZY",new(0,1,Config.InnerDepth.x - 2),new(4,2,3),new(.12f,.64f,.71f),true);
        coreObject.AddComponent<BaseCore>();
        core = coreObject.AddComponent<Health>(); core.Configure(Config.CoreHealth);
        coreObject.AddComponent<ForcedMovementResistanceProfile>().Configure(ForcedMovementResistanceCategory.Structure);
        Box("Podest rdzenia",new(0,.2f,Config.InnerDepth.x - 2),new(6,.4f,4),trim);
        Box("Krysztal",new(0,2.5f,Config.InnerDepth.x - 2),new(1.2f,1.8f,1.2f),new(.24f,.91f,1));
        entranceRoot=new GameObject("Wloty szturmu").transform; entranceRoot.SetParent(transform);
        for(int i=0;i<3;i++)
        {
            float x=(i-1)*9;
            for(int side=-1;side<=1;side+=2)
            {
                var pillar=Box("Filar wlotu",new(x+side*2.8f,1.3f,18),new(.8f,2.6f,1.4f),stone);
                pillar.transform.SetParent(entranceRoot,true);
            }
            var strip=Box("Sygnal natarcia",new(x,.12f,17),new(4.4f,.16f,.5f),new(.28f,.44f,.48f),false,true);
            strip.transform.SetParent(entranceRoot,true); entrances.Add(strip.GetComponent<Renderer>());
        }
        playArea=GetComponent<MapPlayArea>() ?? gameObject.AddComponent<MapPlayArea>();
        ApplyBounds();
    }

    GameObject Box(string label,Vector3 pos,Vector3 size,Color color,bool solid=false,bool uniqueMaterial=false)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=label;
        go.transform.SetParent(transform,false);go.transform.position=pos;go.transform.localScale=size;
        var shader=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material material;
        if (!uniqueMaterial && materialCache.TryGetValue(color, out var cached)) material = cached;
        else
        {
            material=new Material(shader); material.color=color; materials.Add(material);
            if (!uniqueMaterial) materialCache[color] = material;
        }
        go.GetComponent<Renderer>().sharedMaterial=material;
        if(!solid) { var col=go.GetComponent<Collider>();col.enabled=false;DestroyOwned(col); }
        return go;
    }

    static void DestroyOwned(Object obj)
    {
        if (obj == null) return;
        if (Application.isPlaying) Destroy(obj);
        else DestroyImmediate(obj);
    }

    public void CompleteRetreat()
    {
        if(IsInner)return;
        IsInner=true;
        if (Gate != null)
        {
            foreach(var col in Gate.GetComponents<Collider>()) if (col != null) col.enabled=false;
            var gateRenderer = Gate.GetComponent<Renderer>();
            if (gateRenderer != null) gateRenderer.enabled=false;
        }
        if (entranceRoot != null) entranceRoot.position+=Vector3.back*17;
        ApplyBounds();
        foreach(var p in FindObjectsByType<PlayerCharacter>())
            p.transform.position=GetPlayerSpawn(p.PlayerIndex);
        foreach(var enemy in EnemyRegistry.Active)
        {
            if(enemy==null)continue;
            var pos=enemy.transform.position;
            pos.z=Mathf.Clamp(pos.z-10,Depth.x,Depth.y);
            enemy.transform.position=ClampEnemy(pos);
        }
        foreach(var bomb in FindObjectsByType<Deployable>())
            DestroyOwned(bomb.gameObject);
    }

    void ApplyBounds()=>playArea.SetBounds(new(-Config.HalfWidth,Config.HalfWidth),Depth);
    public bool Contains(Vector3 p)=>Mathf.Abs(p.x)<=Config.HalfWidth && p.z>=Depth.x && p.z<=Depth.y;
    public Vector3 ClampEnemy(Vector3 p)=>new(Mathf.Clamp(p.x,-Config.HalfWidth,Config.HalfWidth),p.y,Mathf.Clamp(p.z,Depth.x,Depth.y));
    public Vector3 GetEnemyDestination(Vector3 fromPosition)
    {
        var target=DefenseTarget as Component;
        float targetZ=target != null ? target.transform.position.z+1.3f : Depth.x;
        return new(Mathf.Clamp(fromPosition.x,-2.8f,2.8f),fromPosition.y,Mathf.Clamp(targetZ,Depth.x,Depth.y));
    }
    public Vector3 GetSpawnPosition(AttackLineId lane,int index)
    {
        float x=lane==AttackLineId.Left?-9:lane==AttackLineId.Right?9:0;
        return new(x+((index%7)-3)*.65f,1,Depth.y+1.5f+(index%3)*.45f);
    }
    public Vector3 GetPlayerSpawn(int index)=>new((index%4-1.5f)*2.3f,1,Depth.x+4);
    public void SetEntranceWarning(AttackLineId lane,bool on)
    {
        int i=lane==AttackLineId.Left?0:lane==AttackLineId.Right?2:1;
        if(i<entrances.Count)entrances[i].sharedMaterial.color=on?new(1,.35f,.13f):new(.28f,.44f,.48f);
    }
}
