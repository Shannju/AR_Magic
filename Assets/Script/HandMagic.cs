using UnityEngine;
using OVR;

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
    private float lastFireballTime = 0f;
    private float lastIceballTime = 0f;
	private float lastWindballTime = 0f;
    
    // 手部移动速度检测相关变量
    private Vector3 previousHandPosition;
    private float[] speedHistory;
    private int speedHistoryIndex = 0;
    private bool speedHistoryInitialized = false;
    
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
        
        // 检测手部移动速度并生成魔法球
        if (fireStateActive)
        {
            CheckHandSpeedAndGenerateBall(MagicType.Fire);
        }
        else if (iceStateActive)
        {
            CheckHandSpeedAndGenerateBall(MagicType.Ice);
        }
		else if (windStateActive)
		{
			CheckHandSpeedAndGenerateBall(MagicType.Wind);
		}
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
        
        if (!active) ResetSpeedHistory();
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
            // 当手指闭合时，GetFingerPinchStrength通常接近0（因为大拇指不在位置）
            // 我们需要用另一种方式检测手指闭合
            
            // 方法1：使用手指置信度和关节角度（如果有OVRSkeleton）
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
        
        // 方法1：如果有OVRSkeleton，使用骨骼关节角度检测
        if (RightHandSkeleton != null && RightHandSkeleton.IsInitialized)
        {
            return IsFingerCurlClosedBySkeleton(finger);
        }
        
        // 方法2：使用GetFingerConfidence结合其他特征
        // OVRHand可能没有直接的弯曲度API，使用手指位置估算
        OVRHand.TrackingConfidence fingerConfidence = RightHand.GetFingerConfidence(finger);
        
        // 如果手指跟踪置信度高，检查手指是否弯曲
        // 通过比较手指尖端和根部的距离来估算
        // 简化版本：如果手指置信度高且可能在闭合位置，返回true
        // 这是一个近似方法，更准确的方法需要骨骼数据
        
        // 暂时使用简化检测：如果所有需要检测的手指都有高置信度，
        // 且手指之间的Opposition值很低（说明没有捏合），可能是闭合状态
        // 但这不够准确，优先使用骨骼方法
        
        // TrackingConfidence枚举通常有Low和High值，检查是否为High
        // 如果没有骨骼数据，使用简化检测：置信度高时假设手指可能闭合
        return fingerConfidence == OVRHand.TrackingConfidence.High;
    }
    
    /// <summary>
    /// 通过骨骼检测手指是否闭合（使用OVRSkeleton）
    /// </summary>
    private bool IsFingerCurlClosedBySkeleton(OVRHand.HandFinger finger)
    {
        if (RightHandSkeleton == null || !RightHandSkeleton.IsInitialized) return false;
        
        // 根据手指类型确定需要检查的骨骼
        OVRSkeleton.BoneId[] boneChain;
        
        switch (finger)
        {
            case OVRHand.HandFinger.Index:
                boneChain = new OVRSkeleton.BoneId[] {
                    OVRSkeleton.BoneId.Hand_Index1,
                    OVRSkeleton.BoneId.Hand_Index2,
                    OVRSkeleton.BoneId.Hand_Index3
                };
                break;
            case OVRHand.HandFinger.Middle:
                boneChain = new OVRSkeleton.BoneId[] {
                    OVRSkeleton.BoneId.Hand_Middle1,
                    OVRSkeleton.BoneId.Hand_Middle2,
                    OVRSkeleton.BoneId.Hand_Middle3
                };
                break;
            case OVRHand.HandFinger.Ring:
                boneChain = new OVRSkeleton.BoneId[] {
                    OVRSkeleton.BoneId.Hand_Ring1,
                    OVRSkeleton.BoneId.Hand_Ring2,
                    OVRSkeleton.BoneId.Hand_Ring3
                };
                break;
            case OVRHand.HandFinger.Pinky:
                boneChain = new OVRSkeleton.BoneId[] {
                    OVRSkeleton.BoneId.Hand_Pinky1,
                    OVRSkeleton.BoneId.Hand_Pinky2,
                    OVRSkeleton.BoneId.Hand_Pinky3
                };
                break;
            default:
                return false;
        }
        
        // 获取骨骼Transform
        Transform[] boneTransforms = new Transform[boneChain.Length];
        for (int i = 0; i < boneChain.Length; i++)
        {
            bool found = false;
            foreach (var skeletonBone in RightHandSkeleton.Bones)
            {
                if (skeletonBone.Id == boneChain[i])
                {
                    boneTransforms[i] = skeletonBone.Transform;
                    found = true;
                    break;
                }
            }
            
            if (!found || boneTransforms[i] == null) return false;
        }
        
        // 计算手指弯曲角度
        // 通过计算相邻骨骼之间的角度来判断手指是否闭合
        float totalBendAngle = 0f;
        
        for (int i = 0; i < boneTransforms.Length - 1; i++)
        {
            Vector3 direction1 = (boneTransforms[i + 1].position - boneTransforms[i].position).normalized;
            Vector3 direction2 = i == 0 ? 
                (boneTransforms[0].position - GetPalmPosition()).normalized : 
                (boneTransforms[i].position - boneTransforms[i - 1].position).normalized;
            
            float angle = Vector3.Angle(direction1, direction2);
            totalBendAngle += angle;
        }
        
        // 手指闭合时，关节角度较大（手指弯曲）
        // 阈值可以根据实际情况调整
        float closedAngleThreshold = 120f; // 手指闭合时的总弯曲角度阈值
        
        return totalBendAngle > closedAngleThreshold;
    }
    
    
    /// <summary>
    /// 检测手部移动速度并生成魔法球
    /// </summary>
    private void CheckHandSpeedAndGenerateBall(MagicType magicType)
    {
        if (RightHand == null || !RightHand.IsTracked || Firepoint == null) return;
        
		GameObject ballPrefab = null;
		switch (magicType)
		{
			case MagicType.Fire: ballPrefab = FireBall; break;
			case MagicType.Ice: ballPrefab = IceBall; break;
			case MagicType.Wind: ballPrefab = WindBall; break;
		}
        if (ballPrefab == null) return;
        
        Vector3 currentHandPosition = GetPalmPosition();
        float currentSpeed = Vector3.Distance(previousHandPosition, currentHandPosition) / Time.deltaTime;
        
        // 过滤异常速度值（防止因位置重置导致的异常高速）
        if (currentSpeed < 50f) // 设置合理的最大速度阈值
        {
            UpdateSpeedHistory(currentSpeed);
        }
        
		float lastBallTime = 0f;
		switch (magicType)
		{
			case MagicType.Fire: lastBallTime = lastFireballTime; break;
			case MagicType.Ice: lastBallTime = lastIceballTime; break;
			case MagicType.Wind: lastBallTime = lastWindballTime; break;
		}
        
        if (GetAverageSpeed() > handSpeedThreshold && Time.time - lastBallTime >= fireballCooldown)
        {
			GenerateBall(magicType);
			switch (magicType)
			{
				case MagicType.Fire: lastFireballTime = Time.time; break;
				case MagicType.Ice: lastIceballTime = Time.time; break;
				case MagicType.Wind: lastWindballTime = Time.time; break;
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
    /// 重置速度历史记录
    /// </summary>
    private void ResetSpeedHistory()
    {
        if (speedHistory != null) System.Array.Clear(speedHistory, 0, speedHistory.Length);
        speedHistoryIndex = 0;
        speedHistoryInitialized = false;
        
        // 重置手部位置，防止下次启用时计算异常速度
        if (RightHand != null && RightHand.IsTracked)
        {
            previousHandPosition = GetPalmPosition();
        }
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
            Instantiate(ballPrefab, Firepoint.position, Firepoint.rotation);
            Debug.Log($"{ballName}已生成！");
            SetMagicState(magicType, false); // 生成后立即解除对应状态
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
