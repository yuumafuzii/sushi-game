using UnityEngine;

public class PrefabMove : MonoBehaviour
{
    public float StartingPosition = -10f;
    public float GoalPosition = 10f;
    public float Speed = 5.0f;

    void Update()
    {
        if (transform.position.x < GoalPosition)
        {
            transform.Translate(Vector3.forward * Speed * Time.deltaTime);
        }
    }
}
