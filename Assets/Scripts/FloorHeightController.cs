using UnityEngine;

public class FloorHeightController : MonoBehaviour
{
    [Header("Floor Settings")]
    [Tooltip("Floor对象的Transform引用")]
    public Transform floor;

    /// <summary>
    /// 将floor的Y值设置为自己的Y值
    /// </summary>
    public void SetFloorHeight()
    {
        if (floor != null)
        {
            Vector3 position = floor.position;
            transform.position = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

            floor.position = position;
            Debug.Log($"Floor高度已设置为: {transform.position.y}");
        }
        else
        {
            Debug.LogWarning("Floor引用未设置，无法设置高度！");
        }
    }
}

