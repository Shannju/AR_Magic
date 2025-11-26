using UnityEngine;

public class WindBall : MonoBehaviour
{
    public Rigidbody Rb;

    [Range(0, 200)]
    public float Speed = 12f; // m/s

    // 事件，其他对象可以监听
    public delegate void WindBallCollisionEvent(GameObject collidedObject);
    public static event WindBallCollisionEvent OnWindBallCollision;

    void Start()
    {
        Rb.linearVelocity = transform.up * Speed; // 设置风球的初速度
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Wand"))
        {
            if (collision.collider.CompareTag("Magic"))
            {
                Destroy(collision.collider.gameObject); // 销毁碰撞到的Magic物体
            }

            // 通知其他对象碰撞发生
            OnWindBallCollision?.Invoke(collision.gameObject); // 传递碰撞到的GameObject

            // 停止风球并销毁它
            Rb.isKinematic = true;
            Rb.linearVelocity = Vector3.zero;
            Rb.detectCollisions = false;
            Destroy(gameObject); // 销毁WindBall
        }
    }
}
