using UnityEngine;

public class AutoDestroyEffect : MonoBehaviour
{
    [SerializeField] private float lifeTime = 2f;  // 特效存在多久后自动销毁

    private void OnEnable()
    {
        // lifeTime 秒后销毁这个特效对象本身
        Destroy(gameObject, lifeTime);
    }
}
