using UnityEngine;

public class DualPositionController : MonoBehaviour
{
    [Header("相对父物体的局部位置 (Local Position)")]
    public Vector3 position1 = new Vector3(0, 1, 0);
    public Vector3 position2 = new Vector3(0, 2, 0);

    [Header("统一缩放系数 (应用到 X,Y,Z)")]
    public float scale1 = 1f;
    public float scale2 = 1f;

    [Header("局部旋转 (欧拉角/度)")]
    public Vector3 rotation1 = new Vector3(0, 0, 0);
    public Vector3 rotation2 = new Vector3(0, 0, 0);

    // ✅ 切换到位置1（相对于父物体）
    public void MoveToPosition1()
    {
        transform.localPosition = position1;
        transform.localScale = Vector3.one * scale1;
        transform.localEulerAngles = rotation1;               // 用欧拉角设置
        // 或者：transform.localRotation = Quaternion.Euler(rotation1);
        gameObject.SetActive(true);
    }

    // ✅ 切换到位置1，但覆盖 Z 轴旋转（其余按 rotation1 的 X/Y）
    public void MoveToPosition1(float zRotation)
    {
        transform.localPosition = position1;
        transform.localScale = Vector3.one * scale1;
        transform.localEulerAngles = rotation1;
        gameObject.SetActive(true);
    }

    // ✅ 切换到位置2（相对于父物体）
    public void MoveToPosition2()
    {
        transform.localPosition = position2;
        transform.localScale = Vector3.one * scale2;
        transform.localEulerAngles = rotation2;               // 用欧拉角设置
        gameObject.SetActive(true);
    }

    // ✅ 隐藏物体
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // ✅ 显示物体
    public void Show()
    {
        gameObject.SetActive(true);
    }
}
