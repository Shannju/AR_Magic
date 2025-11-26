using System.Collections.Generic;
using UnityEngine;

public class Magic : MonoBehaviour
{
    public GameObject Wand;
    public GameObject FireBall;
    public GameObject IceBall;
    public GameObject WoodBall;
    public Transform FirePoint;
    
    [Header("Manual Finger Tracking")]
    [Tooltip("左手大拇指指尖位置")]
    public Transform leftThumbTip;
    [Tooltip("左手无名指指尖位置")]
    public Transform leftRingFingerTip;
    [Tooltip("手指接触的距离阈值（米）")]
    public float fingerTouchThreshold = 0.02f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Tooltip("�ڴ�ʱ�䴰�����ۼ���ת������ֵ���ȣ��򴥷�����")]
    public float thresholdAngle = 90f;
    [Tooltip("���ʱ�䴰�ڣ��룩")]
    public float windowSeconds = 1f;
    [Tooltip("��������ȴʱ�䣨�룩����ֹ��ʱ�����ظ�������")]
    public float cooldown = 0.5f;

    struct RotationSample { public float time; public float angle; }

    private Queue<RotationSample> samples = new Queue<RotationSample>();
    private float accumulatedAngle = 0f;
    private Quaternion previousRotation = Quaternion.identity;
    private float nextAllowedTime = 0f;
    
    // 手部追踪相关变量
    private bool isLeftFingersTouching = false;

    void Start()
    {
        if (Wand != null) previousRotation = Wand.transform.rotation;
    }

    void Update()
    {
        if (Wand == null || FireBall == null || FirePoint == null) return;

        // 检测左手大拇指和无名指是否接触
        CheckLeftFingersTouching();

        float now = Time.time;

        // ���㱾֡����һ֡����ת��ȣ�
        Quaternion current = Wand.transform.rotation;
        float delta = Quaternion.Angle(previousRotation, current);
        previousRotation = current;

        // ��¼�������ۼ�
        if (delta > 0f)
        {
            samples.Enqueue(new RotationSample { time = now, angle = delta });
            accumulatedAngle += delta;
        }

        // ��������ʱ�䴰�ڵ�����
        while (samples.Count > 0 && now - samples.Peek().time > windowSeconds)
        {
            var old = samples.Dequeue();
            accumulatedAngle -= old.angle;
        }

        // ���ۼƽǶȳ�����ֵ���Ҳ�����ȴ��ʱ���� FireBall
        if (accumulatedAngle >= thresholdAngle && now >= nextAllowedTime)
        {
            // 根据左手手指接触状态选择法术类型
            GameObject spellToCast = isLeftFingersTouching ? IceBall : FireBall;
            if (spellToCast != null)
            {
                Instantiate(spellToCast, FirePoint.position, FirePoint.rotation);
            }
            samples.Clear();
            accumulatedAngle = 0f;
            nextAllowedTime = now + cooldown;
        }
    }
    
    /// <summary>
    /// 检测左手大拇指和无名指是否接触
    /// </summary>
    private void CheckLeftFingersTouching()
    {
        // 检查手动绑定的Transform是否存在
        if (leftThumbTip == null || leftRingFingerTip == null) 
        {
            isLeftFingersTouching = false;
            return;
        }
        
        // 获取大拇指和无名指的指尖位置
        Vector3 thumbTip = leftThumbTip.position;
        Vector3 ringFingerTip = leftRingFingerTip.position;
        
        // 计算两个指尖之间的距离
        float distance = Vector3.Distance(thumbTip, ringFingerTip);
        
        // 如果距离小于阈值，认为手指接触
        isLeftFingersTouching = distance < fingerTouchThreshold;
    }
}
