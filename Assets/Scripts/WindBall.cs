using UnityEngine;

public class WindBall : MagicBall
{
    protected override void Start()
    {
        base.Start(); // 调用基类的 Start 方法，设置速度等
    }

    protected override void HandleCollision(GameObject hitObject)
    {
        // 1. WindBall 特有的碰撞处理逻辑
        // 在WindBall中重写碰撞处理逻辑
        if (hitObject.CompareTag("Magic"))
        {
            // 销毁碰撞到的Magic物体
            Destroy(hitObject);
        }
        RaiseCollisionEvent(hitObject);

 
    }
}