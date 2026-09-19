using System.Collections.Generic;
using UnityEngine;

public class RandomCanvasSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> uiPrefabs;

    void Start()
    {
        if (uiPrefabs == null || uiPrefabs.Count == 0) return;

        Canvas activeCanvas = GameObject.FindAnyObjectByType<Canvas>();
        if (activeCanvas == null) return;

        int randomIndex = Random.Range(0, uiPrefabs.Count);
        GameObject selectedPrefab = uiPrefabs[randomIndex];

        if (selectedPrefab != null)
        {
            Instantiate(selectedPrefab, activeCanvas.transform, false);
        }
    }
}
