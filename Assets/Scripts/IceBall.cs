using UnityEngine;
using System.Collections;

public class IceBall : MagicBall
{
    [Header("IceBall Settings")]
    [SerializeField] private GameObject iceEffectPrefab;  // 冰爆炸特效 Prefab
    [SerializeField] private float yThreshold = 1f;       // 触地高度阈值
    [SerializeField] private float effectScaleTime = 1f;  // 可选：用来控制冰球自己多久后消失

    private Transform iceMeshTransform;
    private Collider iceCollider;
    private bool hasPlayedEffect = false; // 防止重复触发

    protected override void Start()
    {
        base.Start();
        iceMeshTransform = transform.Find("Mesh");
        iceCollider = GetComponent<Collider>();
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[IceBall Collision] {gameObject.name} hit: {collision.collider.name}, Tag = {collision.collider.tag}");

        // 已经播过特效 / 处理过，不再理会后续碰撞
        if (hasPlayedEffect)
            return;

        // 忽略魔杖
        if (collision.collider.CompareTag("Wand"))
            return;

        // 不是 DestructibleMeshSegment → 直接交给基类处理
        if (collision.collider.name != "DestructibleMeshSegment")
        {
            base.OnCollisionEnter(collision);
            return;
        }

        // 是 DestructibleMeshSegment，计算接触点
        Vector3 contactPoint = collision.GetContact(0).point;

        // ✅ 情况一：低于阈值，播放地面冰特效
        if (contactPoint.y < yThreshold)
        {
            // 如果你希望冰球从手上“脱离出来”，可以把它从父物体解耦
            transform.SetParent(null);

            // 这句看你设计，如果想同时触发基类的破坏逻辑，可以保留
            base.OnCollisionEnter(collision);

            hasPlayedEffect = true;

            // 停止物理运动
            StopMoving();

            // 立刻禁用碰撞，避免后续多次 OnCollisionEnter
            if (iceCollider != null)
                iceCollider.enabled = false;

            // 播放冰特效（协程）
            PlayIceEffect(contactPoint);
            return;
        }

        // ✅ 情况二：高于阈值，按原本 MagicBall 逻辑处理（比如破坏）
        base.OnCollisionEnter(collision);
    }

    private void PlayIceEffect(Vector3 spawnPosition)
    {
        StartCoroutine(PlayIceEffectRoutine(spawnPosition));
    }

    private IEnumerator PlayIceEffectRoutine(Vector3 spawnPosition)
    {
        // 把冰球移动到触发位置
        transform.position = spawnPosition;

        // 1. 冰球隐形
        if (iceMeshTransform != null)
            iceMeshTransform.gameObject.SetActive(false);

        // 确保碰撞器关闭（双保险）
        if (iceCollider != null)
            iceCollider.enabled = false;

        // 2. 创建冰特效 prefab（交给 prefab 自己的 AutoDestroyEffect 管理寿命）
        if (iceEffectPrefab != null)
        {
            GameObject effectInstance = Instantiate(iceEffectPrefab, spawnPosition, Quaternion.identity);

        }
        else
        {
            Debug.LogWarning("[IceBall] iceEffectPrefab 未指定，无法播放冰特效");
        }

        // 3. 冰球自己等一会儿再销毁（这和特效的 lifeTime 可以一样也可以不一样）
        float remain = Mathf.Max(0f, DelayBeforeDestroy - effectScaleTime);
        if (remain > 0f)
            yield return new WaitForSeconds(remain);

        // 4. 只销毁冰球本体，特效的销毁由 AutoDestroyEffect 自己控制
        Destroy(gameObject);

        Debug.Log("[IceBall] Ground ice effect finished, self-destruct (ball only).");
    }
}
