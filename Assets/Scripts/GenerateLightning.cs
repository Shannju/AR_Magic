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

        Instantiate(lightningPrefab, transform.position, Quaternion.identity);
        Debug.Log($"[GenerateLightning] 闪电已生成在自身位置：{transform.position}");
    }
}
