using UnityEngine;
using System.Collections;

// 定义事件参数类
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

    // 延迟时间（Inspector 可配置）
    [Tooltip("碰撞发生后到触发事件并销毁之间等待的时间。")]
    public float DelayBeforeDestroy = 2.5f;

    private bool isCollisionHandled = false;
    private GameObject collidedTarget = null; // 新增：存储被击中的目标

    // 事件委托和事件本身
    public delegate void MagicBallCollisionEvent(object sender, MagicBallCollisionEventArgs e);
    public event MagicBallCollisionEvent OnMagicBallCollision;

    protected virtual void Start()
    {
        if (Rb != null)
        {
            Rb.linearVelocity = transform.up * Speed;
        }
        isCollisionHandled = false;
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        // 确保只处理一次碰撞
        if (isCollisionHandled) return;

        // 忽略与魔杖的碰撞
        if (collision.collider.CompareTag("Wand"))
            return;

        // 1. 标记碰撞已处理，并停止球的运动
        isCollisionHandled = true;
        if (Rb != null)
        {
            // 立即停止球的运动
            Rb.isKinematic = true;
            Rb.linearVelocity = Vector3.zero;
        }

        // 2. 存储被击中的目标，以便协程稍后使用
        collidedTarget = collision.gameObject;

        // 3. 立即启动协程，等待延迟
        // 协程将处理延迟后的事件触发和魔法球销毁
        StartCoroutine(DelayEventAndDestroySelf());
    }

    /// <summary>
    /// 触发 OnMagicBallCollision 事件，通知监听者。
    /// </summary>
    protected virtual void RaiseCollisionEvent(GameObject hitObject)
    {
        MagicBallCollisionEventArgs args = new MagicBallCollisionEventArgs(hitObject);
        OnMagicBallCollision?.Invoke(this, args);
    }

    /// <summary>
    /// 协程：处理延迟触发事件和魔法球自毁。
    /// </summary>
    private IEnumerator DelayEventAndDestroySelf()
    {
        // 1. 等待指定的延迟时间
        yield return new WaitForSeconds(DelayBeforeDestroy);

        // 2. 延迟时间结束后，先通知外部组件发生了碰撞
        if (collidedTarget != null)
        {
            Debug.Log($"Delayed collision event triggered for {collidedTarget.name} after {DelayBeforeDestroy}s.");
            RaiseCollisionEvent(collidedTarget);
        }

        // 3. 最后销毁魔法球自身
        Debug.Log("MagicBall self-destructed.");
        Destroy(gameObject);
    }
}