using UnityEngine;

public class IceBurstSpeedControl : MonoBehaviour
{
    public Animator animator;  // 绑定Animator组件
    [Range(0.1f, 5f)] public float speed = 2.0f;  // 可在Inspector中调节速度

    void Start()
    {
        // 自动获取Animator组件
        if (animator == null)
            animator = GetComponent<Animator>();

        // 设置播放速度
        if (animator != null)
            animator.speed = speed;
    }
}
