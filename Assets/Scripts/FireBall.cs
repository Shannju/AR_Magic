using UnityEngine;
using System;

public class FireBall : MagicBall
{
    protected override void Start()
    {
        base.Start();

        // 监听广播的 OnIncreaseSignal
        SignalBroadcaster.OnIncreaseSignal += HandleIncreaseSignal;
    }

    protected override void Update()
    {
        base.Update();     // 保证父类 Update 在运行（处理 Grow()）
    }

    // 收到增加信号后执行增长
    private void HandleIncreaseSignal()
    {
        Debug.Log("FireBall received OnIncreaseSignal → BeginGrowth()");
        BeginGrowth();
    }

    /// <summary>
    /// 🔥 FireBall 只有当 Scale > 0.1 时才允许播放特效
    /// 我们通过 override 来控制是否调用基类的特效逻辑。
    /// </summary>
    protected override void OnCollisionEnter(Collision collision)
    {
        // 当前大小
        float currentScale = transform.localScale.x;

        // Debug 展示当前缩放
        Debug.Log($"FireBall Collision — Current Scale: {currentScale}");

        // 如果太小，不播放特效，不销毁，不调用基类破坏逻辑
        if (currentScale <= 0.1f)
        {
            Debug.Log("FireBall scale ≤ 0.1 → No VFX will play.");
            return;
        }

        // 如果大小符合要求 → 正常执行 MagicBall 的逻辑（包含播放 VFX）
        base.OnCollisionEnter(collision);
    }


    private void OnDestroy()
    {
        // 取消订阅，防止内存泄露
        SignalBroadcaster.OnIncreaseSignal -= HandleIncreaseSignal;
    }
}

