using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OrderManager : MonoBehaviour
{
    [Header("注文商品Prefab")]
    [Tooltip("PrefabのタグとSpriteRendererまたはImageから情報を取得します")]
    [SerializeField] private List<GameObject> orderPrefabs = new List<GameObject>();

    [Header("注文数")]
    [SerializeField, Min(1)] private int orderCount = 3;

    [Header("注文表示位置")]
    [SerializeField] private Vector2 orderPosition = new Vector2(0f, -20f);

    [Header("注文画像サイズ・間隔")]
    public Vector2 orderItemSize = new Vector2(100f, 100f);
    public float orderSpacing = 10f;

    [Header("ミス・帰るUI Prefab")]
    [SerializeField] private Canvas feedbackCanvas;
    [SerializeField] private GameObject level1UIPrefab;
    [SerializeField] private Vector2 level1UIPosition;
    [SerializeField] private GameObject level2UIPrefab;
    [SerializeField] private Vector2 level2UIPosition;
    [SerializeField] private GameObject leaveUIPrefab;
    [SerializeField] private Vector2 leaveUIPosition;
    [SerializeField, Min(0f)] private float leaveDisplayDuration = 1.5f;

    [Header("ゲーム終了条件")]
    [SerializeField, Min(1)] private int customersToClear = 10;
    [SerializeField, Min(1)] private int mistakesToGameOver = 20;

    [Header("遷移先シーン名")]
    [SerializeField] private string gameOverSceneName;
    [SerializeField] private string gameClearSceneName;

    private Canvas orderCanvas;
    private RectTransform orderParent;
    private bool waitingForNextOrder;
    private bool isEnding;
    private int completedCustomers;
    private int currentCustomerMistakes;

    private readonly List<ProductData> availableProducts =
        new List<ProductData>();

    private readonly List<OrderEntry> currentOrders =
        new List<OrderEntry>();

    private readonly List<string> previousOrderTags =
        new List<string>();

    public int CorrectCount { get; private set; }
    public int IncorrectCount { get; private set; }

    private GameObject level1UI;
    private GameObject level2UI;
    private GameObject leaveUI;

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
        CreateFeedbackUIs();
        HideFeedbackUIs();
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
                    $"Prefab「{productPrefab.name}」からSpriteを取得できません。",
                    productPrefab
                );
                continue;
            }

            availableProducts.Add(new ProductData
            {
                TagName = productPrefab.tag,
                Sprite = sprite
            });
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
        List<ProductData> newProducts = new List<ProductData>();

        for (int i = 0; i < count; i++)
        {
            newProducts.Add(
                availableProducts[Random.Range(0, availableProducts.Count)]
            );
        }

        // 直前と同じ注文になった場合、可能なら1品を別の商品に変更する
        if (IsSameAsPreviousOrder(newProducts))
        {
            for (int i = 0; i < newProducts.Count; i++)
            {
                ProductData alternative = FindAlternativeProduct(
                    previousOrderTags[i]
                );

                if (alternative != null)
                {
                    newProducts[i] = alternative;
                    break;
                }
            }
        }

        previousOrderTags.Clear();

        float spacing = orderItemSize.x + orderSpacing;
        float startX = -((count - 1) * spacing) / 2f;

        for (int i = 0; i < newProducts.Count; i++)
        {
            ProductData product = newProducts[i];
            previousOrderTags.Add(product.TagName);

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

    private bool IsSameAsPreviousOrder(List<ProductData> products)
    {
        if (products.Count != previousOrderTags.Count || products.Count == 0)
            return false;

        for (int i = 0; i < products.Count; i++)
        {
            if (products[i].TagName != previousOrderTags[i])
                return false;
        }

        return true;
    }

    private ProductData FindAlternativeProduct(string excludedTag)
    {
        List<ProductData> alternatives = new List<ProductData>();

        foreach (ProductData product in availableProducts)
        {
            if (product.TagName != excludedTag)
                alternatives.Add(product);
        }

        if (alternatives.Count == 0)
            return null;

        return alternatives[Random.Range(0, alternatives.Count)];
    }

    public bool CheckProductTag(string tagName)
    {
        Debug.Log($"CheckProductTag呼び出し: {tagName}", this);

        if (isEnding || waitingForNextOrder || string.IsNullOrEmpty(tagName))
            return false;

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

                if (completedCustomers >= customersToClear)
                    LoadEndingScene(gameClearSceneName);
                else
                    GenerateNextOrder();
            }

            return true;
        }

        IncorrectCount++;
        currentCustomerMistakes++;

        Debug.Log($"不正解判定: {tagName} / ミス回数={currentCustomerMistakes}", this);

        if (IncorrectCount >= mistakesToGameOver)
        {
            LoadEndingScene(gameOverSceneName);
            return false;
        }

        if (currentCustomerMistakes == 1)
            ShowFeedbackUI(level1UI);
        else if (currentCustomerMistakes == 2)
            ShowFeedbackUI(level2UI);
        else
            StartCoroutine(CustomerLeavesAndRefresh());

        return false;
    }

    private void ShowFeedbackUI(GameObject uiToShow)
    {
        if (uiToShow == null)
        {
            Debug.LogError("表示UIがありません。", this);
            return;
        }

        level1UI.SetActive(uiToShow == level1UI);
        level2UI.SetActive(uiToShow == level2UI);
        leaveUI.SetActive(uiToShow == leaveUI);

        uiToShow.SetActive(true);
        Debug.Log($"UI表示: {uiToShow.name}, active={uiToShow.activeInHierarchy}", uiToShow);
    }

    private void HideFeedbackUIs()
    {
        SetActiveIfAssigned(level1UI, false);
        SetActiveIfAssigned(level2UI, false);
        SetActiveIfAssigned(leaveUI, false);
    }

    private void SetActiveIfAssigned(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }

    private void ApplyFeedbackUIPositions()
    {
        SetUIPosition(level1UI, level1UIPosition);
        SetUIPosition(level2UI, level2UIPosition);
        SetUIPosition(leaveUI, leaveUIPosition);
    }

    private void SetUIPosition(GameObject target, Vector2 position)
    {
        if (target == null)
            return;

        RectTransform rect = target.GetComponent<RectTransform>();

        if (rect != null)
            rect.anchoredPosition = position;
    }

    private void LoadEndingScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("遷移先のシーン名が設定されていません。", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"シーン「{sceneName}」を読み込めません。Build ProfilesのScene Listを確認してください。",
                this
            );
            return;
        }

        isEnding = true;
        SceneManager.LoadScene(sceneName);
    }

    private void GenerateNextOrder()
    {
        waitingForNextOrder = true;
        HideFeedbackUIs();

        if (!isEnding)
        {
            currentCustomerMistakes = 0;
            GenerateOrder();
        }

        waitingForNextOrder = false;
    }

    private IEnumerator CustomerLeavesAndRefresh()
    {
        waitingForNextOrder = true;
        HideFeedbackUIs();

        if (leaveUI != null)
            leaveUI.SetActive(true);

        yield return new WaitForSeconds(leaveDisplayDuration);

        if (!isEnding)
        {
            HideFeedbackUIs();
            currentCustomerMistakes = 0;
            GenerateOrder();
        }

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
        if (orderParent == null)
            return;

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

    private void CreateFeedbackUIs()
    {
        if (feedbackCanvas == null)
        {
            Debug.LogError("Feedback Canvasが設定されていません。", this);
            return;
        }

        level1UI = CreateFeedbackUI(level1UIPrefab, level1UIPosition);
        level2UI = CreateFeedbackUI(level2UIPrefab, level2UIPosition);
        leaveUI = CreateFeedbackUI(leaveUIPrefab, leaveUIPosition);
    }

    private GameObject CreateFeedbackUI(GameObject prefab, Vector2 position)
    {
        if (prefab == null)
            return null;

        GameObject instance = Instantiate(prefab, feedbackCanvas.transform, false);
        instance.name = prefab.name;
        instance.SetActive(false);

        RectTransform rect = instance.GetComponent<RectTransform>();
        if (rect != null)
            rect.anchoredPosition = position;

        return instance;
    }
}