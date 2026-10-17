
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class OrderManager : MonoBehaviour
{
    [Header("注文用UI Prefab（名前＝3D商品のTag）")]
    public List<GameObject> orderPrefabs = new List<GameObject>();

    [Header("注文数")]
    public int orderCount = 3;

    [Header("画像サイズ")]
    public Vector2 imageSize = new Vector2(100, 100);

    [Header("画像間隔")]
    public float spacing = 10f;

    public List<string> orderList = new List<string>();

    private readonly List<GameObject> displayedItems =
        new List<GameObject>();

    private Transform orderParent;

    private void Start()
    {
        CreateOrderCanvas();
        CreateOrder();
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

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject parentObject = new GameObject(
            "OrderParent",
            typeof(RectTransform),
            typeof(HorizontalLayoutGroup)
        );

        orderParent = parentObject.transform;
        orderParent.SetParent(canvasObject.transform, false);

        RectTransform parentRect =
            parentObject.GetComponent<RectTransform>();

        parentRect.anchorMin = new Vector2(0.5f, 1f);
        parentRect.anchorMax = new Vector2(0.5f, 1f);
        parentRect.pivot = new Vector2(0.5f, 1f);
        parentRect.anchoredPosition = new Vector2(0, -20);
        parentRect.sizeDelta = new Vector2(1200, imageSize.y + 20);

        HorizontalLayoutGroup layout =
            parentObject.GetComponent<HorizontalLayoutGroup>();

        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    public void CreateOrder()
    {
        orderList.Clear();

        foreach (GameObject obj in displayedItems)
        {
            if (obj != null)
                Destroy(obj);
        }

        displayedItems.Clear();

        if (orderPrefabs == null || orderPrefabs.Count == 0)
        {
            Debug.LogError(
                "OrderManager: Order PrefabsにPrefabを登録してください。"
            );
            return;
        }

        if (orderCount <= 0)
        {
            Debug.LogError("Order Countは1以上にしてください。");
            return;
        }

        if (orderParent == null)
        {
            Debug.LogError("OrderParentが作成されていません。");
            return;
        }

        for (int i = 0; i < orderCount; i++)
        {
            int index = Random.Range(0, orderPrefabs.Count);
            GameObject prefab = orderPrefabs[index];

            if (prefab == null)
            {
                Debug.LogError(
                    "Order PrefabsのElement " + index +
                    " がNoneです。"
                );
                continue;
            }

            // Prefab名から商品のTagを取得
            string itemTag = prefab.name;

            // UIの親を指定してPrefabを生成
            GameObject display = Instantiate(
                prefab,
                orderParent,
                false
            );

            if (display == null)
            {
                Debug.LogError("注文Prefabの生成に失敗しました。");
                continue;
            }

            RectTransform rect =
                display.GetComponent<RectTransform>();

            if (rect == null)
            {
                Debug.LogError(
                    prefab.name +
                    " のルートにRectTransformがありません。" +
                    "UI用Prefabを登録してください。"
                );

                Destroy(display);
                continue;
            }

            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.sizeDelta = imageSize;

            // 画像が非表示状態なら表示する
            display.SetActive(true);

            orderList.Add(itemTag);
            displayedItems.Add(display);
        }

        Debug.Log(
            "注文生成完了： " + orderList.Count + "個"
        );
    }

    public bool RemoveOrder(string itemTag)
    {
        int index = orderList.IndexOf(itemTag);

        if (index < 0)
            return false;

        orderList.RemoveAt(index);

        if (index < displayedItems.Count)
        {
            if (displayedItems[index] != null)
                Destroy(displayedItems[index]);

            displayedItems.RemoveAt(index);
        }

        if (orderList.Count == 0)
            CreateOrder();

        return true;
    }
}
