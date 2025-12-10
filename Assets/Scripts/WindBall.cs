using UnityEngine;

public class WindBall : MagicBall
{
    protected override void Start()
    {
        base.Start();

        // 🔥 订阅广播信号
        SignalBroadcaster.OnHandSignal += HandleHandSignal;
    }

    private void OnDestroy()
    {
        // 🔥 取消订阅防止报错
        SignalBroadcaster.OnHandSignal -= HandleHandSignal;
    }

    protected override void Update()
    {
        if (isMoving)
        {
            Grow();
        }
    }

    public override void StartMoving()
    {
        Rb.useGravity = false;  // 风球无重力
        isMoving = true;
        Rb.linearVelocity = transform.up * Speed;
    }

    // 🔥 收到广播后执行：销毁自己
    private void HandleHandSignal()
    {
        Debug.Log("[WindBall] 收到 BroadcastHandSignal → 自动销毁自己");
        Destroy(gameObject);
    }
}
