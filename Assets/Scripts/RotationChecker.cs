using UnityEngine;

public class RotationChecker : MonoBehaviour
{
    public float angleTolerance = 30f; // 容差角度 ±30°
    public DualPositionController shield;  // 拖入盾牌（必须挂 DualPositionController）


    private int currentState = 0; // 状态：0=隐藏, 1=位置1, 2=位置2

    void Update()
    {
        if (shield == null)
        {
            UnityEngine.Debug.LogWarning("[RotationChecker] Shield 未设置！");
            return;
        }

        Vector3 rot = transform.rotation.eulerAngles;
        int newState = GetStateBasedOnRotation(rot);

        if (newState != currentState)
        {
            currentState = newState;

            switch (currentState)
            {
                case 0:
                    shield.Hide();
                    UnityEngine.Debug.Log("🛑 Hidden state");
                    break;

                case 1:
                    shield.Show();
                    shield.MoveToPosition1();

                    UnityEngine.Debug.Log("🔥 Case 1 → Position1 + Z Rotation");
                    break;

                case 2:
                    shield.Show();
                    shield.MoveToPosition2();
                    UnityEngine.Debug.Log("🛡 Case 2 → Position2");
                    break;
            }
        }
    }

    private int GetStateBasedOnRotation(Vector3 rot)
    {
        if (IsAngleInRange(rot.x, 0, angleTolerance) && IsAngleInRange(rot.y, 0, angleTolerance))
            return 1;

        if (IsAngleInRange(rot.x, -90, angleTolerance) && IsAngleInRange(rot.y, 10, angleTolerance))
            return 2;

        return 0;
    }

    private bool IsAngleInRange(float current, float targetAngle, float tolerance)
    {
        float delta = Mathf.DeltaAngle(current, targetAngle);
        return Mathf.Abs(delta) <= tolerance;
    }
}

