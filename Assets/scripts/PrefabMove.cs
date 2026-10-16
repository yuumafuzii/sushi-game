using UnityEngine;

public class PrefabMove : MonoBehaviour
{
    public float StartingPosition = -9.5f;
    public float GoalPosition = 9.5f;
    public float Speed = 5.0f;

    public float RotationY = 90f;

    void Start()
    {
        transform.rotation = Quaternion.Euler(0f, RotationY, 0f);
    }

    void Update()
    {
        if (transform.position.x < GoalPosition)
        {
            transform.Translate(Vector3.right * Speed * Time.deltaTime, Space.World);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}


