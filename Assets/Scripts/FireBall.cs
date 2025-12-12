using UnityEngine;
using System;
using System.Collections;
public class FireBall : MagicBall
{

    protected override void Start()
    {
        base.Start();

        // 监听广播的 OnIncreaseSignal
        SignalBroadcaster.OnIncreaseSignal += HandleIncreaseSignal;
    }

    protected override void Update()
    {
        // 只在 isGrowing 为 true 时才调用 Grow()
        // 这样只有在收到 OnIncreaseSignal 信号后才会增长
        if (isGrowing)
        {
            Grow();
        }
    }

    // 收到增加信号后执行增长
    private void HandleIncreaseSignal()
    {
        Debug.Log("FireBall received OnIncreaseSignal → BeginGrowth()");
        BeginGrowth();
    }

    /// <summary>
    /// 🔥 FireBall 只有当 Scale > explosionSizeThreshold 时才允许播放爆炸特效
    /// 我们通过 override 来控制是否调用基类的特效逻辑。
    /// </summary>
    protected override void OnCollisionEnter(Collision collision)
    {
        // 当前大小
        float currentScale = transform.localScale.x;

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


        // ✅ 情况一：达到阈值 —— 触发爆炸特效（不走默认 DelayEventAndDestroySelf）
        if (currentScale > minSize && collision.collider.name == "DestructibleMeshSegment")
        {        // 1. 隐藏 Mesh
                meshTransform.gameObject.SetActive(false);
                transform.position = contactPoint;
                Instantiate(hitEffectPrefab, contactPoint, Quaternion.identity);
                base.OnCollisionEnter(collision);

            
            
            // return;
        }

        // ✅ 情况二：高于阈值 —— 按父类默认逻辑（延迟事件 + 自毁）
        base.OnCollisionEnter(collision);

    }


    private void OnDestroy()
    {
        // 取消订阅，防止内存泄露
        SignalBroadcaster.OnIncreaseSignal -= HandleIncreaseSignal;
    }
   
}


