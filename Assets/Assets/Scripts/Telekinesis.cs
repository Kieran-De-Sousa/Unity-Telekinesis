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
    private int raycastResult;
    private Vector3 hitPosition;
    private Transform highlightedObject;
    private Transform currentHighlight;
    
    // Telekinesis //
    private bool pressedGrab = false;
    private bool pressedShield = false;
    // Pulling/Throwing Objects
    [Header("Pulling/Throwing")]
    public float grabPullForce;
    public float grabThrowForce;
    [Tooltip("Position of where grabbed object should hover.")]
    public Transform grabPosition;
    [Tooltip("Area around grab position where if the object enters, it counts" +
             " as reaching the 'grabPosition' location.")]
    public float grabPositionThreshold = 1.0f;
    private Transform grabbedObject;
    private enum grabStates
    {
        IDLE,
        GRABBING,
        GRABBED
    }
    private grabStates grabState = grabStates.IDLE;
    private bool canGrab = false;
    // Shield
    [Header("Shield")] 
    public float shieldPullForce;
    public float shieldThrowForce;
    [Tooltip("Position of where shield debris objects should hover " +
             "(not accounting for their random scatter)")]
    public Transform shieldPosition;
    [Tooltip("Array of shield debris models to randomly choose from upon instantiation")]
    public GameObject[] shieldDebrisModel;
    [Tooltip("Area around shield position where if any shield debris enters," +
             " it counts as reaching the 'shieldPosition' location.")]
    public float shieldPositionThreshold = 0.5f;
    [Tooltip("Number of shield debris pieces to spawn.")]
    public int shieldDebrisNumber;
    [Tooltip("Size of area to grab debris from.")]
    public Vector3 shieldGrabSize;
    [Tooltip("Size of invisible shield that would have a origin of 'shieldPosition'")]
    public Vector3 shieldSize;
    private GameObject[] shieldDebris;
    private Vector3[] shieldPos;
    private Quaternion playerParentRotation;
    private enum shieldStates
    {
        IDLE,
        SHIELD_ACTIVATED,
        SHIELD_ACTIVE
    }
    private shieldStates shieldState = shieldStates.IDLE;
    private bool canShield = false;
    // Additional Inspector Controls
    public enum pullingModes
    {
        MOVETOWARDS,
        ADDFORCE
    }
    [Header("Additional Controls")]
    [Tooltip("Chose how to pull the object/shield debris - \n" +
             "MOVETOWARDS: 'Vector3.Movetowards' \n" +
             "ADDFORCE: 'addforce'")]
    public pullingModes pullMode = pullingModes.ADDFORCE;

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
        
        Ray raycast = Camera.current.ScreenPointToRay(Input.mousePosition);
        raycastHit = default;
        raycastResult = raycastValidTarget(raycast, raycastHit);
        // Turns off highlight when mouse stops hovering over object
        if (raycastResult != 1)
        {
            raycastRemoveHighlight();
        }
        
        // Pulling/Throwing objects //
        // Throwable target was hit with raycast
        if (raycastResult == 1)
        {
            currentHighlight = highlightedObject;
            var outline = currentHighlight.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = true;
            }

            if (pressedGrab && shieldState == shieldStates.IDLE)
            {
                // When not already holding an object with telekinesis and not already pulling one
                if (grabState == grabStates.IDLE)
                {
                    grabbedObject = currentHighlight;
                    canGrab = canShield = false;
                    grabState = grabStates.GRABBING;
                }
                // When already holding an object with telekinesis
                else if (grabState == grabStates.GRABBING)
                {
                    grabbedObject.GetComponent<Rigidbody>().useGravity = true;
                    grabbedObject.transform.parent = null;
                    grabState = grabStates.IDLE;
                }
            }
        }
        // Shield //
        // Shield Spawner target was hit with raycast
        else if (pressedShield && raycastResult == 2 && grabState == grabStates.IDLE)
        {
            if (shieldState == shieldStates.IDLE)
            {
                for (int i = 0; i < shieldDebris.Length; i++)
                {
                    Vector3 spawnPos = hitPosition + new Vector3(UnityEngine.Random.Range(-shieldGrabSize.x / 2, shieldGrabSize.x / 2), 
                        UnityEngine.Random.Range(-shieldGrabSize.y / 2, shieldGrabSize.y / 2), UnityEngine.Random.Range(-shieldGrabSize.z / 2, shieldGrabSize.z / 2));
                    // Instantiate random debris elements assigned in the inspector
                    shieldDebris[i] = Instantiate(shieldDebrisModel[UnityEngine.Random.Range(0, shieldDebrisModel.Length)], spawnPos, highlightedObject.transform.rotation);
                    
                    shieldPos[i] = new Vector3(UnityEngine.Random.Range(-shieldSize.x / 2, shieldSize.x / 2), 
                        UnityEngine.Random.Range(-shieldSize.y / 2, shieldSize.y / 2), UnityEngine.Random.Range(-shieldSize.z / 2, shieldSize.z / 2));
                }
                shieldState = shieldStates.SHIELD_ACTIVATED;
            }
            else if (shieldState == shieldStates.SHIELD_ACTIVATED)
            {
                foreach (var i in shieldDebris)
                {
                    Destroy(i);
                }
                shieldState = shieldStates.IDLE;
            }
        }
        
        // Used for shield
        playerParentRotation = shieldPosition.parent.rotation;
    }

    void FixedUpdate()
    {
        // Pulling throwable object
        if (grabState == grabStates.GRABBING)
        {
            grabObject();
        }

        // Throwing throwable object
        if (grabState == grabStates.GRABBED)
        {
            grabbedObject.transform.position = grabPosition.transform.position;
            grabbedObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            if (pressedGrab)
            {
                throwObject();
                pressedGrab = false; 
            }
        }

        // Pulling shield debris
        if (shieldState == shieldStates.SHIELD_ACTIVATED)
        {
            grabShield();
        }

        // Throwing shield debris
        if (shieldState == shieldStates.SHIELD_ACTIVE)
        {
            for (int i = 0; i < shieldDebrisNumber; i++)
            {
                shieldDebris[i].transform.rotation = Quaternion.Euler(shieldDebris[i].transform.rotation.x, playerParentRotation.y, shieldDebris[i].transform.rotation.z);
                shieldDebris[i].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            }
            if (pressedShield)
            {
                throwShield();
                pressedShield = false;
            }
        }
        // Reset values for users keypresses every fixed update
        pressedGrab = pressedShield = false;
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
                canGrab = true;
                canShield = false;
                return 1;
            }
            else if (highlightedObject.CompareTag("ShieldSpawnable"))
            {
                hitPosition = raycastHit.point;
                canGrab = false;
                canShield = true;
                return 2;
            }
        }
        canGrab = canShield = false;
        return 0;
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

    private void grabObject()
    {
        float distance = distanceCalculator(grabbedObject.transform.position,grabPosition.transform.position);
        if (distance > grabPositionThreshold)
        {
            // Pulls object based on designers choice
            if (pullMode == pullingModes.ADDFORCE)
            {
                Vector3 pullDirection = grabPosition.transform.position - grabbedObject.transform.position;
                Vector3 pullingForce = pullDirection.normalized * grabPullForce;
                grabbedObject.GetComponent<Rigidbody>().AddForce(pullingForce, ForceMode.Force);
            }
            else if (pullMode == pullingModes.MOVETOWARDS)
            {
                grabbedObject.transform.position = Vector3.MoveTowards(grabbedObject.transform.position,
                    grabPosition.transform.position, grabPullForce);
            }
            
            grabbedObject.GetComponent<Rigidbody>().useGravity = false;
            grabbedObject.transform.parent = GameObject.Find("TelekinesisPos").transform;
        }
        else
        {
            grabState = grabStates.GRABBED;
        }
    }

    private void throwObject()
    {
        canGrab = true;
        grabState = grabStates.IDLE;
        grabbedObject.transform.parent = null;
        grabbedObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        grabbedObject.GetComponent<Rigidbody>().velocity = Input.mousePosition * grabThrowForce;
        grabbedObject.GetComponent<Rigidbody>().useGravity = true;
        grabbedObject = null;
    }

    private void grabShield()
    {
        for (int i = 0; i < shieldDebrisNumber; i++)
        {
            Vector3 endPoint = shieldPosition.transform.position + shieldPos[i];
            float distance = distanceCalculator(shieldDebris[i].transform.position,endPoint);
            if (distance > shieldPositionThreshold)
            {
                // Pulls object based on designers choice
                if (pullMode == pullingModes.ADDFORCE)
                {
                    Vector3 pullDirection = endPoint - shieldDebris[i].transform.position;
                    Vector3 pullingForce = pullDirection.normalized * shieldPullForce;
                    shieldDebris[i].GetComponent<Rigidbody>().AddForce(pullingForce, ForceMode.Force);
                }
                else if (pullMode == pullingModes.MOVETOWARDS)
                {
                    shieldDebris[i].transform.position = Vector3.MoveTowards(shieldDebris[i].transform.position, endPoint, shieldPullForce);
                }
                
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
                shieldState = shieldStates.SHIELD_ACTIVE;
            }
        }
    }

    private void throwShield()
    {
        canShield = true;
        shieldState = shieldStates.IDLE;
        for (int i = 0; i < shieldDebrisNumber; i++)
        {
            shieldDebris[i].transform.parent = null;
            shieldDebris[i].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            shieldDebris[i].GetComponent<Rigidbody>().velocity = transform.forward * shieldThrowForce;
            shieldDebris[i].GetComponent<Rigidbody>().useGravity = true;
            shieldDebris[i].GetComponent<Collider>().enabled = true;
        }
    }
    
    private float distanceCalculator(Vector3 objectPulled, Vector3 destination)
    {
        return Vector3.Distance(objectPulled, destination);
    }
}

