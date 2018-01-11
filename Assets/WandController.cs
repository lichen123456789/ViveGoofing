using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandController : MonoBehaviour {

    private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
    public bool gripDown = false;
    public bool gripUp = false;
    public bool gripHeld = false;
    private Valve.VR.EVRButtonId trigButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
    public bool trigDown = false;
    public bool trigUp = false;
    public bool trigHeld = false;
    private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObject.index); } }
    private SteamVR_TrackedObject trackedObject;

	// Use this for initialization
	void Start () {
        trackedObject = GetComponent<SteamVR_TrackedObject>();
	}
	
	// Update is called once per frame
	void Update () {
		if(controller == null)
        {
            Debug.Log("Controller bork");
        }
        gripDown = controller.GetPressDown(gripButton);
        trigDown = controller.GetPressDown(trigButton);
        trigUp = controller.GetPressUp(trigButton);
        trigHeld = controller.GetPress(trigButton);

        if (trigDown)
        {
            Debug.Log("Grippin it baby");
        }
        if (trigUp)
        {
            Debug.Log("Not grippin it");
        }
    }
}
