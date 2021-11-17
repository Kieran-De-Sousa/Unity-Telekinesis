using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Telekinesis : MonoBehaviour
{
    private RaycastHit raycastHit;
    private Transform highlightedObject;
    private Transform currentHighlight;

    private void Update()
    {
        Ray raycast = Camera.current.ScreenPointToRay(Input.mousePosition);
        raycastHit = default;
        
        // Turns off highlight when mouse stops hovering over object
        raycastRemoveHighlight();
        
        if (raycastValidTarget(raycast, raycastHit))
        {
            currentHighlight = highlightedObject;
            var outline = currentHighlight.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = true;
                Debug.Log("Highlight turned on!");
                // Telekinesis grab function here
            }
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
            Debug.Log("Highlight turned off!");
        }
    }
}

