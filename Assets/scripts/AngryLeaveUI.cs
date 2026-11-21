using System.Collections;
using UnityEngine;

public class AngryLeaveUI : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel;
    [SerializeField, Min(0f)] private float displayDuration = 1.5f;

    public float DisplayDuration => displayDuration;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false);
    }

    public void Show()
    {
        if (uiPanel == null)
        {
            Debug.LogError("怒って帰るUIのPanelが設定されていません。", this);
            return;
        }

        uiPanel.SetActive(true);

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        if (uiPanel != null)
            uiPanel.SetActive(false);

        hideCoroutine = null;
    }
}