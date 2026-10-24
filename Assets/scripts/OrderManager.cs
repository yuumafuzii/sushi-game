using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderManager : MonoBehaviour
{
    [Header("注文用スプライト")]
    [Tooltip("各スプライト名を対応する商品タグ名と一致させてください")]
    public List<Sprite> orderSprites = new List<Sprite>();

    [Header("注文数")]
    [Min(1)]
    public int minOrderCount = 3;

    [Min(1)]
    public int maxOrderCount = 3;

    [Header("注文表示位置")]
    public Vector2 orderPosition = new Vector2(0f, -20f);

    [Header("注文画像サイズ")]
    public Vector2 orderItemSize = new Vector2(100f, 100f);

    [Header("注文画像の間隔")]
    public float orderSpacing = 10f;

    private Canvas orderCanvas;
    private RectTransform orderParent;

    private readonly List<Sprite> currentOrders =
        new List<Sprite>();

    private readonly List<GameObject> displayedOrders =
        new List<GameObject>();

    public int CorrectCount { get; private set; }
    public int IncorrectCount { get; private set; }

    private void Start()
    {
        if (orderSprites == null || orderSprites.Count == 0)
            return;

        foreach (Sprite sprite in orderSprites)
        {
            if (sprite == null)
                return;
        }

        CreateOrderCanvas();
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

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1920f, 1080f);

        scaler.matchWidthOrHeight = 0.5f;

        GameObject parentObject = new GameObject(
            "OrderParent",
            typeof(RectTransform),
            typeof(HorizontalLayoutGroup)
        );

        parentObject.transform.SetParent(
            canvasObject.transform,
            false
        );

        orderParent =
            parentObject.GetComponent<RectTransform>();

        orderParent.anchorMin = new Vector2(0.5f, 1f);
        orderParent.anchorMax = new Vector2(0.5f, 1f);
        orderParent.pivot = new Vector2(0.5f, 1f);
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

        if (orderSprites == null || orderSprites.Count == 0)
            return;

        int min = Mathf.Max(1, minOrderCount);
        int max = Mathf.Max(min, maxOrderCount);

        int orderCount =
            Random.Range(min, max + 1);

        for (int i = 0; i < orderCount; i++)
        {
            Sprite sprite = orderSprites[
                Random.Range(0, orderSprites.Count)
            ];

            if (sprite == null)
                continue;

            GameObject imageObject = new GameObject(
                "OrderImage_" + sprite.name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

            imageObject.transform.SetParent(orderParent, false);

            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;

            RectTransform rect =
                imageObject.GetComponent<RectTransform>();

            rect.sizeDelta = orderItemSize;
            rect.localScale = Vector3.one;

            currentOrders.Add(sprite);
            displayedOrders.Add(imageObject);
        }
    }

    /// <summary>
    /// 商品オブジェクト自身と親階層のタグ名から注文を判定します。
    /// </summary>
    public bool CheckProductObject(GameObject clickedObject)
    {
        if (clickedObject == null)
        {
            IncorrectCount++;
            return false;
        }

        for (Transform current = clickedObject.transform;
             current != null;
             current = current.parent)
        {
            if (TryRemoveCurrentOrder(current.tag))
                return true;
        }

        IncorrectCount++;
        return false;
    }

    /// <summary>
    /// タグ名から注文を判定します。
    /// </summary>
    public bool CheckProductTag(string tagName)
    {
        if (TryRemoveCurrentOrder(tagName))
            return true;

        IncorrectCount++;
        return false;
    }

    private bool TryRemoveCurrentOrder(string tagName)
    {
        if (string.IsNullOrEmpty(tagName) ||
            tagName == "Untagged")
        {
            return false;
        }

        Sprite matchingSprite = null;

        foreach (Sprite sprite in orderSprites)
        {
            if (sprite != null &&
                sprite.name == tagName)
            {
                matchingSprite = sprite;
                break;
            }
        }

        if (matchingSprite == null)
            return false;

        int index = currentOrders.IndexOf(matchingSprite);

        if (index < 0)
            return false;

        CorrectCount++;
        currentOrders.RemoveAt(index);

        if (index < displayedOrders.Count)
        {
            GameObject imageObject = displayedOrders[index];
            displayedOrders.RemoveAt(index);

            if (imageObject != null)
                Destroy(imageObject);
        }

        if (currentOrders.Count == 0)
            GenerateOrder();

        return true;
    }

    private void ClearOrder()
    {
        currentOrders.Clear();

        foreach (GameObject imageObject in displayedOrders)
        {
            if (imageObject != null)
                Destroy(imageObject);
        }

        displayedOrders.Clear();
    }

    private void OnDestroy()
    {
        if (orderCanvas != null)
            Destroy(orderCanvas.gameObject);
    }
}