using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class OnMouseDownRayCraft : MonoBehaviour
{
    public static OnMouseDownRayCraft Instance;
    public List<string> destroyedList = new List<string>();

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Transform cam = Camera.main.transform;

            Vector3 forwardXZ = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;

            Vector3 center = new Vector3(cam.position.x, 0f, cam.position.z);
            Vector3 halfExtents = new Vector3(0.1f, 500f, 0.1f);
            Quaternion orientation = Quaternion.LookRotation(forwardXZ);

            RaycastHit hitInfo;

            if (Physics.BoxCast(center, halfExtents, forwardXZ, out hitInfo, orientation, 100f))
            {
                destroyedList.Add(hitInfo.collider.gameObject.name);
                Destroy(hitInfo.collider.gameObject);
            }
        }
    }
}


