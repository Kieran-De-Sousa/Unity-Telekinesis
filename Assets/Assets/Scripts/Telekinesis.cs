using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Telekinesis : MonoBehaviour
{
    // Highlighting
    private RaycastHit raycastHit;
    private Transform highlightedObject;
    private Transform currentHighlight;
    
    // Telekinesis
    public float pullForce;
    public float throwForce;
    public Transform grabPositon;
    private Transform grabbedObject;
    private bool pressedGrab = false;
    private bool canGrab = false;
    private bool grabbing = false;
    private bool grabbed = false;

    void Update()
    {
        if (Input.GetButtonDown("Grab"))
        {
            pressedGrab = true;
            Debug.Log("Pressed Grab!");
        }
        
        if (canGrab && pressedGrab)
        {
            // When not already holding an object with telekinesis and not already pulling one
            if (!grabbing && !grabbed)
            {
                grabbedObject = currentHighlight;
                canGrab = false;
                grabbing = true;
            }
            // When already holding an object with telekinesis
            else if (grabbing)
            {
                grabbedObject.GetComponent<Rigidbody>().useGravity = true;
                grabbedObject.transform.parent = null;
                canGrab = true;
                grabbing = false;
            }
        }
        
        Ray raycast = Camera.current.ScreenPointToRay(Input.mousePosition);
        raycastHit = default;
        // Throwable target was hit with raycast
        if (raycastValidTarget(raycast, raycastHit) == 1)
        {
            currentHighlight = highlightedObject;
            var outline = currentHighlight.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = true;
                canGrab = true;
                Debug.Log("Highlight turned on!");
            }
        }
        // Shield Spawner target was hit with raycast
        else if (raycastValidTarget(raycast, raycastHit) == 2)
        {
            // Instantiate objects from position and prevent player from grabbing either objects of floor
            // Update bool to move them to shield position (in front of player, height doesn't matter currently)
            // Keep their position updated in fixed update to in front of player
            // Throw objects in ... direction (random directions possibly?) if player presses same button again
            // Allow them to grab pieces of floor again or throwable objects
        }
        else
        {
            // Turns off highlight when mouse stops hovering over object
            raycastRemoveHighlight();
        }
    }

    void FixedUpdate()
    {
        if (grabbing)
        {
            grabObject();
        }

        if (grabbed && pressedGrab)
        {
            throwObject();
            pressedGrab = false;
        }

        pressedGrab = false;
    }

    /// Checks if mouse is hovering over a object with the "Throwable" tag, used
    /// for telekinesis abilities
    private int raycastValidTarget(Ray raycast, RaycastHit raycastHit)
    {
        if (Physics.Raycast(raycast, out raycastHit))
        {
            highlightedObject = raycastHit.transform;
            if (highlightedObject.CompareTag("Throwable"))
            {
                return 1;
            }
            if (highlightedObject.CompareTag("ShieldSpawnable"))
            {
                return 2;
            }
        }
        return 0;
    }

    private void raycastRemoveHighlight()
    {
        if (currentHighlight != null)
        {
            var outline = currentHighlight.GetComponent<Outline>();
            outline.enabled = false;
            currentHighlight = null;
            canGrab = false;
            Debug.Log("Highlight turned off!");
        }
    }

    private void grabObject()
    {
        if (grabbedObject.transform.position != grabPositon.transform.position)
        {
            grabbedObject.transform.position = Vector3.MoveTowards(grabbedObject.transform.position,
                grabPositon.transform.position, pullForce);
            grabbedObject.GetComponent<Rigidbody>().useGravity = false;
            grabbedObject.transform.parent = GameObject.Find("TelekinesisPos").transform;
        }
        else
        {
            grabbing = false;
            grabbed = true;
        }
    }

    private void throwObject()
    {
        canGrab = true;
        grabbed = false;
        grabbedObject.transform.parent = null;
        grabbedObject.GetComponent<Rigidbody>().velocity = transform.forward * throwForce;
        grabbedObject.GetComponent<Rigidbody>().useGravity = true;
        grabbedObject = null;
        // Vector3 pullDirection = grabPositon.position - grabbedObject.position;
        // Vector3 pullForce = pullDirection.normalized * 3.0f;
        // grabbedObject.GetComponent<Rigidbody>().AddForce(pullForce, ForceMode.Force);AddForce
    }
}

