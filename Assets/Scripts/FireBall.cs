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

    // 收到增加信号后执行增长
    private void HandleIncreaseSignal()
    {
        Debug.Log("FireBall received OnIncreaseSignal → BeginGrowth()");
        BeginGrowth();
    }

    private void OnDestroy()
    {
        // 非常重要！取消订阅，防止报错
        SignalBroadcaster.OnIncreaseSignal -= HandleIncreaseSignal;
    }
}
