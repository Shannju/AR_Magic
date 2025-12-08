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

    // 延迟时间（Inspector 可配置）
    [Tooltip("碰撞发生后到触发事件并销毁之间等待的时间。")]
    public float DelayBeforeDestroy = 2.5f;

    private bool isCollisionHandled = false;  // 确保只处理一次碰撞
    private GameObject collidedTarget = null; // 存储被击中的目标
    private bool isMoving = false;  // 魔法球是否在运动

    // 事件委托和事件本身
    public delegate void MagicBallCollisionEvent(object sender, MagicBallCollisionEventArgs e);
    public event MagicBallCollisionEvent OnMagicBallCollision;

    // 新增的变量，用于控制魔法球大小变化的范围
    public float minSize = 1f;  // 最小大小
    public float maxSize = 3f;  // 最大大小
    public float minColliderSize = 1f;  // 最小Collider大小
    public float maxColliderSize = 9f;  // 最大Collider大小
    public float growthSpeed = 0.5f;  // 增长速度
    public float shrinkSpeed = 0.2f;  // 缩小速度

    private bool isGrowing = false;  // 是否在增长
    private bool isShrinking = false;  // 是否在缩小

    private Transform meshTransform;  // 子物体的 Transform（挂载网格的物体）
    private Collider magicBallCollider;  // 引用魔法球的 Collider

    protected virtual void Start()
    {
        // 获取子物体的 Transform（假设子物体名称为 "Mesh"）
        meshTransform = transform.Find("Mesh");  // 获取挂载网格的子物体
        magicBallCollider = GetComponent<Collider>();  // 获取物体本身的 Collider

        isCollisionHandled = false;
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        // --- 调试输出 ---
        Debug.Log($"[MagicBall Collision] {gameObject.name} hit: {collision.collider.name}, Tag = {collision.collider.tag}");

        // 确保只处理一次碰撞
        if (isCollisionHandled) return;

        // 忽略与魔杖的碰撞
        if (collision.collider.CompareTag("Wand"))
            return;

        if (collision.collider.name == "DestructibleMeshSegment")
        {
            // 1. 标记碰撞已处理，并停止球的运动
            isCollisionHandled = true;
            StopMoving();

            // 2. 存储被击中的目标，以便协程稍后使用
            collidedTarget = collision.gameObject;

            // 3. 立即启动协程，等待延迟
            // 协程将处理延迟后的事件触发和魔法球销毁
            StartCoroutine(DelayEventAndDestroySelf());
        }


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

    // 新增的逻辑：根据是否在运动控制是否改变大小
    protected virtual void Update()
    {
        if (!isMoving)  // 只有在不运动时才修改大小
        {
            if (isGrowing)
            {
                // 增长逻辑：修改子物体大小
                meshTransform.localScale = Vector3.Lerp(meshTransform.localScale, Vector3.one * maxSize, growthSpeed * Time.deltaTime);
                // 增加Collider的大小
                if (magicBallCollider != null)
                {
                    magicBallCollider.transform.localScale = Vector3.Lerp(magicBallCollider.transform.localScale, Vector3.one * maxColliderSize, growthSpeed * Time.deltaTime);
                }

                // 达到最大值后停止增长
                if (meshTransform.localScale.x >= maxSize)
                {
                    isGrowing = false;
                }
            }
            else if (isShrinking)
            {
                // 缩小逻辑：修改子物体大小
                meshTransform.localScale = Vector3.Lerp(meshTransform.localScale, Vector3.one * minSize, shrinkSpeed * Time.deltaTime);
                // 减少Collider的大小
                if (magicBallCollider != null)
                {
                    magicBallCollider.transform.localScale = Vector3.Lerp(magicBallCollider.transform.localScale, Vector3.one * minColliderSize, shrinkSpeed * Time.deltaTime);
                }

                // 达到最小值后停止缩小
                if (meshTransform.localScale.x <= minSize)
                {
                    isShrinking = false;
                }
            }
        }
    }

    // 开始运动的方法
    public void StartMoving()
    {
        isMoving = true;
        Rb.linearVelocity = transform.up * Speed;  // 使用手部的移动方向和速度
    }


    // 停止运动的方法
    public void StopMoving()
    {
        isMoving = false;
        Rb.isKinematic = true;
        Rb.linearVelocity = Vector3.zero;  // 停止运动
    }
}


