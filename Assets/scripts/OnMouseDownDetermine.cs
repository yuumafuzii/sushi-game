using UnityEngine;
using UnityEngine.InputSystem;

public class OnMouseDownDetermine : MonoBehaviour
{
    [Header("注文管理")]
    [SerializeField]
    private OrderManager orderManager;

    [Header("カメラ")]
    [SerializeField]
    private Camera targetCamera;

    [Header("Raycast設定")]
    [SerializeField]
    private float maxRayDistance = 1000f;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (orderManager == null)
            orderManager = FindFirstObjectByType<OrderManager>();
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (targetCamera == null || orderManager == null)
            return;

        Ray ray = targetCamera.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hitInfo,
            maxRayDistance))
        {
            return;
        }

        // 商品自身から親方向へ名前を確認し、注文と一致したら判定します
        orderManager.CheckProductObject(hitInfo.collider.gameObject);
    }
}
