using UnityEngine;
public enum MagicType
{
    Fire,
    Ice,
    Wind
}

public class HandMagic : MonoBehaviour
{
    [Header("Meta ISDK Hand Tracking")]
    [Tooltip("右手OVRHand组件（Meta ISDK手部跟踪）")]
    public OVRHand RightHand;
    [Tooltip("右手OVRSkeleton组件（可选，用于获取骨骼位置）")]
    public OVRSkeleton RightHandSkeleton;
    [Tooltip("Opposition特征阈值（0-1，1表示完全接触）")]
    [Range(0f, 1f)]
    public float oppositionThreshold = 0.8f;

    //public GameObject XRHand_Palm;
    public GameObject OpenXRRightHand;


    [Header("Fire Magic Settings")]
    public Transform Firepoint;
    public GameObject FireBall;
    public GameObject Fire;
    [Header("Ice Magic Settings")]
    public GameObject IceBall;
    public GameObject Ice;
    [Header("Wind Magic Settings")]
    public GameObject WindBall;
    public GameObject Wind;

    [Tooltip("手部移动速度阈值（米/秒）")]
    public float handSpeedThreshold = 2.0f;
    [Tooltip("速度检测时间窗口（秒）")]
    public float speedDetectionWindow = 0.5f;
    [Tooltip("手指弯曲闭合的阈值（0-1，1表示完全闭合）")]
    [Range(0f, 1f)]
    public float fingerCurlThreshold = 0.7f;
    [Tooltip("FireBall生成冷却时间（秒）")]
    public float fireballCooldown = 0.3f;

    // 手势检测相关变量
    private bool isFingersTouching = false;
    private bool wasFingersTouching = false;
    private bool isIceFingersTouching = false;
    private bool wasIceFingersTouching = false;
    private bool isWindFingersTouching = false;
    private bool wasWindFingersTouching = false;
    private bool fireStateActive = false;
    private bool iceStateActive = false;
    private bool windStateActive = false;
    private float lastBallTime = 0f;
    private float lastIceballTime = 0f;
    private float lastWindballTime = 0f;

    // 手部移动速度检测相关变量
    private Vector3 previousHandPosition;
    private float[] speedHistory;
    private int speedHistoryIndex = 0;
    private bool speedHistoryInitialized = false;

    // 当前魔法球实例
    private GameObject currentBall = null;
    private bool isBallCast = false;  // 标记魔法球是否已经生成

    public event System.Action<MagicBall> OnBallCast;

    void Start()
    {
        // 初始化速度历史数组
        int historySize = Mathf.RoundToInt(speedDetectionWindow / Time.fixedDeltaTime);
        speedHistory = new float[historySize];

        // 初始化手部位置
        if (RightHand != null && RightHand.IsTracked)
        {
            previousHandPosition = GetPalmPosition();
        }
    }
    


    void FixedUpdate()
    {
        // 检查手指手势
        CheckFingerGesture();

        // 检查状态解除条件
        CheckStateDeactivation();

        CheckHandSpeedAndShootBall(MagicType.Fire);

        //// 检测手部移动速度并生成魔法球
        //if (fireStateActive)
        //{
        //    CheckHandSpeedAndShootBall(MagicType.Fire);
        //}
        //else if (iceStateActive)
        //{
        //    CheckHandSpeedAndShootBall(MagicType.Ice);
        //}
        //else if (windStateActive)
        //{
        //    CheckHandSpeedAndShootBall(MagicType.Wind);
        //}
    }

    /// <summary>
    /// 检测手指手势（使用Meta ISDK Opposition特征）
    /// </summary>
    private void CheckFingerGesture()
    {
        if (RightHand == null || !RightHand.IsTracked) return;

        // 使用Opposition特征检测拇指与其他手指的对指动作（类似于距离检测）
        // GetFingerPinchStrength返回0-1之间的值，表示手指捏合强度，类似于Opposition特征

        // 检测Fire手势（中指 + 大拇指）
        float middleOpposition = RightHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
        CheckGestureOpposition(middleOpposition, ref isFingersTouching, ref wasFingersTouching, MagicType.Fire);

        // 检测Ice手势（无名指 + 大拇指）
        float ringOpposition = RightHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);
        CheckGestureOpposition(ringOpposition, ref isIceFingersTouching, ref wasIceFingersTouching, MagicType.Ice);

        // 检测Wind手势（小指 + 大拇指）
        float pinkyOpposition = RightHand.GetFingerPinchStrength(OVRHand.HandFinger.Pinky);
        CheckGestureOpposition(pinkyOpposition, ref isWindFingersTouching, ref wasWindFingersTouching, MagicType.Wind);
    }

    /// <summary>
    /// 通用手势检测方法（使用Opposition特征值，替代距离检测）
    /// </summary>
    private void CheckGestureOpposition(float oppositionValue, ref bool isTouching, ref bool wasTouching, MagicType magicType)
    {
        // Opposition值范围0-1，1表示完全接触
        // 使用阈值判断是否达到接触状态
        isTouching = oppositionValue >= oppositionThreshold;

        // 检测从接触状态到分开状态的转换（手势完成）
        if (wasTouching && !isTouching)
        {
            EnableMagicState(magicType);
            // 删除之前的魔法球（如果存在）
            if (currentBall != null)
            {
                Destroy(currentBall);  // 删除之前的魔法球
            }

            // 生成新的魔法球
            GenerateBall(magicType);
        }

        wasTouching = isTouching;
    }

    /// <summary>
    /// 启用魔法状态
    /// </summary>
    private void EnableMagicState(MagicType magicType)
    {
        // 停用其他魔法状态
        if (fireStateActive) SetMagicState(MagicType.Fire, false);
        if (iceStateActive) SetMagicState(MagicType.Ice, false);
        if (windStateActive) SetMagicState(MagicType.Wind, false);

        // 启用指定魔法状态
        SetMagicState(magicType, true);

  

        // 确保手部位置被正确初始化
        if (RightHand != null && RightHand.IsTracked)
        {
            previousHandPosition = GetPalmPosition();
        }
    }

    /// <summary>
    /// 设置魔法状态
    /// </summary>
    private void SetMagicState(MagicType magicType, bool active)
    {
        switch (magicType)
        {
            case MagicType.Fire:
                fireStateActive = active;
                if (Fire != null) Fire.SetActive(active);
                break;
            case MagicType.Ice:
                iceStateActive = active;
                if (Ice != null) Ice.SetActive(active);
                break;
            case MagicType.Wind:
                windStateActive = active;
                if (Wind != null) Wind.SetActive(active);
                break;
        }

        Debug.Log($"{magicType}状态已{(active ? "启用" : "停用")}！");
    }

    /// <summary>
    /// 检查状态解除条件
    /// </summary>
    private void CheckStateDeactivation()
    {
        if (RightHand == null || !RightHand.IsTracked) return;

        if (AreAllFingersClosed())
        {
            if (fireStateActive) SetMagicState(MagicType.Fire, false);
            if (iceStateActive) SetMagicState(MagicType.Ice, false);
            if (windStateActive) SetMagicState(MagicType.Wind, false);
        }
    }

    /// <summary>
    /// 检查所有手指（除大拇指）是否闭合（使用Meta ISDK API）
    /// </summary>
    private bool AreAllFingersClosed()
    {
        if (RightHand == null || !RightHand.IsTracked) return false;

        // 检测除大拇指外的所有手指：食指、中指、无名指、小指
        OVRHand.HandFinger[] fingersToCheck = {
            OVRHand.HandFinger.Index,
            OVRHand.HandFinger.Middle,
            OVRHand.HandFinger.Ring,
            OVRHand.HandFinger.Pinky
        };

        foreach (OVRHand.HandFinger finger in fingersToCheck)
        {
            // 使用GetFingerPinchStrength来检测手指弯曲程度
            if (!IsFingerCurlClosed(finger))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 检测单个手指是否闭合（使用Meta ISDK API）
    /// </summary>
    private bool IsFingerCurlClosed(OVRHand.HandFinger finger)
    {
        if (RightHand == null || !RightHand.IsTracked) return false;

        // 使用GetFingerConfidence结合其他特征
        OVRHand.TrackingConfidence fingerConfidence = RightHand.GetFingerConfidence(finger);
        return fingerConfidence == OVRHand.TrackingConfidence.High;
    }

    /// <summary>
    /// 检测手部移动速度并生成魔法球
    /// </summary>
    private void CheckHandSpeedAndShootBall(MagicType magicType)
    {
        if (RightHand == null || !RightHand.IsTracked || Firepoint == null) return;
        Vector3 currentHandPosition = GetPalmPosition();
        float currentSpeed = Vector3.Distance(previousHandPosition, currentHandPosition) / Time.deltaTime;
        // 计算手部的移动方向
        //Vector3 handMovementDirection = currentHandPosition - previousHandPosition;

        // 过滤异常速度值
        if (currentSpeed < 50f)
        {
            UpdateSpeedHistory(currentSpeed);
        }
        // 计算速度是否达到阈值并且判断冷却时间
        switch (magicType)
        {
            case MagicType.Fire: lastBallTime = this.lastBallTime; break;
            case MagicType.Ice: lastBallTime = this.lastBallTime; break;
            case MagicType.Wind: lastBallTime = this.lastBallTime; break;
        }
        Debug.Log("魔法球发射准备");

        // 魔法球发射
        if (OpenXRRightHand.transform.position.y > 1f)
        {


            if (GetAverageSpeed() > handSpeedThreshold && Time.time - this.lastBallTime >= fireballCooldown)
            {
                Debug.Log("移到根目录");
                currentBall.transform.SetParent(null);
                currentBall.transform.SetPositionAndRotation(Firepoint.transform.position, Firepoint.transform.rotation);


                FireBallProjectile();
                this.lastBallTime = Time.time;
            }
        }



        previousHandPosition = currentHandPosition;
    }

    /// <summary>
    /// 更新速度历史记录
    /// </summary>
    private void UpdateSpeedHistory(float speed)
    {
        speedHistory[speedHistoryIndex] = speed;
        speedHistoryIndex = (speedHistoryIndex + 1) % speedHistory.Length;
        if (!speedHistoryInitialized && speedHistoryIndex == 0) speedHistoryInitialized = true;
    }

    /// <summary>
    /// 获取平均速度
    /// </summary>
    private float GetAverageSpeed()
    {
        if (!speedHistoryInitialized) return 0f;

        float totalSpeed = 0f;
        int validSamples = 0;

        foreach (float speed in speedHistory)
        {
            if (speed > 0f)
            {
                totalSpeed += speed;
                validSamples++;
            }
        }

        return validSamples > 0 ? totalSpeed / validSamples : 0f;
    }
    /// <summary>
    /// 生成魔法球
    /// </summary>
    private void GenerateBall(MagicType magicType)
    {
        GameObject ballPrefab = null;
        string ballName = "";
        switch (magicType)
        {
            case MagicType.Fire: ballPrefab = FireBall; ballName = "FireBall"; break;
            case MagicType.Ice: ballPrefab = IceBall; ballName = "IceBall"; break;
            case MagicType.Wind: ballPrefab = WindBall; ballName = "WindBall"; break;
        }

        if (ballPrefab != null && Firepoint != null)
        {
            // 1. 实例化法球并获取 GameObject 引用
            currentBall = Instantiate(ballPrefab, Firepoint.position, Firepoint.rotation);

            // 2. 尝试获取其基类脚本 MagicBall 的引用
            MagicBall newBallInstance = currentBall.GetComponent<MagicBall>();

            // 2. 将新生成的魔法球设置为当前物体的子物体
            currentBall.transform.SetParent(Firepoint.transform);
            

            if (newBallInstance != null)
            {
                // 3. 【关键】触发 OnBallCast 事件，将新生成的实例传递给监听者
                OnBallCast?.Invoke(newBallInstance);
            }

            Debug.Log($"{ballName}已生成！");
            isBallCast = true; // 标记魔法球已生成
        }
    }

    /// <summary>
    /// 发射已生成的魔法球
    /// </summary>
    private void FireBallProjectile()
    {
        if (currentBall != null)
        {
            // 此处可以添加发射的具体逻辑，比如启动刚体速度、播放特效等
            Debug.Log("魔法球已发射！");
            MagicBall magicBallScript = currentBall.GetComponent<MagicBall>();
            Debug.Log($"魔法球发射，速度：{GetAverageSpeed():F2} 米/秒");
            magicBallScript.StartMoving();
        }
    }

    /// <summary>
    /// 获取手指指尖位置（使用Meta ISDK OVRSkeleton API）
    /// </summary>
    private Vector3 GetFingerTipPosition(OVRHand.HandFinger finger)
    {
        if (RightHand == null || !RightHand.IsTracked)
        {
            return Vector3.zero;
        }

        // 如果提供了OVRSkeleton，使用骨骼信息获取精确位置
        if (RightHandSkeleton != null && RightHandSkeleton.IsInitialized)
        {
            // 根据手指类型获取对应的最后一个骨骼ID（通常是指尖）
            OVRSkeleton.BoneId targetBoneId = OVRSkeleton.BoneId.Hand_Thumb3;
            switch (finger)
            {
                case OVRHand.HandFinger.Thumb:
                    targetBoneId = OVRSkeleton.BoneId.Hand_Thumb3;
                    break;
                case OVRHand.HandFinger.Index:
                    targetBoneId = OVRSkeleton.BoneId.Hand_Index3;
                    break;
                case OVRHand.HandFinger.Middle:
                    targetBoneId = OVRSkeleton.BoneId.Hand_Middle3;
                    break;
                case OVRHand.HandFinger.Ring:
                    targetBoneId = OVRSkeleton.BoneId.Hand_Ring3;
                    break;
                case OVRHand.HandFinger.Pinky:
                    targetBoneId = OVRSkeleton.BoneId.Hand_Pinky3;
                    break;
            }

            // 遍历骨骼列表找到对应的骨骼
            foreach (var bone in RightHandSkeleton.Bones)
            {
                if (bone.Id == targetBoneId)
                {
                    return bone.Transform.position;
                }
            }
        }

        // 如果没有OVRSkeleton或找不到骨骼，使用简化的位置估算
        // 使用手部Transform位置作为近似值
        if (RightHand.transform != null)
        {
            return RightHand.transform.position;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// 获取手掌位置（使用Meta ISDK API）
    /// </summary>
    private Vector3 GetPalmPosition()
    {
        if (RightHand == null || !RightHand.IsTracked)
        {
            return Vector3.zero;
        }

        // 如果提供了OVRSkeleton，尝试获取手腕骨骼位置
        if (RightHandSkeleton != null && RightHandSkeleton.IsInitialized)
        {
            foreach (var bone in RightHandSkeleton.Bones)
            {
                if (bone.Id == OVRSkeleton.BoneId.Hand_WristRoot)
                {
                    return bone.Transform.position;
                }
            }
        }

        // 如果没有OVRSkeleton或找不到骨骼，使用手部Transform的位置
        if (RightHand.transform != null)
        {
            return RightHand.transform.position;
        }

        return Vector3.zero;
    }
}
