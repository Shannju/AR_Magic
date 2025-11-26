using UnityEngine;

public class FireBall : MonoBehaviour
{
    public Rigidbody Rb;
    public bool isCollision;
    [Range(0, 200)]
    public float Speed = 10; // m/s

    // 事件，墙面可以监听
    public delegate void FireBallCollisionEvent(GameObject collidedObject); // 修改为传递 GameObject
    public static event FireBallCollisionEvent OnFireBallCollision;

    void Start()
    {
        Rb.linearVelocity = transform.up * Speed; // 设置火球的初速度
        isCollision = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Wand") && !isCollision)
        {
            Rb.isKinematic = true;
            Rb.linearVelocity = Vector3.zero;
            isCollision = true;

            // Fireball碰撞后通知监听者，并传递碰撞到的墙面对象的 GameObject
            OnFireBallCollision?.Invoke(collision.gameObject);  // 传递碰撞的物体的 GameObject

            // 立即销毁火球
            Destroy(gameObject); // 立即销毁火球
        }
    }
}
