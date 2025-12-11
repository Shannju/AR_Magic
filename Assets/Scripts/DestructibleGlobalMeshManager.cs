using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;

public class DestructibleGlobalMeshManager : MonoBehaviour
{
    // Reference to the mesh spawner that generates destructible meshes
    public DestructibleGlobalMeshSpawner meshSpawner;

    private List<GameObject> segments = new List<GameObject>();
    private DestructibleMeshComponent currentComponent;
    public HandMagic handMagic;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Add listener for when a destructible mesh is created
        meshSpawner.OnDestructibleMeshCreated.AddListener(SetupDestructibleComponents);


        if (handMagic != null)
        {
            // 当 HandMagic 铸造任何 MagicBall 时，调用 SetupBallListener 方法
            handMagic.OnBallCast += SetupBallListener;
        }
        else
        {
            Debug.LogError("HandMagic 引用未设置！无法监听魔法球碰撞事件。请检查 Inspector 设置。");
        }
        //MagicBall.OnMagicBallCollision += DestroyMeshSegment; // 监听火球碰撞事件
        //WindBall.OnWindBallCollision += DestroyMeshSegment; // 监听风球碰撞事件
    }

    private void SetupBallListener(MagicBall newBall)
    {
        // 🟢 正确地将方法附加到这个具体的 'newBall' 实例上
        newBall.OnMagicBallCollision += DestroyMeshSegment;
    }

    /// <summary>
    /// 为闪电设置碰撞事件监听，使其能够破坏场景
    /// </summary>
    public void SetupLightningListener(LightningBreakEffect lightningEffect)
    {
        if (lightningEffect != null)
        {
            lightningEffect.OnMagicBallCollision += DestroyMeshSegment;
            Debug.Log("[DestructibleGlobalMeshManager] 已为闪电设置场景破坏事件监听");
        }
    }

    // This method sets up the components for the destructible mesh
    public void SetupDestructibleComponents(DestructibleMeshComponent component)
    {
        currentComponent = component;
        segments.Clear(); // Clear the list before adding new segments

        // Get the segments of the destructible mesh
        component.GetDestructibleMeshSegments(segments);

        // Iterate through each segment and perform setup or initialization
        foreach (var item in segments)
        {
            item.AddComponent<MeshCollider>();
        }
    }

    // 通过传递的GameObject来销毁相应的墙面部分
    private void DestroyMeshSegment(object sender, MagicBallCollisionEventArgs e)
    {
        // 从事件参数中获取被击中的 GameObject
        GameObject collidedObject = e.CollidedObject;

        if (collidedObject == null)
        {
            return;
        }

        // 输出调试信息
        Debug.Log("Collision detected with: " + collidedObject.name);

        // 1. 【优化】直接检查 segments 列表中是否包含被碰撞到的对象
        if (segments.Contains(collidedObject))
        {
            // 输出调试信息，打印销毁的墙面部分
            Debug.Log("Destroying segment: " + collidedObject.name);

            // 2. 调用当前组件的销毁逻辑
            // 假设 currentComponent 已经被正确赋值
            if (currentComponent != null)
            {
                currentComponent.DestroySegment(collidedObject);
            }

            // 3. 【优化】从列表中移除该项
            segments.Remove(collidedObject);
        }
        // 注意：如果 collidedObject 不在 segments 列表中，则不会执行任何操作。
    }

    /// <summary>
    /// 公共方法：破坏单个网格段（供特效等外部脚本调用）
    /// </summary>
    public void DestroySegment(GameObject segment)
    {
        if (segment == null)
        {
            return;
        }

        // 检查 segments 列表中是否包含该对象
        if (segments.Contains(segment))
        {
            Debug.Log("Destroying segment: " + segment.name);

            // 调用当前组件的销毁逻辑
            if (currentComponent != null)
            {
                currentComponent.DestroySegment(segment);
            }

            // 从列表中移除该项
            segments.Remove(segment);
        }
    }

    /// <summary>
    /// 公共方法：破坏多个网格段（供特效等外部脚本调用）
    /// </summary>
    public void DestroySegments(List<GameObject> segmentsToDestroy)
    {
        if (segmentsToDestroy == null || segmentsToDestroy.Count == 0)
        {
            return;
        }

        foreach (GameObject segment in segmentsToDestroy)
        {
            if (segment != null)
            {
                DestroySegment(segment);
            }
        }
    }


    //// 清理事件监听器
    //private void OnDestroy()
    //{
    //    FireBall.OnFireBallCollision -= DestroyMeshSegment; // 移除事件监听
    //}
}
