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
    public float Speed = 30f;

    [Tooltip("碰撞发生后到触发事件并销毁之间等待的时间。")]
    public float DelayBeforeDestroy = 2.5f;

    private bool isCollisionHandled = false;
    private GameObject collidedTarget = null;
    protected bool isMoving = false;

    // 事件委托
    public delegate void MagicBallCollisionEvent(object sender, MagicBallCollisionEventArgs e);
    public event MagicBallCollisionEvent OnMagicBallCollision;

    // 🔧 现在只有整体缩放相关参数
    public float minSize = 1f;
    public float maxSize = 2f;
    public float growthSpeed = 0.5f;

    private bool isGrowing = false;

    protected virtual void Start()
    {
        isCollisionHandled = false;
    }

    // ------------------------------
    //         碰撞处理
    // ------------------------------
    protected virtual void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[MagicBall Collision] {gameObject.name} hit: {collision.collider.name}, Tag = {collision.collider.tag}");

        if (isCollisionHandled) return;

        if (collision.collider.CompareTag("Wand"))
            return;

        if (collision.collider.name == "DestructibleMeshSegment")
        {
            isCollisionHandled = true;
            StopMoving();

            collidedTarget = collision.gameObject;

            StartCoroutine(DelayEventAndDestroySelf());
        }
    }

    protected virtual void RaiseCollisionEvent(GameObject hitObject)
    {
        var args = new MagicBallCollisionEventArgs(hitObject);
        OnMagicBallCollision?.Invoke(this, args);
    }

    private IEnumerator DelayEventAndDestroySelf()
    {
        yield return new WaitForSeconds(DelayBeforeDestroy);

        if (collidedTarget != null)
        {
            Debug.Log($"Delayed collision event triggered for {collidedTarget.name} after {DelayBeforeDestroy}s.");
            RaiseCollisionEvent(collidedTarget);
        }

        Debug.Log("MagicBall self-destructed.");
        Destroy(gameObject);
    }

    // ------------------------------
    //         运动控制
    // ------------------------------
    public void StartMoving()
    {
        isMoving = true;
        Rb.linearVelocity = transform.up * Speed;
    }

    public void StopMoving()
    {
        isMoving = false;
        Rb.isKinematic = true;
        Rb.linearVelocity = Vector3.zero;
    }

    // ------------------------------
    //         大小变化（简化版）
    // ------------------------------
    public void BeginGrowth()
    {
        isGrowing = true;
    }

    public void StopGrowth()
    {
        isGrowing = false;
    }

    protected virtual void Update()
    {
        Grow();
    }

    protected  void Grow()
    {
        if (!isGrowing) return;

        // ⭐ 只缩放这个物体本身，不再管 mesh / collider
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            Vector3.one * maxSize,
            growthSpeed * Time.deltaTime
        );

        // 达到最大 → 停止
        if (transform.localScale.x >= maxSize * 0.98f)
        {
            transform.localScale = Vector3.one * maxSize;
            isGrowing = false;

            Debug.Log("MagicBall reached max size.");
        }
    }
}
