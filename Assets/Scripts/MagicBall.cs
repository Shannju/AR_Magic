using UnityEngine;
using System.Collections;

// 1. 定义事件参数类，用于传递碰撞对象
public class MagicBallCollisionEventArgs : System.EventArgs
{
    public GameObject CollidedObject { get; }

    public MagicBallCollisionEventArgs(GameObject obj)
    {
        CollidedObject = obj;
    }
}

public abstract class MagicBall : MonoBehaviour
{
    public Rigidbody Rb;
    [Range(0, 200)]
    public float Speed = 10f;

    // 🟢 修改点：将延时时间设为 public 字段，以便在 Inspector 中调整
    [Tooltip("碰撞发生后到销毁之间等待的时间。")]
    public float DelayBeforeDestroy = 2.5f;

    protected bool isCollision = false;

    // 2. 改进事件签名：使用标准的 (object sender, EventArgs e) 模式
    public delegate void MagicBallCollisionEvent(object sender, MagicBallCollisionEventArgs e);
    public event MagicBallCollisionEvent OnMagicBallCollision;

    protected virtual void Start()
    {
        // 建议使用 velocity
        Rb.linearVelocity = transform.up * Speed;
        isCollision = false;
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (isCollision) return;

        if (collision.collider.CompareTag("Wand"))
            return;

        Rb.isKinematic = true;
        Rb.linearVelocity = Vector3.zero;
        isCollision = true;

        // 处理碰撞，子类可以扩展
        HandleCollision(collision.gameObject);

        // 开启计时器，稍后再执行销毁逻辑
        StartCoroutine(DelayHit(collision.gameObject));
    }

    // 3. protected 方法：封装事件触发逻辑
    protected virtual void RaiseCollisionEvent(GameObject hitObject)
    {
        MagicBallCollisionEventArgs args = new MagicBallCollisionEventArgs(hitObject);
        // 只有在基类内部才能安全地调用 Invoke
        OnMagicBallCollision?.Invoke(this, args);
    }

    protected virtual void HandleCollision(GameObject hitObject)
    {
        // 基类的默认行为是触发事件通知
        RaiseCollisionEvent(hitObject);
    }

    private IEnumerator DelayHit(GameObject hitObject)
    {
        // 🟢 修改点：使用新的 public 字段 DelayBeforeDestroy
        yield return new WaitForSeconds(DelayBeforeDestroy);

        // 再次通知监听者
        RaiseCollisionEvent(hitObject);

        // 销毁魔法球
        Destroy(gameObject);
    }
}