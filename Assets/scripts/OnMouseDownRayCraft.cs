using UnityEngine;
using UnityEngine.InputSystem;

public class OnMouseDownRayCraft : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, 100f))
            {
                Debug.Log(hitInfo.collider.name);
            }
        }
    }
}
