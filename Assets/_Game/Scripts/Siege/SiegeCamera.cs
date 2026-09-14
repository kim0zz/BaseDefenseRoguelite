using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SiegeCamera : MonoBehaviour
{
    Camera view;
    bool initialized;
    void Awake(){view=GetComponent<Camera>();view.orthographic=true;view.backgroundColor=new(.045f,.075f,.105f);}
    void LateUpdate()
    {
        var arena=SiegeArena.Instance;if(arena==null)return;
        var config=arena.Config;
        float minDepth=arena.Depth.x;
        float maxDepth=arena.Depth.y;
        // Keep the gate as the focus until the retreat switches arenas.  A dead gate
        // should not make the camera jump out to the much more distant core.
        var defenseTarget=(arena.IsInner ? arena.Core : arena.Gate) as Component;
        float minHeight=0f;
        float maxHeight=4f;
        if (defenseTarget != null)
        {
            var bounds=defenseTarget.GetComponent<Renderer>();
            float zRadius=bounds != null ? bounds.bounds.extents.z : 2.5f;
            float yRadius=bounds != null ? bounds.bounds.extents.y : 2.5f;
            minDepth=Mathf.Min(minDepth,defenseTarget.transform.position.z-zRadius);
            maxDepth=Mathf.Max(maxDepth,defenseTarget.transform.position.z+zRadius);
            minHeight=Mathf.Min(minHeight,defenseTarget.transform.position.y-yRadius);
            maxHeight=Mathf.Max(maxHeight,defenseTarget.transform.position.y+yRadius);
        }
        float tiltRadians=config.CameraTilt*Mathf.Deg2Rad;
        float projectedMin=minDepth*Mathf.Sin(tiltRadians)+minHeight*Mathf.Cos(tiltRadians);
        float projectedMax=maxDepth*Mathf.Sin(tiltRadians)+maxHeight*Mathf.Cos(tiltRadians);
        float projectedHalf=(projectedMax-projectedMin)*.5f;
        // HUDs consume vertical viewport only; horizontal framing remains aspect-based.
        const float safeViewportHeight = .76f;
        float size=Mathf.Max(projectedHalf+config.CameraPadding,
            (config.HalfWidth+2)/Mathf.Max(.6f,view.aspect));
        size = Mathf.Max(4f, size / safeViewportHeight);
        var rotation=Quaternion.Euler(config.CameraTilt,0,0);
        var target=new Vector3(0,(minHeight+maxHeight)*.5f,(minDepth+maxDepth)*.5f)-rotation*Vector3.forward*35;
        if (!initialized)
        {
            initialized = true;
            transform.SetPositionAndRotation(target,rotation);
            view.orthographicSize=size;
            return;
        }
        float blend=1-Mathf.Exp(-config.CameraTransitionSpeed*Time.unscaledDeltaTime);
        transform.SetPositionAndRotation(Vector3.Lerp(transform.position,target,blend),rotation);
        view.orthographicSize=Mathf.Lerp(view.orthographicSize,size,blend);
    }
}
