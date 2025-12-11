using UnityEngine;
using System;

public class FireBall : MagicBall
{
    [Header("Explosion Settings")]
    [Tooltip("火球触发爆炸特效的最小大小阈值（scale值）")]
    [Range(0.1f, 3f)]
    public float explosionSizeThreshold = 0.5f;

    protected override void Start()
    {
        base.Start();

        // 监听广播的 OnIncreaseSignal
        SignalBroadcaster.OnIncreaseSignal += HandleIncreaseSignal;
    }

    protected override void Update()
    {
        // 只在 isGrowing 为 true 时才调用 Grow()
        // 这样只有在收到 OnIncreaseSignal 信号后才会增长
        if (isGrowing)
        {
            Grow();
        }
    }

    // 收到增加信号后执行增长
    private void HandleIncreaseSignal()
    {
        Debug.Log("FireBall received OnIncreaseSignal → BeginGrowth()");
        BeginGrowth();
    }

    /// <summary>
    /// 🔥 FireBall 只有当 Scale > explosionSizeThreshold 时才允许播放爆炸特效
    /// 我们通过 override 来控制是否调用基类的特效逻辑。
    /// </summary>
    protected override void OnCollisionEnter(Collision collision)
    {
        if (isCollisionHandled) return;

        // 忽略魔杖
        if (collision.collider.CompareTag("Wand"))
            return;

        // 当前大小
        float currentScale = transform.localScale.x;

        Vector3 contactPoint = collision.GetContact(0).point;


        // ✅ 情况一：达到阈值 —— 触发爆炸特效并破坏墙面
        if (currentScale > explosionSizeThreshold && collision.collider.name == "DestructibleMeshSegment")
        {
            isCollisionHandled = true;
            
            // 从手上脱离
            transform.SetParent(null);

            // 停止运动
            StopMoving();

            // 保存碰撞目标，用于触发破坏事件
            collidedTarget = collision.gameObject;

            // 播放爆炸特效
            PlayHitEffectAndDestroy(contactPoint);
            
            // 🔥 关键修复：触发破坏事件，通知 DestructibleGlobalMeshManager 破坏墙面
            RaiseCollisionEvent(collidedTarget);
            
            return;
        }

        // ✅ 情况二：未达到阈值 —— 按父类默认逻辑（延迟事件 + 自毁）
        base.OnCollisionEnter(collision);

    }


    private void OnDestroy()
    {
        // 取消订阅，防止内存泄露
        SignalBroadcaster.OnIncreaseSignal -= HandleIncreaseSignal;
    }
}

