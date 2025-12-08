using UnityEngine;
using System;

public class SignalBroadcaster : MonoBehaviour
{
    // 定义增加信号和落地信号的事件
    public static event Action OnIncreaseSignal;  // 增加信号
    public static event Action OnLandSignal;      // 落地信号

    // 更新方法，可以触发不同的信号
    void Update()
    {
    }

    // 广播增加信号
    public void BroadcastIncreaseSignal()
    {
        OnIncreaseSignal?.Invoke();
    }

    // 广播落地信号
    public void BroadcastLandSignal()
    {
        OnLandSignal?.Invoke();
    }
}
