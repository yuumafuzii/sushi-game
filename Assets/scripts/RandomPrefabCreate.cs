using UnityEngine;

public class RandomPrefabCreate : MonoBehaviour
{
    [SerializeField]
    private GameObject[] prefabs;

    [SerializeField]
    private Transform spawnPoint;

    private void Start()
    {
        Spawn();
    }

    private void Spawn()
    {
        int index = Random.Range(0, prefabs.Length);

        Instantiate(
            prefabs[index],
            spawnPoint.position,
            spawnPoint.rotation
        );
    }
}