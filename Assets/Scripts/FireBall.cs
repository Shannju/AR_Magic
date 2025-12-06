using UnityEngine;

public class FireBall : MagicBall
{
    protected override void Start()
    {
        base.Start(); // 调用基类的 Start 方法，设置速度等
    }

    protected override void HandleCollision(GameObject hitObject)
    {
        // 1. FireBall 特有的碰撞处理逻辑
        if (hitObject.CompareTag("Magic"))
        {
            Destroy(hitObject); // 销毁碰撞到的Magic物体
        }

        // 2. 【修改】调用基类的 protected 触发方法
        //    通过调用 RaiseCollisionEvent 触发事件，而不是直接调用 OnMagicBallCollision?.Invoke(...)
        RaiseCollisionEvent(hitObject);
    }
}