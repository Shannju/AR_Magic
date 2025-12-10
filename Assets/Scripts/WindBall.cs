using UnityEngine;

public class WindBall : MagicBall
{
    protected override void Start()
    {
        base.Start(); // ���û���� Start �����������ٶȵ�
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
        Rb.useGravity = false;  // 风球没有重力
        isMoving = true;
        Rb.linearVelocity = transform.up * Speed;
    }
}