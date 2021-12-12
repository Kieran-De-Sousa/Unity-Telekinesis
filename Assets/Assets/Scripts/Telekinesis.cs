using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class Telekinesis : MonoBehaviour
{
    // Raycast/Highlighting //
    private RaycastHit raycastHit;
    private Vector3 hitPosition;
    private Transform highlightedObject;
    private Transform currentHighlight;
    
    // Telekinesis //
    private bool pressedGrab = false;
    private bool pressedShield = false;
    // Pulling/Throwing Objects
    [Header("Pulling/Throwing")]
    public float pullForce;
    public float throwForce;
    public Transform grabPosition;
    private Transform grabbedObject;
    private bool canGrab = false;
    private bool grabbing = false;
    private bool grabbed = false;
    // Shield
    [Header("Shield")]
    public Transform shieldPosition;
    public GameObject shieldDebrisModel;
    public int shieldDebrisNumber;
    public Vector3 shieldGrabSize;
    public Vector3 shieldSize;
    private GameObject[] shieldDebris;
    private Vector3[] shieldPos;
    private Quaternion playerParentRotation;
    private bool canShield = false;
    private bool shieldActivated = false;
    private bool shieldActive = false;

    private void Start()
    {
        shieldDebris = new GameObject[shieldDebrisNumber];
        shieldPos = new Vector3[shieldDebrisNumber];
        for (int i = 0; i < shieldDebrisNumber; i++)
        {
            shieldPos[i] = new Vector3(UnityEngine.Random.Range(-shieldSize.x / 2, shieldSize.x / 2), 
                UnityEngine.Random.Range(-shieldSize.y / 2, shieldSize.y / 2), UnityEngine.Random.Range(-shieldSize.z / 2, shieldSize.z / 2));
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Grab"))
        {
            pressedGrab = true;
            Debug.Log("Pressed Grab!");
        }

        if (Input.GetButtonDown("Shield"))
        {
            pressedShield = true;
            Debug.Log("Pressed Shield!");
        }
        
        
        // Pulling/Throwing objects //
        if (canGrab && pressedGrab && !shieldActivated && !shieldActive)
        {
            // When not already holding an object with telekinesis and not already pulling one
            if (!grabbing && !grabbed)
            {
                grabbedObject = currentHighlight;
                canGrab = false;
                canShield = false;
                grabbing = true;
            }
            // When already holding an object with telekinesis
            else if (grabbing)
            {
                grabbedObject.GetComponent<Rigidbody>().useGravity = true;
                grabbedObject.transform.parent = null;
                canGrab = true;
                canShield = true;
                grabbing = false;
            }
        }
        // Shield //
        if (canShield && pressedShield && !grabbing && !grabbed)
        {
            if (!shieldActivated && !shieldActive)
            {
                for (int i = 0; i < shieldDebris.Length; i++)
                {
                    Vector3 spawnPos = hitPosition + new Vector3(UnityEngine.Random.Range(-shieldGrabSize.x / 2, shieldGrabSize.x / 2), 
                        UnityEngine.Random.Range(-shieldGrabSize.y / 2, shieldGrabSize.y / 2), UnityEngine.Random.Range(-shieldGrabSize.z / 2, shieldGrabSize.z / 2));
                    shieldDebris[i] = Instantiate(shieldDebrisModel, spawnPos, highlightedObject.transform.rotation);
                    
                    shieldPos[i] = new Vector3(UnityEngine.Random.Range(-shieldSize.x / 2, shieldSize.x / 2), 
                        UnityEngine.Random.Range(-shieldSize.y / 2, shieldSize.y / 2), UnityEngine.Random.Range(-shieldSize.z / 2, shieldSize.z / 2));
                }
                canShield = false;
                canGrab = false;
                shieldActivated = true;
            }
            else if (shieldActivated)
            {
                for (int i = 0; i < shieldDebris.Length; i++)
                {
                    Destroy(shieldDebris[i]);
                }
                canShield = true;
                canGrab = true;
                shieldActivated = false;
                // !!! - Instantiate objects from position and prevent player from grabbing either objects of floor
                // !!! Update bool to move them to shield position (in front of player, height doesn't matter currently)
                // Keep their position updated in fixed update to in front of player
                // !!! Throw objects in ... direction (random directions possibly?) if player presses same button again
                // !!! Allow them to grab pieces of floor again or throwable objects
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
            canShield = true;
        }
        else
        {
            // Turns off highlight when mouse stops hovering over object
            raycastRemoveHighlight();
        }
        
        // Used for shield
        playerParentRotation = shieldPosition.parent.rotation;
    }

    void FixedUpdate()
    {
        // Pulling throwable object
        if (grabbing)
        {
            grabObject();
        }

        // Throwing throwable object
        if (grabbed && pressedGrab)
        {
            throwObject();
            pressedGrab = false;
        }

        // Pulling shield debris
        if (shieldActivated)
        {
            grabShield();
        }

        // Throwing shield debris
        if (shieldActive)
        {
            for (int i = 0; i < shieldDebrisNumber; i++)
            {
                shieldDebris[i].transform.rotation = Quaternion.Euler(shieldDebris[i].transform.rotation.x, playerParentRotation.y, playerParentRotation.z);
                shieldDebris[i].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            }
            if (pressedShield)
            {
                throwShield();
                pressedShield = false;
            }
        }
        pressedGrab = false;
        pressedShield = false;
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
                hitPosition = raycastHit.point;
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
        if (grabbedObject.transform.position != grabPosition.transform.position)
        {
            grabbedObject.transform.position = Vector3.MoveTowards(grabbedObject.transform.position,
                grabPosition.transform.position, pullForce);
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

    private void grabShield()
    {
        for (int i = 0; i < shieldDebrisNumber; i++) 
        {
            Vector3 temp = shieldPosition.transform.position + shieldPos[i];
            if (shieldDebris[i].transform.position != temp)
            {
                shieldDebris[i].transform.position = Vector3.MoveTowards(shieldDebris[i].transform.position, temp, pullForce);
                shieldDebris[i].GetComponent<Rigidbody>().useGravity = false;
                shieldDebris[i].GetComponent<Collider>().enabled = false;
                shieldDebris[i].transform.parent = GameObject.Find("ShieldPos").transform;
            }
            else
            {
                for (int j = 0; j < shieldDebrisNumber; j++)
                {
                    shieldDebris[j].GetComponent<Collider>().enabled = true;
                }
                shieldActivated = false;
                shieldActive = true;
            }
        }
    }

    private void throwShield()
    {
        canShield = true;
        shieldActive = false;
        for (int i = 0; i < shieldDebrisNumber; i++)
        {
            shieldDebris[i].transform.parent = null;
            shieldDebris[i].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            shieldDebris[i].GetComponent<Rigidbody>().velocity = transform.forward * throwForce;
            shieldDebris[i].GetComponent<Rigidbody>().useGravity = true;
            shieldDebris[i].GetComponent<Collider>().enabled = true;
        }
    }
}

