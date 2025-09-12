#if Use_CameraController
using Unity.Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(CinemachineCamera))]
public class CameraInfo : MonoBehaviour
{
    [LabelText("镜头是否可操作")]
    public bool canMove = true;
    //用于记录该相机的参数
    [Header("是否改变控制相机的参数")]
    public bool changeMoveProperty = false;
    [HorizontalGroup("set")]
    [ShowIf("@canMove==true&&changeMoveProperty==true")]
    public CameraSet set;
#if UNITY_EDITOR
    [HorizontalGroup("set"), Button("CreatNew"), ShowIf("@canMove==true&&changeMoveProperty==true")]
    private void CreatSet()
    {
        set = CameraSet.CreatNew();
    }
    [OnInspectorInit]
    private void InspectorInit()
    {
        if (set == null)
        {

            set = CameraSet.Instance;
        }
    }
#endif
    [LabelText("镜头focous"), ShowIf("canMove")]
    public Transform focous;
    [LabelText("虚拟相机")]
    public CinemachineCamera vc;

    public UnityEvent ActiveEvent = new UnityEvent();
    public UnityEvent ArriveEvent = new UnityEvent();
    public UnityEvent LeaveEvent = new UnityEvent();

    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 focusPos;
    private static Transform defualtFocus;

    private bool _isInit = false;

    private Coroutine cor;

    public Vector3 StartPos { get => startPos; set => startPos = value; }
    public Quaternion StartRot { get => startRot; set => startRot = value; }
    public bool IsInit { get => _isInit; set => _isInit = value; }
    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        if (defualtFocus == null)
        {
            defualtFocus = new GameObject().transform;
            defualtFocus.parent = transform.parent;
        }
        StartPos = vc.transform.position;
        StartRot = vc.transform.rotation;
        if (focous != null)
        {
            focusPos = focous.position;
        }
        else
        {
            focous = defualtFocus;
        }

        IsInit = true;
    }

    [Button]
    public void ResetCamera()
    {
        if (!_isInit) return;
        vc.transform.position = StartPos;
        vc.transform.rotation = StartRot;
        if (focous != null)
        {
            if(focous == defualtFocus)
            {
                var ground = CameraController.Instance.ground;
                focous.position = CameraController.Instance.GetIntersectWithLineAndPlane(StartPos, vc.transform.forward, ground.forward, ground.position);
            }
            else
            {
                focous.position = focusPos;
            }
        }
    }


    public void BeginChange(float time)
    {
        if (gameObject.activeInHierarchy)
        {
            cor = StartCoroutine(YjjUtility.DeLay(time, () =>
            {
                //Debug.Log($"到达{gameObject.name}");
                ArriveEvent?.Invoke();
            }));
        }
    }
    public void Leave()
    {
        //Debug.Log($"离开镜头{gameObject.name}");
        StopCor();
        LeaveEvent?.Invoke();
    }
    public void StopCor()
    {
        if (cor != null)
        {
            StopCoroutine(cor);
        }
    }

    [OnInspectorInit]
    private void OnInspectorInit()
    {
        if (vc == null)
        {
            vc = GetComponent<Unity.Cinemachine.CinemachineCamera>();
        }
    }
    [Button("切换到该相机")]
    public void GuiChangeCamera()
    {
        CameraController.Instance.ChangeCinemachine(this);
    }

    [Button]
    private void SetFollow(Transform t)
    {
        vc.Follow = t;
        vc.LookAt = t;
    }

}
#endif
