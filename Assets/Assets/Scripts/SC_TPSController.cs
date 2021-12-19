using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class SC_TPSController : MonoBehaviour
{
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;

    [Header("Camera Settings")]
    public float speed;
    public Transform playerCameraParent;
    public Transform leftPosition;
    public Transform rightPosition;
    
    Vector2 rotation = Vector2.zero;
    private enum cameraPositions
    {
        LEFT,
        RIGHT,
    }

    private cameraPositions cameraPos = cameraPositions.RIGHT;
    private bool cameraMoving = false;
    private Vector3 movingTarget;

    [HideInInspector]

    void Start()
    {
        rotation.y = transform.eulerAngles.y;
    }

    void Update()
    {
        // Camera swapping script
        if (Input.GetButtonDown("CameraSwap"))
        {
            cameraMoving = true;
            if (cameraPos == cameraPositions.RIGHT)
            {
                cameraPos = cameraPositions.LEFT;
                movingTarget = leftPosition.transform.position;
            }
            else if (cameraPos == cameraPositions.LEFT)
            {
                cameraPos = cameraPositions.RIGHT;
                
                movingTarget = rightPosition.transform.position;
            }
        }
        
        // Player and Camera rotation
        rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
        rotation.x += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
        playerCameraParent.localRotation = Quaternion.Euler(rotation.x, 0, 0);
        transform.eulerAngles = new Vector2(0, rotation.y);

        if (cameraPos == cameraPositions.RIGHT)
        {
            movingTarget = leftPosition.transform.position;
        }
        else if (cameraPos == cameraPositions.LEFT)
        {
            movingTarget = rightPosition.transform.position;
        }
    }

    private void FixedUpdate()
    {
        float movement = speed * Time.deltaTime;
        
        // Slowly moves towards left/right camera position if camera is in moving state
        if (cameraMoving)
        {
            if (playerCameraParent.transform.position != movingTarget)
            {
                playerCameraParent.transform.position = Vector3.MoveTowards(playerCameraParent.transform.position, movingTarget, movement);
            }
            else
            {
                {
                    cameraMoving = false;
                }
            }
        }
    }
}
