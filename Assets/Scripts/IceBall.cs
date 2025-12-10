using UnityEngine;
using System.Collections;

public class IceBall : MagicBall
{
    [Header("IceBall Settings")]
    [SerializeField] private float yThreshold = 1f;  // 触地高度阈值

    // 不再需要自己的 mesh/collider/特效协程，Start 用父类即可

    protected override void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[IceBall Collision] {gameObject.name} hit: {collision.collider.name}, Tag = {collision.collider.tag}");

        // 已经播过特效 / 处理过，不再理会后续碰撞
        if (hasPlayedEffect)
            return;

        // 忽略魔杖
        if (collision.collider.CompareTag("Wand"))
            return;

        // 不是 DestructibleMeshSegment → 用父类默认处理
        if (collision.collider.name != "DestructibleMeshSegment")
        {
            base.OnCollisionEnter(collision);
            return;
        }

        // 是 DestructibleMeshSegment，计算接触点
        Vector3 contactPoint = collision.GetContact(0).point;

        // ✅ 情况一：低于阈值 —— 使用“地面冰特效逻辑”（不走默认 DelayEventAndDestroySelf）
        if (contactPoint.y < yThreshold)
        {
            // 从手上脱离
            transform.SetParent(null);

            // 停止运动
            StopMoving();

            // 这里不再调用 base.OnCollisionEnter，避免再触发一次默认破坏逻辑
            // 直接调用基类封装好的“播放特效 + 冰球自毁”
            PlayHitEffectAndDestroy(contactPoint);
            return;
        }

        // ✅ 情况二：高于阈值 —— 按父类默认逻辑（延迟事件 + 自毁）
        base.OnCollisionEnter(collision);
    }

    // 重写 StartMoving 方法，禁用重力
    public override void StartMoving()
    {
        Rb.useGravity = false;
        isMoving = true;
        Rb.linearVelocity = transform.up * Speed;
    }
}
