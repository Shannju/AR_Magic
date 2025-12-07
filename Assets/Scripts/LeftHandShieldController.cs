using UnityEngine;

/// 左手：摊开+掌心向上 ⇒ 平放盾；握拳 ⇒ 竖起防御
public class LeftHandShieldController : MonoBehaviour
{
    [Header("References")]
    public OVRHand leftHand;         // 拖左手 OVRHand
    public OVRSkeleton leftSkeleton; // 拖左手 OVRSkeleton
    public GameObject shieldPrefab;  // 盾牌预制（正面朝 +Z）

    [Header("Placement")]
    public float palmOffset = 0.12f;
    public float defendOffset = 0.05f;
    public float followSmooth = 18f;

    [Header("Gesture thresholds (curl-based)")]
    [Tooltip("平均弯曲度 < openCurl 才算张开（0=完全伸直，1=完全握拳）")]
    [Range(0, 1)] public float openCurl = 0.25f;
    [Tooltip("平均弯曲度 > fistCurl 才算握拳")]
    [Range(0, 1)] public float fistCurl = 0.65f;
    [Tooltip("掌心向上阈值（与世界Up点积）")]
    public float palmUpDot = 0.6f;

    private GameObject activeShield;

    // 关键骨骼
    Transform wrist, index1, index2, index3;
    Transform middle1, middle2, middle3;
    Transform ring1, ring2, ring3;
    Transform pinky1, pinky2, pinky3;

    void Start()
    {
        if (!leftHand) leftHand = GetComponentInParent<OVRHand>();
        if (!leftSkeleton) leftSkeleton = GetComponentInParent<OVRSkeleton>();
        InvokeRepeating(nameof(TryCacheBones), 0.1f, 0.2f);
    }

    void TryCacheBones()
    {
        if (leftSkeleton == null || leftSkeleton.Bones == null || leftSkeleton.Bones.Count == 0) return;

        wrist = Bone(OVRSkeleton.BoneId.Hand_WristRoot);

        index1 = Bone(OVRSkeleton.BoneId.Hand_Index1);
        index2 = Bone(OVRSkeleton.BoneId.Hand_Index2);
        index3 = Bone(OVRSkeleton.BoneId.Hand_Index3);

        middle1 = Bone(OVRSkeleton.BoneId.Hand_Middle1);
        middle2 = Bone(OVRSkeleton.BoneId.Hand_Middle2);
        middle3 = Bone(OVRSkeleton.BoneId.Hand_Middle3);

        ring1 = Bone(OVRSkeleton.BoneId.Hand_Ring1);
        ring2 = Bone(OVRSkeleton.BoneId.Hand_Ring2);
        ring3 = Bone(OVRSkeleton.BoneId.Hand_Ring3);

        pinky1 = Bone(OVRSkeleton.BoneId.Hand_Pinky1);
        pinky2 = Bone(OVRSkeleton.BoneId.Hand_Pinky2);
        pinky3 = Bone(OVRSkeleton.BoneId.Hand_Pinky3);

        if (wrist && index3 && middle3 && ring3 && pinky3) CancelInvoke(nameof(TryCacheBones));
    }

    Transform Bone(OVRSkeleton.BoneId id)
    {
        foreach (var b in leftSkeleton.Bones)
            if (b.Id == id) return b.Transform;
        return null;
    }

    void Update()
    {
        if (!leftHand || !leftHand.IsTracked || wrist == null || index1 == null) { HideShield(); return; }
        if (leftHand.HandConfidence == OVRHand.TrackingConfidence.Low) return;

        // 掌面法线与中心
        var acrossPalm = (index1.position - pinky1.position);
        var wristToMid = (middle1.position - wrist.position);
        var palmNormal = Vector3.Normalize(Vector3.Cross(acrossPalm, wristToMid));
        bool palmUp = Vector3.Dot(palmNormal, Vector3.up) > palmUpDot;

        var palmCenter = Vector3.Lerp(Vector3.Lerp(index1.position, pinky1.position, 0.5f), middle1.position, 0.25f);

        // 计算四指弯曲度：两节骨之间夹角越大 ⇒ 越弯曲（0直 1拳）
        float cIndex = FingerCurl(index1, index2, index3);
        float cMiddle = FingerCurl(middle1, middle2, middle3);
        float cRing = FingerCurl(ring1, ring2, ring3);
        float cPinky = FingerCurl(pinky1, pinky2, pinky3);
        float avgCurl = (cIndex + cMiddle + cRing + cPinky) * 0.25f;

        bool isOpen = avgCurl < openCurl;
        bool isFist = avgCurl > fistCurl;

        // Debug（可注释）
        // Debug.Log($"tracked={leftHand.IsTracked} curl={avgCurl:F2} open={isOpen} fist={isFist} palmUp={palmUp}");

        if (isOpen && palmUp)
        {
            SpawnIfNeeded();
            Vector3 pos = palmCenter + palmNormal * palmOffset;
            Vector3 handFwd = Vector3.Normalize(wristToMid);
            Quaternion rot = Quaternion.LookRotation(handFwd, palmNormal); // 平放：盾的“上方向”=掌法线
            SmoothPlace(pos, rot);
        }
        else if (isFist)
        {
            SpawnIfNeeded();
            Vector3 pos = palmCenter + palmNormal * (palmOffset + defendOffset);
            Quaternion rot = Quaternion.LookRotation(palmNormal, Vector3.up); // 竖起：盾朝外，世界Up竖直
            SmoothPlace(pos, rot);
        }
        else
        {
            // 其它姿势可隐藏/保持
            // HideShield();
        }
    }

    float FingerCurl(Transform b1, Transform b2, Transform b3)
    {
        if (!b1 || !b2 || !b3) return 0f;
        Vector3 v1 = (b2.position - b1.position).normalized;
        Vector3 v2 = (b3.position - b2.position).normalized;
        // 直时 dot≈1，弯曲时 dot→-1；映射到 0..1
        float dot = Mathf.Clamp01((1f - Vector3.Dot(v1, v2)) * 0.5f * 2f); // 简单拉伸到 0..1
        return dot;
    }

    void SpawnIfNeeded()
    {
        if (activeShield || !shieldPrefab) return;
        activeShield = Instantiate(shieldPrefab);
    }

    void HideShield()
    {
        if (activeShield) activeShield.SetActive(false);
    }

    void SmoothPlace(Vector3 pos, Quaternion rot)
    {
        if (!activeShield) return;
        if (!activeShield.activeSelf) activeShield.SetActive(true);

        if (followSmooth > 0f)
        {
            activeShield.transform.position = Vector3.Lerp(activeShield.transform.position, pos, Time.deltaTime * followSmooth);
            activeShield.transform.rotation = Quaternion.Slerp(activeShield.transform.rotation, rot, Time.deltaTime * followSmooth);
        }
        else
            activeShield.transform.SetPositionAndRotation(pos, rot);
    }
}
