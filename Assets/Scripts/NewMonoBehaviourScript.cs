using UnityEngine;

public class ShieldController : MonoBehaviour
{
    public Transform handTransform; // 手掌位置
    public GameObject shield;       // 盾牌模型
    public float rotateSpeed = 90f; // 旋转速度（度/秒）

    void Update()
    {
        // 假设你有一个手势识别方法
        if (IsHandOpen())
        {
            // 盾牌在手掌上顺时针旋转
            shield.transform.position = handTransform.position;
            shield.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
        }
        else if (IsHandFist())
        {
            // 盾牌竖直显示
            shield.transform.position = handTransform.position;
            shield.transform.rotation = Quaternion.LookRotation(handTransform.up);
        }
    }

    // 伪代码：根据你的手势识别系统实现
    bool IsHandOpen() { /* ... */ return true; }
    bool IsHandFist() { /* ... */ return false; }
}