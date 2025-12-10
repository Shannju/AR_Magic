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
            Debug.LogWarning("[GenerateLightning] lightningPrefab 未设置！");
            return;
        }

        // ?生成位置 = 自身位置 + (1, 0, 0)
        Vector3 spawnPosition = transform.position + new Vector3(1f, 0f, 0f);

        Instantiate(lightningPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"[GenerateLightning] 闪电已生成在：{spawnPosition}");
    }
}
