#if Use_CameraController
using Unity.Cinemachine;

using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using YJJTool;
//using static UnityEngine.InputSystem.InputAction;


public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [ValueDropdown("RayType"), InfoBox("如果地面为统一高度平面，那么相交公式为更优解，如果有高差，那么摄像碰撞更好,如果为射线检测，请确定地面的layer为Ground")]
    public int rayType = 0;
    #region 类型相关
    private IEnumerable RayType = new ValueDropdownList<int>
    {
        {"相交公式计算",0},
        {"射线检测",1 },
    };
    public void ChangeRayType(int type)
    {
        rayType = type;
    }
    #endregion

    
    CinemachineBrain _brain;
    private Camera _mainCamera;
    private EventSystem _eventSystem;
    private int _groundLayer = -1;
    [HorizontalGroup("set"), Required]
    [LabelText("相机控制属性")]
    public CameraSet set;
#if UNITY_EDITOR
    [HorizontalGroup("set"), Button("CreatNew")]
    private void Creat()
    {
        set = CameraSet.CreatNew();
    }
#endif
    public BoxCollider clampBox;
    [Header("地面"),Required]
    public Transform ground;
    public void SetGround(Transform ground)
    {
        this.ground = ground;
    }


    [Header("是否根据高度开启阻塞")]
    public bool openScale = true;
    [ShowIf("openScale")]
    public float minMoveSpeed = 0.1f;


    [LabelText("初始相机"), Required]
    public CameraInfo beginCamera;
    public float inputMoveSpeed = 1;
    public UnityEvent OnMove = new UnityEvent();

    [ReadOnly]
    public bool canMove = false;
    [ReadOnly]
    public Transform currentFocus;
    protected Vector3 groundPosition { get => ground == null ? Vector3.zero : ground.position; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // 缓存组件引用
        _mainCamera = Camera.main;
        _eventSystem = EventSystem.current;
        _groundLayer = LayerMask.NameToLayer("Ground");
    }
    private void Start()
    {
        if (beginCamera != null && CurrentInfo == null)
        {
            ChangeCinemachine(beginCamera);
        }
    }
    public CinemachineBrain Brain
    {
        get
        {
            if (_brain == null)
            {
                if (_mainCamera == null) _mainCamera = Camera.main;
                _brain = _mainCamera.transform.GetOrAddComponent<CinemachineBrain>();
            }
            return _brain;
        }
        set => _brain = value;
    }
    //外部click注册方法 控制镜头
    public void ChangeCinemachine(Unity.Cinemachine.CinemachineCamera camera)
    {
        CinemachineCamera old = (CinemachineCamera)Brain.ActiveVirtualCamera;
        if (old == camera) return;
        if (focusCor != null)
        {
            StopCoroutine(focusCor);
        }
        Debug.LogFormat("切换到{0}", camera.gameObject.name);
        if (old != null)
        {
            old.Priority = 1;
        }
        camera.Priority = 100;
        current = camera;
    }
    /// <summary>
    /// 检查要切换的相机是否同一相机
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public bool CheckSameInfo(CameraInfo info)
    {
        return info == CurrentInfo;
    }
    public void ChangeCinemachine(CameraInfo info)
    {
        SwitchCamera(info, true);
    }

    public void ChangeCinemachineNotReset(CameraInfo info)
    {
        SwitchCamera(info, false);
    }

    #region 相机切换辅助方法
    /// <summary>
    /// 计算相机切换的混合时间
    /// </summary>
    private float CalculateBlendTime(ICinemachineCamera old, CameraInfo info)
    {
        if (old == null || CurrentInfo == info)
        {
            return 0;
        }

        if (Brain.CustomBlends != null)
        {
            return _brain.CustomBlends.GetBlendForVirtualCameras(old.Name, info.vc.name, _brain.DefaultBlend).BlendTime;
        }
        else
        {
            return _brain.DefaultBlend.Time;
        }
    }

    /// <summary>
    /// 切换相机的公共逻辑
    /// </summary>
    private void SwitchCamera(CameraInfo info, bool resetCamera)
    {
        if (focusCor != null)
        {
            StopCoroutine(focusCor);
        }

        if (!info.IsInit) info.Init();
        
        var old = Brain.ActiveVirtualCamera;
        if (CurrentInfo == info && resetCamera) return;

        CurrentInfo?.Leave();  //执行上一个相机离开事件

        float time = CalculateBlendTime(old, info);
        
        Debug.Log($"收到改变相机消息,当前激活相机{old?.Name},上一个:{CurrentInfo},要切换到：{info},时间：{time}", info.gameObject);
        
        info.BeginChange(time);
        
        if (CurrentInfo != null)
        {
            CurrentInfo.vc.Priority = 1;
        }
        info.vc.Priority = 100;

        if (resetCamera)
        {
            info.ResetCamera(); //重置相机初始位置
        }

        if (info.changeMoveProperty)
        {
            set = info.set;
        }
        
        current = info.vc;
        CurrentInfo = info;
        info.ActiveEvent?.Invoke(); //激活事件
        currentFocus = info.focous == null ? currentFocus : info.focous;
        canMove = info.canMove;
    }
    #endregion
    private CameraInfo _currentInfo;
    [ReadOnly,ShowInInspector]
    private CinemachineVirtualCameraBase current;
    float distance;

    #region 运动临时参数
    private float _deltaX;
    private float _deltaY;
    public float RotateX
    {
        get => _deltaX; set
        {
            if (Mathf.Abs(value) > Mathf.Abs(_deltaX))
            {
                _deltaX = value;
            }
        }
    }
    public float RotateY
    {
        get => _deltaY; set
        {
            if (Mathf.Abs(value) > Mathf.Abs(_deltaY))
            {
                _deltaY = value;
            }
        }
    }


    public CameraInfo CurrentInfo { get => _currentInfo; set => _currentInfo = value; }


    #endregion

    bool clickIsOnUI = false;
    bool invokeMoveEvent = false;
    bool needMove = false;
    Vector3 moveTarget;
    Vector3 velocity;
    float lastClickTime = -1;
    bool doubleClick = false;
    Coroutine focusCor;

    private void LateUpdate()
    {
        if (!canMove) return;

        UpdateCameraMovement();
        UpdateUIClickState();
        
        if (clickIsOnUI) return;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        if (Input.touchCount == 0)
        {
            HandleMouseInput();
        }
#endif
        HandleTouchInput();
    }

    #region 输入处理
    /// <summary>
    /// 更新相机移动
    /// </summary>
    private void UpdateCameraMovement()
    {
        invokeMoveEvent = false;
        
        if (Mathf.Abs(RotateX) > 0.01f)
        {
            RotateCameraX(current.transform, RotateX * set.rotateSpeed, currentFocus);
            _deltaX = Mathf.Lerp(_deltaX, 0, set.rotateDamping * Time.deltaTime);
            invokeMoveEvent = true;
        }
        
        if (Mathf.Abs(RotateY) > 0.01f)
        {
            RotateCameraY(current.transform, RotateY * set.rotateSpeed, currentFocus);
            _deltaY = Mathf.Lerp(_deltaY, 0, set.rotateDamping * Time.deltaTime);
            invokeMoveEvent = true;
        }
        
        if (needMove)
        {
            var pos = Vector3.SmoothDamp(current.transform.position, moveTarget, ref velocity, set.moveSmoothTime);
            if ((moveTarget - pos).sqrMagnitude < 0.02f)
            {
                needMove = false;
            }
            else
            {
                current.transform.position = pos;
            }
            invokeMoveEvent = true;
        }
        
        if (invokeMoveEvent)
        {
            OnMove?.Invoke();
            if (focusCor != null)
            {
                StopCoroutine(focusCor);
                focusCor = null;
            }
        }
    }

    /// <summary>
    /// 更新 UI 点击状态
    /// </summary>
    private void UpdateUIClickState()
    {
        if (_eventSystem == null) _eventSystem = EventSystem.current;
        
        if (Input.GetMouseButtonDown(0))
        {
            if (_eventSystem.IsPointerOverGameObject())
            {
                clickIsOnUI = true;
            }
        }
        
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            clickIsOnUI = false;
            doubleClick = false;
        }
        
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (_eventSystem.IsPointerOverGameObject(touch.fingerId))
                {
                    clickIsOnUI = true;
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                clickIsOnUI = false;
            }
        }
    }

    /// <summary>
    /// 处理鼠标输入
    /// </summary>
    private void HandleMouseInput()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
        if (_eventSystem == null) _eventSystem = EventSystem.current;
        
        if (Input.GetMouseButtonDown(0))
        {
            //初始化点击位置 和点击时间
            var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            currentFocus.position = GetIntersectWithLineAndPlane(current.transform.position, ray.direction, Vector3.up, groundPosition);
            var clickTime = Time.realtimeSinceStartup;
            if (clickTime - lastClickTime <= 0.2)
            {
                //双击传送
                doubleClick = true;
                var pos = _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
                var dic = (pos - _mainCamera.transform.position).normalized;
                var jd = GetIntersectWithLineAndPlane(pos, dic, Vector3.up, groundPosition);
                if (focusCor != null)
                {
                    StopCoroutine(focusCor);
                }
                Focus(jd, set.focusTime, set.focusDistance, null);
            }
            lastClickTime = clickTime;
        }
        
        //右键旋转
        if (Input.GetMouseButtonDown(1))
        {
            currentFocus.position = GetIntersectWithLineAndPlane(current.transform.position, current.transform.forward, Vector3.up, groundPosition);
            needMove = false;
        }
        
        //左键按住位移
        if (Input.GetMouseButton(0) && !doubleClick)
        {
            var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            moveTarget = GetIntersectWithLineAndPlane(currentFocus.position, -ray.direction, _mainCamera.transform.forward, _mainCamera.transform.position);
            moveTarget = ClampPos(moveTarget);
            needMove = true;
        }
        
        if (Input.GetMouseButton(1))
        {
            RotateX = Input.GetAxis("Mouse X");
            _deltaX = Mathf.Clamp(_deltaX, -10, 10);
            RotateY = Input.GetAxis("Mouse Y");
            _deltaY = Mathf.Clamp(_deltaY, -2, 2);
        }
        
        if (!_eventSystem.IsPointerOverGameObject())
        {
            MoveCamera(Input.GetAxis("Mouse ScrollWheel"));
        }
        else
        {
            MoveCamera(0);
        }
    }

    /// <summary>
    /// 处理触摸输入
    /// </summary>
    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            var t = Input.touches[0];
            if (t.phase == TouchPhase.Moved)
            {
                RotateX = t.deltaPosition.x * 0.02f;
                RotateY = t.deltaPosition.y * 0.02f;
                Debug.Log("touch rotate");
            }
            else if (t.phase == TouchPhase.Began)
            {
                currentFocus.position = GetIntersectWithLineAndPlane(current.transform.position, current.transform.forward, Vector3.up, groundPosition);
            }
        }
        else if (Input.touchCount == 2)
        {
            var t1 = Input.touches[0];
            var t2 = Input.touches[1];
            if (t1.phase == TouchPhase.Began || t2.phase == TouchPhase.Began)
            {
                distance = Vector2.Distance(t1.position, t2.position);
            }
            else
            {
                float currentDistance = Vector2.Distance(t1.position, t2.position);
                MoveCamera((currentDistance - distance) * 0.0025f);
                distance = currentDistance;
            }
        }
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (currentFocus != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(currentFocus.transform.position, 10);
        }

    }
#endif

    #region 相机移动

    private void RotateCameraX(Transform cam, float v, Transform target)
    {
        cam.RotateAround(currentFocus.position, Vector3.up, v);
    }
    private void RotateCameraY(Transform cam, float v, Transform target)
    {
        var oldx = cam.transform.eulerAngles.x;
        var targetX = oldx - v;
        if (targetX < set.minAngle)
        {
            cam.RotateAround(currentFocus.position, cam.right, (set.minAngle - oldx));
            _deltaY = 0;
        }
        else if (targetX > 85)
        {
            cam.RotateAround(currentFocus.position, cam.right, (85 - oldx));
            _deltaY = 0;
        }
        else
        {
            cam.RotateAround(currentFocus.position, cam.right, -v);
        }
    }

    private float scrollValue = 0;

    private Vector3 ClampPos(Vector3 currentPosition)
    {
        if (clampBox == null) return currentPosition;
        var minPosition = clampBox.bounds.min;
        var maxPosition = clampBox.bounds.max;
        float clampedX = Mathf.Clamp(currentPosition.x, minPosition.x, maxPosition.x);
        float clampedY = Mathf.Clamp(currentPosition.y, minPosition.y, maxPosition.y);
        float clampedZ = Mathf.Clamp(currentPosition.z, minPosition.z, maxPosition.z);

        Vector3 clampedPosition = new Vector3(clampedX, clampedY, clampedZ);
        return clampedPosition;
    }


    /// <summary>
    /// 镜头前后移动
    /// </summary>
    /// <param name="value"></param>
    private void MoveCamera(float value)
    {
        if (scrollValue == 0 && value == 0)
        {
            return;
        }
        if (!invokeMoveEvent)
        {
            OnMove?.Invoke();
        }

        // 计算当前相机到焦点的距离
        if (_mainCamera == null) _mainCamera = Camera.main;
        float currentDistance = Vector3.Distance(currentFocus.position, _mainCamera.transform.position);

        // 使用平方根函数创建更平滑的速度曲线
        // 归一化距离（以 nearDistanceThreshold 为基准）
        float normalizedDistance = currentDistance / set.nearDistanceThreshold;
        
        // 使用平方根函数计算速度倍数，提供平滑的衰减曲线
        // 距离越远速度越快，越近速度越慢
        // 限制最大值为1，超过阈值时不再加速
        float speedMultiplier = Mathf.Sqrt(Mathf.Clamp(normalizedDistance, 0.01f, 1f));
        
        //计算最大移动距离  
        var distanceWithCam = currentDistance - set.minDistance;
        if (distanceWithCam < 0)
        {
            distanceWithCam = 0;
        }

        if (value != 0)
        {
            //计算交点
            needMove = false;
            currentFocus.position = GetIntersectWithLineAndPlane(current.transform.position, current.transform.forward, Vector3.up, groundPosition);

            if (focusCor != null)
            {
                StopCoroutine(focusCor);
                focusCor = null;
            }
            scrollValue = openScale ? value * speedMultiplier : value;
        }
        else
        {
            //如果没有输入 逐渐减少位移变量
            if (Mathf.Abs(scrollValue) > 0.002f)
            {
                scrollValue = Mathf.Lerp(scrollValue, 0, Time.deltaTime * set.forwardDamping);
            }
            else
            {
                scrollValue = 0;
            }
        }
        
        // 计算移动方向和距离
        var dir = currentFocus.position - current.transform.position;
        var moveDic = dir.normalized;
        float moveLength;
        
        if (openScale)
        {
            // 根据距离动态调整移动速度
            // speedMultiplier 已经通过平方根函数计算，范围约为 [0.1, 1]
            // 当 normalizedDistance = 0.01 时，speedMultiplier ≈ 0.1（最小速度）
            // 当 normalizedDistance >= 1 时，speedMultiplier = 1（最大速度，不再加速）
            
            float minSpeedMultiplier = Mathf.Sqrt(0.01f); // 最小归一化距离对应的速度倍数 ≈ 0.1
            float maxSpeedMultiplier = 1f; // 最大速度倍数（对应 nearDistanceThreshold）
            
            // 将 speedMultiplier 映射到速度范围 [minMoveSpeed, set.moveSpeed]
            float t = (speedMultiplier - minSpeedMultiplier) / (maxSpeedMultiplier - minSpeedMultiplier);
            float adjustSpeed = Mathf.Lerp(minMoveSpeed, set.moveSpeed, Mathf.Clamp01(t));
            
            moveLength = scrollValue * adjustSpeed;
        }
        else
        {
            moveLength = scrollValue * set.moveSpeed;
        }
       
        moveLength = moveLength > distanceWithCam ? distanceWithCam : moveLength;
        dir = moveDic * moveLength;
        var targetPos = current.transform.position + dir;
        targetPos = ClampPos(targetPos);
        //current.transform.Translate(dir, Space.World);
        current.transform.position = targetPos;
        // currentFocus.Translate(dir, Space.World);
    }

    Coroutine moveCor;
    //public void OnCharctorControll(CallbackContext content)
    //{
    //    var value = content.ReadValue<Vector2>();
    //    if (value == Vector2.zero)
    //    {
    //        if (moveCor != null)
    //        {
    //            StopCoroutine(moveCor);
    //        }
    //    }
    //    else
    //    {
    //        if (moveCor != null)
    //        {
    //            StopCoroutine(moveCor);
    //        }
    //        moveCor = StartCoroutine(InputMove(value));
    //    }

    //}

    IEnumerator InputMove(Vector2 value)
    {

        while (true)
        {
            var translate = current.transform.right * value.x * inputMoveSpeed + current.transform.forward * value.y * inputMoveSpeed;
            current.transform.position += translate;
            yield return null;
        }
    }
    #endregion

    #region Focus逻辑

    /// <summary>
    /// 看向目标位置
    /// </summary>
    /// <param name="target">目标位置</param>
    /// <param name="time">过渡时间</param>
    /// <param name="distance">与目标的距离</param>
    /// <param name="curve">动画曲线</param>
    /// <param name="endPositionAction">结束时的回调</param>
    public void Focus(Vector3 target, float time, float distance, AnimationCurve curve = null, Action<Vector3> endPositionAction = null)
    {
        if (!canMove) return;
        
        StopAllCoroutines();
        ResetMovementParams();
        
        var start = current.transform.position;
        var end = target - current.transform.forward * distance;
        end = ClampPos(end);

        focusCor = this.FadeIn(time, (t) =>
        {
            current.transform.position = Vector3.Lerp(start, end, t);
            OnMove?.Invoke();
        }, () => endPositionAction?.Invoke(end), curve);
    }

    /// <summary>
    /// 看向目标Transform
    /// </summary>
    /// <param name="target">目标Transform</param>
    /// <param name="height">目标高度(可选)</param>
    /// <param name="time">过渡时间(可选)</param>
    /// <param name="distance">与目标的距离(可选)</param>
    /// <param name="curve">动画曲线(可选)</param>
    /// <param name="endPositionAction">结束时的回调(可选)</param>
    public void Focus(Transform target, float? height = null, float? time = null, float? distance = null, AnimationCurve curve = null, Action<Vector3> endPositionAction = null)
    {
        if (!canMove) return;
        Debug.Log($"Focus on {target}", target.gameObject);

        if (height.HasValue)
        {
            FocusWithHeight(target.position, height.Value, time ?? set.focusTime, distance ?? set.focusDistance, curve);
        }
        else
        {
            Focus(target.position, time ?? set.focusTime, distance ?? set.focusDistance, curve, endPositionAction);
        }
    }

    /// <summary>
    /// 指定高度的Focus
    /// </summary>
    private void FocusWithHeight(Vector3 target, float height, float time, float distance, AnimationCurve curve = null)
    {
        StopAllCoroutines();
        ResetMovementParams();
        
        var start = current.transform.position;
        var startRot = current.transform.rotation;
        
        var end = (new Vector3(start.x, target.y, start.z) - target).normalized * distance + target;
        end.y = height;
        end = ClampPos(end);
        
        var endRotate = Quaternion.LookRotation((target - end).normalized);

        focusCor = this.FadeIn(time, (t) =>
        {
            current.transform.position = Vector3.Lerp(start, end, t);
            current.transform.rotation = Quaternion.Lerp(startRot, endRotate, t);
            OnMove?.Invoke();
        }, null, curve);
    }

    /// <summary>
    /// 移动到指定位置和旋转
    /// </summary>
    public void FocusWithEndPosition(Vector3 end, Quaternion rotation, float time)
    {
        if (!canMove) return;
        
        StopAllCoroutines();
        ResetMovementParams();
        
        var start = current.transform.position;
        var startRot = current.transform.rotation;
        
        focusCor = this.FadeIn(time, (t) =>
        {
            current.transform.position = Vector3.Lerp(start, end, t);
            current.transform.rotation = Quaternion.Lerp(startRot, rotation, t);
            OnMove?.Invoke();
        });
    }

    private void ResetMovementParams()
    {
        _deltaX = 0;
        _deltaY = 0;
        needMove = false;
    }

    #endregion

    /// <summary>
    /// 计算直线与平面的交点
    /// </summary>
    /// <param name="point">直线上某一点</param>
    /// <param name="direct">直线的方向</param>
    /// <param name="planeNormal">垂直于平面的的向量</param>
    /// <param name="planePoint">平面上的任意一点</param>
    /// <returns></returns>
    public Vector3 GetIntersectWithLineAndPlane(Vector3 point, Vector3 direct, Vector3 planeNormal, Vector3 planePoint)
    {
        if (rayType == 1)
        {
            if (_groundLayer == -1) _groundLayer = LayerMask.NameToLayer("Ground");
            if (Physics.Raycast(point, direct, out RaycastHit raycast, float.MaxValue, 1 << _groundLayer))
            {
                return raycast.point;
            }
        }
        return MeshUtility.GetIntersectWithLineAndPlane(point, direct, planeNormal, planePoint);
    }

    public bool IsPointInCameraView(Vector3 point)
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
        Vector3 viewportPoint = _mainCamera.WorldToViewportPoint(point);
        // 检查视口坐标是否在[0,1]范围内
        bool inViewport = viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                          viewportPoint.y >= 0 && viewportPoint.y <= 1;
        // 检查点是否在相机前方
        bool inFrontOfCamera = viewportPoint.z > 0;
        return inViewport && inFrontOfCamera;
    }


    public void ResetCamera()
    {
        CurrentInfo?.ResetCamera();
    }


    /// <summary>
    /// 移动到初始位置
    /// </summary>
    public void Move2ResetTransform()
    {
        if (CurrentInfo != null)
        {
            var orignPos = CurrentInfo.StartPos;
            var orignRotation = CurrentInfo.StartRot;

            var beginPos = CurrentInfo.transform.position;
            var beginRotation = CurrentInfo.transform.rotation;
            focusCor = this.FadeIn(set.focusTime, (t) =>
            {
                CurrentInfo.transform.position = Vector3.Lerp(beginPos, orignPos,t);
                CurrentInfo.transform.rotation = Quaternion.Lerp(beginRotation, orignRotation,t);
            });
        }
    }

}
#endif