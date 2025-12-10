using UnityEngine;

public class GenerateLightning : MonoBehaviour
{
    [SerializeField] private GameObject lightningPrefab;

    private void OnEnable()
    {
        SignalBroadcaster.OnHandSignal += SpawnLightning;
    }

    private void OnDisable()
    {
        SignalBroadcaster.OnHandSignal -= SpawnLightning;
    }

    private void SpawnLightning()
    {
        if (lightningPrefab == null)
        {
            Debug.LogWarning("[GenerateLightning] lightningPrefab 未设置");
            return;
        }

        // 生成位置 = 当前位置 + (1, 0, 0)
        Vector3 spawnPosition = transform.position + new Vector3(1f, 0f, 0f);

        GameObject lightningInstance = Instantiate(lightningPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"[GenerateLightning] 闪电已生成在：{spawnPosition}");

        // 🔥 关键修复：为闪电设置碰撞事件监听，使其能够破坏场景
        SetupLightningCollisionListener(lightningInstance);
    }

    /// <summary>
    /// 为生成的闪电设置碰撞事件监听，使其能够破坏场景
    /// </summary>
    private void SetupLightningCollisionListener(GameObject lightningInstance)
    {
        // 获取闪电上的 LightningBreakEffect 组件
        LightningBreakEffect lightningEffect = lightningInstance.GetComponent<LightningBreakEffect>();
        
        if (lightningEffect == null)
        {
            Debug.LogWarning("[GenerateLightning] 闪电预制体上未找到 LightningBreakEffect 组件");
            return;
        }

        // 查找 DestructibleGlobalMeshManager 并设置事件监听
        DestructibleGlobalMeshManager meshManager = FindFirstObjectByType<DestructibleGlobalMeshManager>();
        
        if (meshManager != null)
        {
            // 使用公共方法设置事件监听
            meshManager.SetupLightningListener(lightningEffect);
            Debug.Log("[GenerateLightning] 已为闪电设置场景破坏事件监听");
        }
        else
        {
            Debug.LogWarning("[GenerateLightning] 未找到 DestructibleGlobalMeshManager，闪电无法破坏场景");
        }
    }
}
