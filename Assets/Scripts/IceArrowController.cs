using UnityEngine;

public class IceArrowController : MonoBehaviour
{
    [Header("Movement")]
    public float rotateSpeed = 360f;   // 自旋速度
    public float moveSpeed = 10f;      // 飞行速度
    public float lifeTime = 3f;        // 存活时间

    [Header("VFX")]
    public ParticleSystem smokeTrail;    // 雾气拖尾
    public ParticleSystem debrisEmitter; // 碎冰
    public ParticleSystem impactBurst;   // 爆裂

    private Vector3 moveDirection;
    private float tick;

    void Start()
    {
        moveDirection = transform.forward;
        if (smokeTrail) smokeTrail.Play(true);
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Rotate(moveDirection, rotateSpeed * Time.deltaTime, Space.World);
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}
