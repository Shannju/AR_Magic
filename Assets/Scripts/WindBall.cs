using UnityEngine;

public class WindBall : MagicBall
{
    protected override void Start()
    {
        base.Start(); // 调用基类的 Start 方法，设置速度等
    }
    protected override void Update()
    {
        if (isMoving)
        {
            Grow();
        }

    }
    public void StartMoving()
    {
        isMoving = true;
        Rb.linearVelocity = transform.up * Speed;
    }
}