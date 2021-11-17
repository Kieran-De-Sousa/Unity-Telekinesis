using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Telekinesis : MonoBehaviour
{
    private RaycastHit raycastHit;
    private Transform currentHighlight;
    
    private void Update()
    {
        Ray raycast = Camera.current.ScreenPointToRay(Input.mousePosition);
        raycastHit = default;
        // Turns off highlight when mouse stops hovering over object
        if (currentHighlight != null)
        {
            var outline = currentHighlight.GetComponent<Outline>();
            outline.enabled = false;
            currentHighlight = null;
            Debug.Log("Highlight turned off!");
        }

        if (raycastValidTarget(raycast, raycastHit))
        {
            Debug.Log("Function worked");
        }
        if (Physics.Raycast(raycast,  out raycastHit))
        {
            var highlightedObject = raycastHit.transform;
            if (highlightedObject.CompareTag("Throwable"))
            {
                currentHighlight = highlightedObject;
                var outline = currentHighlight.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = true;
                    Debug.Log("Highlight turned on!");
                }
            }
        }
    }

    private bool raycastValidTarget(Ray raycast, RaycastHit raycastHit)
    {
        if (Physics.Raycast(raycast, out raycastHit))
        {
            var highlightedObject = raycastHit.transform;
            if (highlightedObject.CompareTag("Throwable"))
            {
                return true;
            }
        }
        return false;
    }
}

