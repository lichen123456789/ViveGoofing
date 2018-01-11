using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPickup : MonoBehaviour {
    private Valve.VR.EVRButtonId trigButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
    private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObject.index); } }
    private SteamVR_TrackedObject trackedObject;

    [SerializeField]
    private GameObject obj;
    private FixedJoint fjoint;

    private bool throwing = false;
    private Rigidbody heldObject;

    // Use this for initialization
    void Start () {
        trackedObject = GetComponent<SteamVR_TrackedObject>();
        fjoint = GetComponent<FixedJoint>();

	}
	
	// Update is called once per frame
	void Update () {
		if (controller == null)
        {
            Debug.Log("Controller not initialized");
        }
        var device = SteamVR_Controller.Input((int)trackedObject.index);
        if (controller.GetPressDown(trigButton))
        {
            Debug.Log("press");
            pickUpObject();
        }
        if (controller.GetPressUp(trigButton))
        {
            dropObject();
        }
	}

    private void FixedUpdate()
    {
        if (throwing)
        {
            //throwing = false;
            Transform origin;
            if (trackedObject.origin != null)
            {
                origin = trackedObject.origin;
            }
            else
            {
                origin = trackedObject.transform.parent;
            }

            if (origin != null)
            {
                heldObject.velocity = origin.TransformVector(controller.velocity);
                heldObject.angularVelocity = origin.TransformVector(controller.angularVelocity);
            }

            throwing = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        obj = null;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Pickupable"))
        {
            obj = other.gameObject;
        }
    }
    void pickUpObject()
    {
        if(obj != null)
        {
            
            fjoint.connectedBody = obj.GetComponent<Rigidbody>();
            throwing = false;
            heldObject = null;
        }
        else
        {
            fjoint.connectedBody = null;
        }
    }

    void dropObject()
    {
        if (fjoint.connectedBody != null)
        {
            
            heldObject = fjoint.connectedBody;
            fjoint.connectedBody = null;
            throwing = true;
        }

    }
}
