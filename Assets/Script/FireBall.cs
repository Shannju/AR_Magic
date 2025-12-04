using UnityEngine;
using System.Collections;

public class FireBall : MonoBehaviour
{
    public Rigidbody Rb;
    public bool isCollision;
    [Range(0, 200)]
    public float Speed = 10; // m/s

    // 事件，墙面可以监听
    public delegate void FireBallCollisionEvent(GameObject collidedObject);
    public static event FireBallCollisionEvent OnFireBallCollision;

    void Start()
    {
        Rb.linearVelocity = transform.up * Speed;
        isCollision = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Wand") && !isCollision)
        {
            Rb.isKinematic = true;
            Rb.linearVelocity = Vector3.zero;
            isCollision = true;

            // 开启计时器，稍后再执行销毁逻辑
            StartCoroutine(DelayHit(collision.gameObject));
        }
    }

    private IEnumerator DelayHit(GameObject hitObject)
    {
        yield return new WaitForSeconds(2.5f);  // 等待时间可调

        // 通知监听者
        OnFireBallCollision?.Invoke(hitObject);

        // 销毁火球
        Destroy(gameObject);
    }
}
