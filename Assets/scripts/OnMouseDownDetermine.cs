
using UnityEngine;
using UnityEngine.InputSystem;

public class OnMouseDownDetermine : MonoBehaviour
{
    [Header("注文管理")]
    public OrderManager orderManager;

    [Header("正解数")]
    public int correctCount = 0;

    [Header("不正解数")]
    public int incorrectCount = 0;

    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (Camera.main == null || orderManager == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        if (!Physics.Raycast(ray, out RaycastHit hitInfo))
            return;

        // Colliderの付いたオブジェクトのTagを取得
        string clickedTag = hitInfo.collider.tag;

        if (orderManager.RemoveOrder(clickedTag))
        {
            correctCount++;
        }
        else
        {
            incorrectCount++;
        }
    }
}
