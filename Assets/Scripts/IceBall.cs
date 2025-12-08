using UnityEngine;
using System.Collections;

public class IceBall : MagicBall
{
    [Header("IceBall Settings")]
    [SerializeField] private GameObject iceEffectPrefab;  // 冰爆炸特效 Prefab
    [SerializeField] private float yThreshold =1f;     // 触地高度阈值
    [SerializeField] private float effectScaleTime = 0.5f; // 放大到最大倍数所需时间

    private Transform iceMeshTransform;   // 冰球自身 Mesh
    private Collider iceCollider;         // 冰球自身 Collider
    private bool hasPlayedEffect = false; // 防止重复触发

    protected override void Start()
    {
        base.Start();
        // 自己再缓存一份引用，和基类的保持一致命名
        iceMeshTransform = transform.Find("Mesh");
        iceCollider = GetComponent<Collider>();
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[IceBall Collision] {gameObject.name} hit: {collision.collider.name}, Tag = {collision.collider.tag}");


        // 忽略魔杖
        if (collision.collider.CompareTag("Wand"))
            return;

        Vector3 contactPoint = collision.GetContact(0).point;

        if (collision.collider.name == "DestructibleMeshSegment")
        {
            //Debug.Log("冰球效果111");
            // 触地点低于阈值：只放冰特效，不破坏 DestructibleMeshSegment
            if (contactPoint.y < yThreshold)
            {
                //Debug.Log("冰球效果");
                hasPlayedEffect = true;
                StopMoving();                        // 停止刚体运动（基类方法）
                PlayIceEffect(contactPoint);         // 调用特效逻辑
                return;                              // 不调用 base.OnCollisionEnter → 不破坏 mesh
            }

            // 其它情况走基类默认逻辑（破坏 DestructibleMeshSegment）
            // 其它情况走基类默认逻辑（破坏 DestructibleMeshSegment）
            base.OnCollisionEnter(collision);
        }

    }

    private void PlayIceEffect(Vector3 spawnPosition)
    {
        StartCoroutine(PlayIceEffectRoutine(spawnPosition));
    }

    private IEnumerator PlayIceEffectRoutine(Vector3 spawnPosition)
    {
        // 把冰球移动到触发位置
        transform.position = spawnPosition;

        // 1. 冰球隐形 + 禁用自身碰撞
        if (iceMeshTransform != null)
            iceMeshTransform.gameObject.SetActive(false);

        if (iceCollider != null)
            iceCollider.enabled = false;

        // 2. 创建冰特效 prefab
        GameObject effectInstance = null;
        if (iceEffectPrefab != null)
        {
            effectInstance = Instantiate(iceEffectPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("[IceBall] iceEffectPrefab 未指定，无法播放冰特效");
            yield break;
        }

        // 3. 缩放动画：从当前大小放大到 maxSize
        Vector3 startScale = effectInstance.transform.localScale;
        Vector3 targetScale = Vector3.one * maxSize;

        float elapsed = 0f;
        while (elapsed < effectScaleTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / effectScaleTime);

            effectInstance.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        // 结束时对齐最大大小
        effectInstance.transform.localScale = targetScale;

        // 4. 参照基类逻辑：等待剩余时间再销毁
        float remain = Mathf.Max(0f, DelayBeforeDestroy - effectScaleTime);
        if (remain > 0f)
            yield return new WaitForSeconds(remain);

        // 5. 清除特效并销毁冰球本体
        Destroy(effectInstance);
        Destroy(gameObject);

        Debug.Log("[IceBall] Ground ice effect finished, self-destruct.");
    }

}
