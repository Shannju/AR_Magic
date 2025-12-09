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
    [Header("Movement")]
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

    [Header("Hit Effect (Optional)")]
    [SerializeField] protected GameObject hitEffectPrefab;  // 通用碰撞特效
    [SerializeField] protected float effectScaleTime = 1f;  // 用来控制协程中等待时间

    // 通用：缩放 & 特效需要的引用
    public float minSize = 1f;
    public float maxSize = 2f;
    public float growthSpeed = 0.5f;

    protected bool isGrowing = false;
    protected bool hasPlayedEffect = false;    // 防止重复播特效
    protected Transform meshTransform;         // 假设子物体名为 "Mesh"
    protected Collider ballCollider;

    protected virtual void Start()
    {
        isCollisionHandled = false;

        // 通用缓存：Mesh + Collider
        meshTransform = transform.Find("Mesh");
        ballCollider = GetComponent<Collider>();
    }

    // ------------------------------
    //         碰撞处理（默认：破坏 DestructibleMeshSegment）
    // ------------------------------
    protected virtual void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[MagicBall Collision] {gameObject.name} hit: {collision.collider.name}, Tag = {collision.collider.tag}");

        if (isCollisionHandled) return;

        // 忽略魔杖
        if (collision.collider.CompareTag("Wand"))
            return;

        if (collision.collider.name == "DestructibleMeshSegment")
        {
            isCollisionHandled = true;
            StopMoving();

            collidedTarget = collision.gameObject;

            // 默认逻辑：延迟触发事件，再删除自身
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
        Rb.useGravity = true;
        isMoving = true;
        Rb.linearVelocity = transform.up * Speed;
    }

    public void StopMoving()
    {
        isMoving = false;
        if (Rb != null)
        {
            Rb.isKinematic = true;
            Rb.linearVelocity = Vector3.zero;
        }
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

    protected void Grow()
    {
        if (!isGrowing) return;

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            Vector3.one * maxSize,
            growthSpeed * Time.deltaTime
        );

        if (transform.localScale.x >= maxSize * 0.98f)
        {
            transform.localScale = Vector3.one * maxSize;
            isGrowing = false;
            Debug.Log("MagicBall reached max size.");
        }
    }

    // ------------------------------
    //     ✅ 通用：播放碰撞特效 + 隐藏球体 + 延迟自毁
    // ------------------------------
    protected void PlayHitEffectAndDestroy(Vector3 spawnPosition)
    {
        // 对外暴露一个简单入口，子类只要调用这一个函数就行
        StartCoroutine(PlayHitEffectRoutine(spawnPosition));
    }

    private IEnumerator PlayHitEffectRoutine(Vector3 spawnPosition)
    {
        hasPlayedEffect = true;

        // 把球移动到特效位置
        transform.position = spawnPosition;

        // 1. 隐藏 Mesh
        if (meshTransform != null)
            meshTransform.gameObject.SetActive(false);

        // 2. 关闭碰撞
        if (ballCollider != null)
            ballCollider.enabled = false;

        // 3. 生成特效（特效的生命周期交给 AutoDestroyEffect 自己管）
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("[MagicBall] hitEffectPrefab 未指定，无法播放碰撞特效");
        }

        // 4. 等待一段时间再销毁自己（这里用 DelayBeforeDestroy 和 effectScaleTime 组合）
        float remain = Mathf.Max(0f, DelayBeforeDestroy - effectScaleTime);
        if (remain > 0f)
            yield return new WaitForSeconds(remain);

        Destroy(gameObject);
        Debug.Log("[MagicBall] Hit effect finished, self-destruct.");
    }
}
