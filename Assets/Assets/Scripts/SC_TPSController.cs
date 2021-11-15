using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class SC_TPSController : MonoBehaviour
{
    public Transform playerCameraParent;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;

    public Transform leftPosition;
    public Transform rightPosition;
    
    Vector2 rotation = Vector2.zero;
    private enum cameraPositions
    {
        LEFT,
        RIGHT,
    }

    private cameraPositions cameraPos = cameraPositions.RIGHT;

    [HideInInspector]

    void Start()
    {
        rotation.y = transform.eulerAngles.y;
    }

    void Update()
    {
        // Player and Camera rotation
        rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
        rotation.x += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
        playerCameraParent.localRotation = Quaternion.Euler(rotation.x, 0, 0);
        transform.eulerAngles = new Vector2(0, rotation.y);

        if (Input.GetKeyUp(KeyCode.Q))
        {
            if (cameraPos == cameraPositions.RIGHT)
            {
                playerCameraParent.transform.position = leftPosition.transform.position;
                cameraPos = cameraPositions.LEFT;
            }
            else if (cameraPos == cameraPositions.LEFT)
            {
                playerCameraParent.transform.position = rightPosition.transform.position;
                cameraPos = cameraPositions.RIGHT;
            }
        }
    }
}
