using UnityEngine;

public class AutoDestroyEffect : MonoBehaviour
{
    [SerializeField] private float lifeTime = 2f;  // ��Ч���ڶ�ú��Զ�����

    private void OnEnable()
    {
        // lifeTime ������������Ч������
        Destroy(gameObject, lifeTime);
    }
}
