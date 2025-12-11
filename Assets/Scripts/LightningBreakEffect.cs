using System.Collections;
using UnityEngine;
using static MagicBall;

public class LightningBreakEffect : MonoBehaviour
{


    protected bool isCollisionHandled = false;
    protected GameObject collidedTarget = null;
    protected bool isMoving = false;

    public float DelayBeforeDestroy = 2.5f;
    public event MagicBallCollisionEvent OnMagicBallCollision;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[MagicBall Collision] {gameObject.name} hit: {collision.collider.name}, Tag = {collision.collider.tag}");

        if (isCollisionHandled) return;

        // ����ħ��
        if (collision.collider.CompareTag("Wand"))
            return;

        if (collision.collider.name == "DestructibleMeshSegment")
        {
            isCollisionHandled = true;

            collidedTarget = collision.gameObject;

            // Ĭ���߼����ӳٴ����¼�����ɾ������
            StartCoroutine(DelayEventAndDestroySelf());
        }
    }
    private IEnumerator DelayEventAndDestroySelf()
    {
        yield return new WaitForSeconds(DelayBeforeDestroy);

        if (collidedTarget != null)
        {
            Debug.Log($"Delayed collision event triggered for {collidedTarget.name} after {DelayBeforeDestroy}s.");
            RaiseCollisionEvent(collidedTarget);
        }

        Debug.Log("MagicBall self-destructed.");
        Destroy(gameObject);
    }
    protected virtual void RaiseCollisionEvent(GameObject hitObject)
    {
        var args = new MagicBallCollisionEventArgs(hitObject);
        OnMagicBallCollision?.Invoke(this, args);
    }

}
