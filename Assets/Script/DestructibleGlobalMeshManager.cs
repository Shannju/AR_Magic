using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;

public class DestructibleGlobalMeshManager : MonoBehaviour
{
    // Reference to the mesh spawner that generates destructible meshes
    public DestructibleGlobalMeshSpawner meshSpawner;

    private List<GameObject> segments = new List<GameObject>();
    private DestructibleMeshComponent currentComponent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Add listener for when a destructible mesh is created
        meshSpawner.OnDestructibleMeshCreated.AddListener(SetupDestructibleComponents);

        // 只在这里添加监听
        FireBall.OnFireBallCollision += DestroyMeshSegment; // 监听火球碰撞事件
        WindBall.OnWindBallCollision += DestroyMeshSegment; // 监听风球碰撞事件
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
    private void DestroyMeshSegment(GameObject collidedObject)
    {
        // 输出调试信息，打印碰撞到的物体
        Debug.Log("Collision detected with: " + collidedObject.name);

        // 直接通过传递的GameObject销毁墙面部分
        foreach (var segment in segments)
        {
            if (segment == collidedObject)
            {
                // 输出调试信息，打印销毁的墙面部分
                Debug.Log("Destroying segment: " + segment.name);

                currentComponent.DestroySegment(segment); // 立即销毁墙面部分
                break; // 找到对应的墙面部分后跳出循环
            }
        }
    }


    //// 清理事件监听器
    //private void OnDestroy()
    //{
    //    FireBall.OnFireBallCollision -= DestroyMeshSegment; // 移除事件监听
    //}
}
