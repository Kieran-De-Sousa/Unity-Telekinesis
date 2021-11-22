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
    private Transform grabbedObject;
    private bool canGrab = false;
    private bool grabbing = false;
    public Transform grabPositon;

    private void Update()
    {
        Ray raycast = Camera.current.ScreenPointToRay(Input.mousePosition);
        raycastHit = default;

        if (raycastValidTarget(raycast, raycastHit))
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
        else
        {
            // Turns off highlight when mouse stops hovering over object
            raycastRemoveHighlight();
        }
    }

    private void FixedUpdate()
    {
        if (canGrab)
        {
            // Telekinesis grab function here
            if (Input.GetButtonDown("Fire1"))
            {
                Debug.Log("Pressed Grab!");
                // When not already holding an object with telekinesis
                if (!grabbing)
                {
                    grabbedObject = currentHighlight;
                    grabbedObject.GetComponent<Rigidbody>().useGravity = false;
                    grabbedObject.transform.position = grabPositon.position;
                    grabbedObject.transform.parent = GameObject.Find("TelekinesisPos").transform;
                    grabbing = true;
                }
                // When already holding an object with telekinesis
                else if (grabbing)
                {
                    grabbedObject.GetComponent<Rigidbody>().useGravity = true;
                    grabbedObject.transform.parent = null;
                    grabbing = false;
                }
            }
        }

        if (grabbing)
        {
            
        }
        else if (!grabbing)
        {
            
        }
    }

    /// Checks if mouse is hovering over a object with the "Throwable" tag, used
    /// for telekinesis abilities
    private bool raycastValidTarget(Ray raycast, RaycastHit raycastHit)
    {
        if (Physics.Raycast(raycast, out raycastHit))
        {
            highlightedObject = raycastHit.transform;
            if (highlightedObject.CompareTag("Throwable"))
            {
                return true;
            }
        }
        return false;
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
}

