using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLeftCameraTurn : MonoBehaviour
{
    [SerializeField]
    private float maxTurnSpeed = 90f;

    // 0.05なら、画面中央から左へ5%の範囲では停止
    [SerializeField, Range(0f, 0.25f)]
    private float centerDeadZone = 0.05f;

    private void Update()
    {
        if (Mouse.current == null || Screen.width <= 0)
        {
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // ゲーム画面外なら停止
        if (mousePosition.x < 0f || mousePosition.x > Screen.width ||
            mousePosition.y < 0f || mousePosition.y > Screen.height)
        {
            return;
        }

        float normalizedX = mousePosition.x / Screen.width;
        float leftStopPosition = 0.5f - centerDeadZone;

        // 中央付近および右側では停止
        if (normalizedX >= leftStopPosition)
        {
            return;
        }

        // 中央付近では遅く、左端に近づくほど速くする
        float turnStrength =
            Mathf.InverseLerp(leftStopPosition, 0f, normalizedX);

        float turnAmount =
            -maxTurnSpeed * turnStrength * Time.deltaTime;

        transform.Rotate(
            Vector3.up,
            turnAmount,
            Space.World
        );
    }
}