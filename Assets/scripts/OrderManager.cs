
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderManager : MonoBehaviour
{
    [Header("注文に使うUI用プレハブ")]
    [Tooltip("Imageなど、RectTransformを持つUI用プレハブを登録")]
    public List<GameObject> orderPrefabs = new List<GameObject>();

    [Header("注文の生成設定")]
    [Min(1)]
    public int minOrderCount = 2;

    [Min(1)]
    public int maxOrderCount = 5;

    [Header("注文の表示位置（Inspectorから変更可能）")]
    public Vector2 orderPosition = new Vector2(0f, -20f);

    [Header("注文画像のサイズ")]
    public Vector2 orderItemSize = new Vector2(100f, 100f);

    [Header("注文画像の間隔")]
    public float orderSpacing = 10f;

    private RectTransform orderParent;
    private readonly List<string> currentOrders = new List<string>();
    private readonly List<GameObject> displayedOrders = new List<GameObject>();

    private Canvas orderCanvas;

    private void Start()
    {
        CreateOrderCanvas();

        if (orderPrefabs.Count == 0)
        {
            Debug.LogError("Order Prefabsが空です。UI用プレハブを登録してください。");
            return;
        }

        foreach (GameObject prefab in orderPrefabs)
        {
            if (prefab == null)
            {
                Debug.LogError("Order PrefabsにNoneがあります。空欄を埋めるか削除してください。");
                return;
            }

            if (prefab.GetComponent<RectTransform>() == null)
            {
                Debug.LogError(
                    prefab.name + " にRectTransformがありません。" +
                    "3DモデルではなくUI用プレハブを登録してください。"
                );
                return;
            }
        }

        GenerateOrder();
    }

    private void CreateOrderCanvas()
    {
        GameObject canvasObject = new GameObject(
            "OrderCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        orderCanvas = canvasObject.GetComponent<Canvas>();
        orderCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        orderCanvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject parentObject = new GameObject(
            "OrderParent",
            typeof(RectTransform),
            typeof(HorizontalLayoutGroup)
        );

        parentObject.transform.SetParent(canvasObject.transform, false);

        orderParent = parentObject.GetComponent<RectTransform>();

        // 画面上部中央を基準にする
        orderParent.anchorMin = new Vector2(0.5f, 1f);
        orderParent.anchorMax = new Vector2(0.5f, 1f);
        orderParent.pivot = new Vector2(0.5f, 1f);

        // Inspectorで設定した位置を反映
        orderParent.anchoredPosition = orderPosition;

        HorizontalLayoutGroup layout =
            parentObject.GetComponent<HorizontalLayoutGroup>();

        layout.spacing = orderSpacing;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    private void GenerateOrder()
    {
        ClearOrder();

        if (orderPrefabs.Count == 0)
            return;

        int min = Mathf.Max(1, minOrderCount);
        int max = Mathf.Max(min, maxOrderCount);

        int orderCount = Random.Range(min, max + 1);

        for (int i = 0; i < orderCount; i++)
        {
            GameObject prefab = orderPrefabs[
                Random.Range(0, orderPrefabs.Count)
            ];

            if (prefab == null)
                continue;

            // プレハブ名を商品Tagとして扱う
            string itemTag = prefab.name;

            currentOrders.Add(itemTag);

            GameObject display = Instantiate(prefab, orderParent);
            displayedOrders.Add(display);

            RectTransform itemRect =
                display.GetComponent<RectTransform>();

            itemRect.sizeDelta = orderItemSize;
            itemRect.localScale = Vector3.one;
        }
    }

    public bool RemoveOrder(string itemTag)
    {
        int index = currentOrders.IndexOf(itemTag);

        // 注文にない商品
        if (index < 0)
            return false;

        currentOrders.RemoveAt(index);

        if (index < displayedOrders.Count)
        {
            GameObject display = displayedOrders[index];
            displayedOrders.RemoveAt(index);

            if (display != null)
                Destroy(display);
        }

        // 注文をすべて達成したら次の注文を生成
        if (currentOrders.Count == 0)
        {
            GenerateOrder();
        }

        return true;
    }

    private void ClearOrder()
    {
        currentOrders.Clear();

        foreach (GameObject display in displayedOrders)
        {
            if (display != null)
                Destroy(display);
        }

        displayedOrders.Clear();
    }

    private void OnDestroy()
    {
        if (orderCanvas != null)
            Destroy(orderCanvas.gameObject);
    }
}
