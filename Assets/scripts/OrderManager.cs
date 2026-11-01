using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OrderManager : MonoBehaviour
{
    [Header("注文商品Prefab")]
    [Tooltip("PrefabのタグとSpriteRendererまたはImageから情報を自動取得します")]
    [SerializeField]
    private List<GameObject> orderPrefabs = new List<GameObject>();

    [Header("注文数")]
    [SerializeField, Min(1)]
    private int orderCount = 3;

    [Header("注文表示位置（画面中央からのXYオフセット）")]
    [SerializeField]
    private Vector2 orderPosition = new Vector2(0f, -20f);

    [Header("注文画像サイズ")]
    public Vector2 orderItemSize = new Vector2(100f, 100f);

    [Header("注文画像の間隔")]
    public float orderSpacing = 10f;

    [Header("ゲーム終了条件")]
    [SerializeField, Min(1)]
    private int customersToClear = 10;

    [SerializeField, Min(1)]
    private int mistakesToGameOver = 20;

    [Header("遷移先シーン名")]
    [SerializeField]
    private string gameOverSceneName;

    [SerializeField]
    private string gameClearSceneName;

    private Canvas orderCanvas;
    private RectTransform orderParent;
    private bool waitingForNextOrder;
    private bool isEnding;
    private int completedCustomers;

    private readonly List<ProductData> availableProducts =
        new List<ProductData>();

    private readonly List<OrderEntry> currentOrders =
        new List<OrderEntry>();

    public int CorrectCount { get; private set; }
    public int IncorrectCount { get; private set; }

    private class ProductData
    {
        public string TagName;
        public Sprite Sprite;
    }

    private class OrderEntry
    {
        public string TagName;
        public GameObject ImageObject;
    }

    private void Start()
    {
        LoadProducts();

        if (availableProducts.Count == 0)
        {
            Debug.LogError(
                "注文に使えるPrefabがありません。PrefabのタグとSpriteを確認してください。",
                this
            );
            return;
        }

        CreateOrderCanvas();
        GenerateOrder();
    }

    private void LoadProducts()
    {
        availableProducts.Clear();

        foreach (GameObject productPrefab in orderPrefabs)
        {
            if (productPrefab == null)
            {
                Debug.LogWarning("注文Prefabに未設定の項目があります。", this);
                continue;
            }

            Sprite sprite = GetPrefabSprite(productPrefab);

            if (sprite == null)
            {
                Debug.LogWarning(
                    $"Prefab「{productPrefab.name}」からSpriteを取得できません。SpriteRendererまたはImageを設定してください。",
                    productPrefab
                );
                continue;
            }

            ProductData product = new ProductData
            {
                TagName = productPrefab.tag,
                Sprite = sprite
            };

            availableProducts.Add(product);

            Debug.Log(
                $"注文Prefabを登録: {productPrefab.name}, タグ={product.TagName}, Sprite={sprite.name}",
                productPrefab
            );
        }
    }

    private Sprite GetPrefabSprite(GameObject prefab)
    {
        SpriteRenderer spriteRenderer =
            prefab.GetComponentInChildren<SpriteRenderer>(true);

        if (spriteRenderer != null && spriteRenderer.sprite != null)
            return spriteRenderer.sprite;

        Image image = prefab.GetComponentInChildren<Image>(true);

        if (image != null && image.sprite != null)
            return image.sprite;

        return null;
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
            typeof(RectTransform)
        );

        parentObject.transform.SetParent(canvasObject.transform, false);

        orderParent = parentObject.GetComponent<RectTransform>();
        orderParent.anchorMin = new Vector2(0.5f, 0.5f);
        orderParent.anchorMax = new Vector2(0.5f, 0.5f);
        orderParent.pivot = new Vector2(0.5f, 0.5f);
        orderParent.anchoredPosition = orderPosition;
    }

    private void GenerateOrder()
    {
        ClearOrder();

        if (orderParent == null || availableProducts.Count == 0)
            return;

        int count = Mathf.Max(1, orderCount);
        float spacing = orderItemSize.x + orderSpacing;
        float startX = -((count - 1) * spacing) / 2f;

        for (int i = 0; i < count; i++)
        {
            ProductData product =
                availableProducts[Random.Range(0, availableProducts.Count)];

            GameObject imageObject = new GameObject(
                "OrderImage_" + product.Sprite.name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

            imageObject.transform.SetParent(orderParent, false);

            Image image = imageObject.GetComponent<Image>();
            image.sprite = product.Sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;

            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.sizeDelta = orderItemSize;
            rect.anchoredPosition = new Vector2(startX + i * spacing, 0f);

            currentOrders.Add(new OrderEntry
            {
                TagName = product.TagName,
                ImageObject = imageObject
            });
        }
    }

    public bool CheckProductTag(string tagName)
    {
        if (isEnding || string.IsNullOrEmpty(tagName) || waitingForNextOrder)
            return false;

        List<string> orderTags = new List<string>();

        foreach (OrderEntry order in currentOrders)
            orderTags.Add(order.TagName);

        Debug.Log(
            $"注文タグ一覧: [{string.Join(", ", orderTags)}] / クリックタグ: {tagName}",
            this
        );

        for (int i = 0; i < currentOrders.Count; i++)
        {
            if (currentOrders[i].TagName != tagName)
                continue;

            GameObject imageObject = currentOrders[i].ImageObject;
            currentOrders.RemoveAt(i);

            if (imageObject != null)
                Destroy(imageObject);

            CorrectCount++;
            RefreshOrderPositions();

            if (currentOrders.Count == 0)
            {
                completedCustomers++;
                Debug.Log(
                    $"注文完了: {completedCustomers}/{customersToClear}人",
                    this
                );

                if (completedCustomers >= customersToClear)
                {
                    LoadEndingScene(gameClearSceneName);
                }
                else
                {
                    StartCoroutine(GenerateNextOrder());
                }
            }

            return true;
        }

        IncorrectCount++;
        Debug.Log(
            $"不正解: {IncorrectCount}/{mistakesToGameOver}回",
            this
        );

        if (IncorrectCount >= mistakesToGameOver)
            LoadEndingScene(gameOverSceneName);

        return false;
    }

    private void LoadEndingScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("遷移先のシーン名が設定されていません。", this);
            return;
        }

        isEnding = true;
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator GenerateNextOrder()
    {
        waitingForNextOrder = true;
        yield return new WaitForSeconds(0.5f);

        if (!isEnding)
            GenerateOrder();

        waitingForNextOrder = false;
    }

    private void ClearOrder()
    {
        foreach (OrderEntry order in currentOrders)
        {
            if (order.ImageObject != null)
                Destroy(order.ImageObject);
        }

        currentOrders.Clear();
    }

    private void RefreshOrderPositions()
    {
        float spacing = orderItemSize.x + orderSpacing;
        float startX = -((Mathf.Max(1, orderCount) - 1) * spacing) / 2f;

        for (int i = 0; i < currentOrders.Count; i++)
        {
            GameObject imageObject = currentOrders[i].ImageObject;

            if (imageObject == null)
                continue;

            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(startX + i * spacing, 0f);
        }
    }

    private void LateUpdate()
    {
        if (orderParent != null)
            ApplyOrderPosition();
    }

    private void ApplyOrderPosition()
    {
        orderParent.anchorMin = new Vector2(0.5f, 0.5f);
        orderParent.anchorMax = new Vector2(0.5f, 0.5f);
        orderParent.pivot = new Vector2(0.5f, 0.5f);
        orderParent.anchoredPosition = orderPosition;
    }

    private void OnDestroy()
    {
        if (orderCanvas != null)
            Destroy(orderCanvas.gameObject);
    }
}