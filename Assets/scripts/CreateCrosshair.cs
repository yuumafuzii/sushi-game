using UnityEngine;
using UnityEngine.UI;

public class CreateCrosshair : MonoBehaviour
{
    public Color crosshairColor = Color.green;
    public float thickness = 2f;
    public float length = 20f;

    public Vector2 offsetPosition = Vector2.zero;
    public float crosshairScale = 1.0f;

    private RectTransform rootRect;

    void Start()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("CrosshairCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        GameObject crosshairRoot = new GameObject("Crosshair");
        crosshairRoot.transform.SetParent(canvas.transform, false);
        rootRect = crosshairRoot.AddComponent<RectTransform>();
        SetAnchorToCenter(rootRect);

        CreateLine(crosshairRoot.transform, Vector2.zero, new Vector2(thickness, length));
        CreateLine(crosshairRoot.transform, Vector2.zero, new Vector2(length, thickness));

        UpdateCrosshairTransform();
    }

    void OnValidate()
    {
        if (rootRect != null)
        {
            UpdateCrosshairTransform();
        }
    }

    void UpdateCrosshairTransform()
    {
        rootRect.anchoredPosition = offsetPosition;
        rootRect.localScale = new Vector3(crosshairScale, crosshairScale, 1f);
    }

    void CreateLine(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject line = new GameObject("Line");
        line.transform.SetParent(parent, false);

        Image img = line.AddComponent<Image>();
        img.color = crosshairColor;

        RectTransform rect = line.GetComponent<RectTransform>();
        SetAnchorToCenter(rect);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    void SetAnchorToCenter(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }
}