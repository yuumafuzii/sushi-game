using UnityEngine;

public class RandomPrefabCreate : MonoBehaviour
{
    [SerializeField] private float spawnPointX = -9f;
    [SerializeField] private float spawnPointY = 0f;
    [SerializeField] private float spawnPointZ = 7.5f;
    [SerializeField] private GameObject[] spawnPrefabs = default;

    [SerializeField] private float spawnInterval = 2.0f;
    private float timer = 0.0f;

    void Update()
    {
        if (spawnPrefabs == null || spawnPrefabs.Length == 0) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0.0f;

            int randomIndex = Random.Range(0, spawnPrefabs.Length);

            Vector3 spawnPos = new Vector3(spawnPointX, spawnPointY, spawnPointZ);

            Instantiate(spawnPrefabs[randomIndex], spawnPos, Quaternion.identity);
        }
    }
}
