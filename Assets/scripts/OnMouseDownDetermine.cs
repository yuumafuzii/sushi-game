using UnityEngine;

public class OnMouseDownDetermine : MonoBehaviour
{
    [SerializeField] private OrderManager orderManager;

    private void Awake()
    {
        if (orderManager == null)
            orderManager = FindAnyObjectByType<OrderManager>();
    }

    /// <summary>
    /// OnMouseDownRayCraftから受け取ったタグで注文UIを判定します。
    /// </summary>
    public bool CheckClickedTag(string tagName)
    {
        if (orderManager == null)
        {
            Debug.LogError("OrderManagerが見つかりません。", this);
            return false;
        }

        return orderManager.CheckProductTag(tagName);
    }
}
