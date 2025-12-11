using UnityEngine;
using System.Collections.Generic;

public class AutoDestroyEffect : MonoBehaviour
{
    [SerializeField] private float lifeTime = 2f;  // 特效在多少秒后自动销毁

    // 记录所有碰撞到的 DestructibleMeshSegment
    private HashSet<GameObject> collidedSegments = new HashSet<GameObject>();
    private DestructibleGlobalMeshManager meshManager;

    private void OnEnable()
    {
        // 清空之前的碰撞记录
        collidedSegments.Clear();
        
        // 查找 DestructibleGlobalMeshManager
        meshManager = FindFirstObjectByType<DestructibleGlobalMeshManager>();
        
        if (meshManager == null)
        {
            Debug.LogWarning("[AutoDestroyEffect] 未找到 DestructibleGlobalMeshManager，无法破坏网格段");
        }

        // lifeTime 秒后销毁特效，并在销毁前破坏所有碰撞到的网格段
        Invoke(nameof(DestroyEffectAndSegments), lifeTime);
    }

    /// <summary>
    /// 检测碰撞到 DestructibleMeshSegment 的对象
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name == "DestructibleMeshSegment")
        {
            GameObject segment = collision.gameObject;
            if (segment != null && !collidedSegments.Contains(segment))
            {
                collidedSegments.Add(segment);
                Debug.Log($"[AutoDestroyEffect] 记录碰撞到的网格段: {segment.name}");
            }
        }
    }

    /// <summary>
    /// 检测触发器碰撞到 DestructibleMeshSegment 的对象
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "DestructibleMeshSegment")
        {
            GameObject segment = other.gameObject;
            if (segment != null && !collidedSegments.Contains(segment))
            {
                collidedSegments.Add(segment);
                Debug.Log($"[AutoDestroyEffect] 记录触发器碰撞到的网格段: {segment.name}");
            }
        }
    }

    /// <summary>
    /// 销毁特效并破坏所有碰撞到的网格段
    /// </summary>
    private void DestroyEffectAndSegments()
    {
        // 在销毁前破坏所有记录到的网格段
        if (collidedSegments.Count > 0 && meshManager != null)
        {
            List<GameObject> segmentsList = new List<GameObject>(collidedSegments);
            Debug.Log($"[AutoDestroyEffect] 特效结束，开始破坏 {segmentsList.Count} 个网格段");
            meshManager.DestroySegments(segmentsList);
        }

        // 销毁特效对象
        Destroy(gameObject);
    }
}
