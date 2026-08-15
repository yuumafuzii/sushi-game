using UnityEngine;

public class PrefabMove : MonoBehaviour
{
    public float StartingPosition = -10f;
    public float GoalPosition = 10f;
    public float Speed = 5.0f;

    public float RotationY = 0f;

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


