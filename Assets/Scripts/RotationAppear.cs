using UnityEngine;

public class RotationAppear : MonoBehaviour
{
    public float maxSpeed = 360f;      // 初始速度（度/秒）
    public float rotateDuration = 3f;  // 旋转持续时间
    private float timer = 0f;
    private bool isRotating = true;

    void OnEnable()
    {
        timer = 0f;
        isRotating = true;
    }

    void Update()
    {
        if (!isRotating) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / rotateDuration);

        // ✅ 速度从快到慢（减速停止）
        float currentSpeed = Mathf.Lerp(maxSpeed, 0f, Mathf.SmoothStep(0f, 1f, t));

        // ✅ ✅ ✅ 改为绕 Y 轴旋转（这个就是第三个方向）
        transform.Rotate(0f, currentSpeed * Time.deltaTime, 0f);

        if (timer >= rotateDuration)
            isRotating = false;
    }
}

