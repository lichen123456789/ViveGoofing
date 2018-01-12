using System.Collections;
using UnityEngine;
using Obi;
using VRTK;

[System.Serializable]
public class InteractingInfo : MonoBehaviour
{
    [Header("Grab Options")]
    public bool grabbingEnabled;

    [Header("Pin Options")]
    public Vector3 pinOffset;
    [Range(0f, 1f)]
    public float stiffness;

    [HideInInspector]
    public bool isTouchingParticle;
    [HideInInspector]
    public int touchingParticle;
    [HideInInspector]
    public int? grabbedParticle;
    [HideInInspector]
    public int? pinIndex;
    [HideInInspector]
    public VRTK_InteractGrab interactGrab;
    [HideInInspector]
    public Collider collider;
    [HideInInspector]
    public ObiCollider obiCollider;
    [HideInInspector]
    public VRTK_ObiRopeInteraction currentInteraction;

    public void Clear()
    {
        isTouchingParticle = false;
    }
}

[RequireComponent(typeof(ObiSolver))]
public class VRTK_ObiRopeSolver : MonoBehaviour
{
    #region Fields

    [Tooltip("Check if using VRTK_InteractTouch custom collider")]
    public bool customColliders;
    public string colliderObjectName = "Head";

    [Header("Controller Info")]

    public InteractingInfo leftControllerInfo = new InteractingInfo();
    public InteractingInfo rightControllerInfo = new InteractingInfo();

    private ObiSolver solver;

    private GameObject leftController;
    private GameObject rightController;

    #endregion


    #region Mono Events

    private void Awake()
    {
        solver = GetComponent<ObiSolver>();

        VRTK_SDKManager.instance.AddBehaviourToToggleOnLoadedSetupChange(this);
    }

    private void OnDestroy()
    {
        VRTK_SDKManager.instance.RemoveBehaviourToToggleOnLoadedSetupChange(this);
    }

    private void Reset()
    {
        colliderObjectName = "Head";
    }

    private void OnEnable()
    {
        leftController = VRTK.VRTK_DeviceFinder.GetControllerLeftHand(false);
        rightController = VRTK.VRTK_DeviceFinder.GetControllerRightHand(false);

        if (customColliders)
        {
            SetupController(leftController, true);
            SetupController(rightController, false);
        }
        else
        {
           // leftController.GetComponent<VRTK_ControllerEvents>().ControllerModelAvailable += ControllerModelAvailiable;
           // rightController.GetComponent<VRTK_InteractTouch>().customColliderContainer = ControllerModelAvailiable;
        }

        if (!ReferenceEquals(solver, null))
        {
            solver.OnCollision += HandleSolverCollision;
        }
    }

    private void OnDisable()
    {
        if (!ReferenceEquals(solver, null))
        {
            solver.OnCollision -= HandleSolverCollision;
        }
    }

    #endregion


    #region Private Methods

    private bool SetupController(GameObject controller, bool isLeft)
    {
        if (controller.transform.childCount == 0) return false;

        Transform colliderObject = customColliders ? controller.transform.Find(colliderObjectName) : controller.transform.GetChild(0).Find(colliderObjectName);
        colliderObject.gameObject.layer = LayerMask.NameToLayer("Default");

        var collider = colliderObject.gameObject.GetComponent<ObiCollider>();

        if (isLeft)
        {
            if (ReferenceEquals(collider, null))
            {
                leftControllerInfo.obiCollider = colliderObject.gameObject.AddComponent<ObiCollider>();
            }
            else
            {
                leftControllerInfo.obiCollider = collider;
            }

            leftControllerInfo.collider = colliderObject.GetComponent<Collider>();

            leftControllerInfo.interactGrab = controller.GetComponent<VRTK_InteractGrab>();
            leftControllerInfo.interactGrab.GrabButtonPressed += HandleGrabButtonPressed;
            leftControllerInfo.interactGrab.GrabButtonReleased += HandleGrabButtonReleased;
        }
        else
        {
            if (ReferenceEquals(collider, null))
            {
                rightControllerInfo.obiCollider = colliderObject.gameObject.AddComponent<ObiCollider>();
            }
            else
            {
                rightControllerInfo.obiCollider = collider;
            }

            rightControllerInfo.collider = colliderObject.GetComponent<Collider>();

            rightControllerInfo.interactGrab = controller.GetComponent<VRTK_InteractGrab>();
            rightControllerInfo.interactGrab.GrabButtonPressed += HandleGrabButtonPressed;
            rightControllerInfo.interactGrab.GrabButtonReleased += HandleGrabButtonReleased;
        }

        return true;
    }

    #endregion


    #region Controller Events

    private void ControllerModelAvailiable(object sender, ControllerInteractionEventArgs e)
    {
        StartCoroutine(ModelLoaded(sender as VRTK_ControllerEvents));
    }

    private IEnumerator ModelLoaded(VRTK_ControllerEvents events)
    {
        yield return new WaitForEndOfFrame();

        bool succeeded = true;

        if (ReferenceEquals(events.gameObject, leftController))
        {
            succeeded = SetupController(events.gameObject as GameObject, true);
        }
        else if (ReferenceEquals(events.gameObject, rightController))
        {
            succeeded = SetupController(events.gameObject as GameObject, false);
        }

        if (!succeeded)
            StartCoroutine(ModelLoaded(events as VRTK_ControllerEvents));
    }

    private void HandleGrabButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (ReferenceEquals(sender, leftControllerInfo.interactGrab) && leftControllerInfo.grabbingEnabled)
        {
            if (leftControllerInfo.isTouchingParticle)
            {
                // Get actor and apply constraint
                leftControllerInfo.currentInteraction.GrabRope(leftControllerInfo);
            }
        }
        else if (ReferenceEquals(sender, rightControllerInfo.interactGrab) && rightControllerInfo.grabbingEnabled)
        {
            if (rightControllerInfo.isTouchingParticle)
            {
                // Get actor and apply constraint
                rightControllerInfo.currentInteraction.GrabRope(rightControllerInfo);
            }
        }
    }

    private void HandleGrabButtonReleased(object sender, ControllerInteractionEventArgs e)
    {
        if (rightControllerInfo.grabbedParticle != null && ReferenceEquals(sender, rightControllerInfo.interactGrab) && rightControllerInfo.grabbingEnabled)
        {
            rightControllerInfo.currentInteraction.ReleaseRope(rightControllerInfo);

            if (leftControllerInfo.pinIndex.HasValue && leftControllerInfo.isTouchingParticle && leftControllerInfo.pinIndex > rightControllerInfo.pinIndex)
                leftControllerInfo.pinIndex--;

            rightControllerInfo.grabbedParticle = null;
            rightControllerInfo.pinIndex = null;

        }
        else if (leftControllerInfo.grabbedParticle != null && ReferenceEquals(sender, leftControllerInfo.interactGrab) && leftControllerInfo.grabbingEnabled)
        {
            leftControllerInfo.currentInteraction.ReleaseRope(leftControllerInfo);

            if (rightControllerInfo.pinIndex.HasValue && rightControllerInfo.isTouchingParticle && rightControllerInfo.pinIndex > leftControllerInfo.pinIndex)
                rightControllerInfo.pinIndex--;

            leftControllerInfo.grabbedParticle = null;
            leftControllerInfo.pinIndex = null;
        }
    }

    #endregion


    #region Obi Solver Events

    private void HandleSolverCollision(object sender, Obi.ObiSolver.ObiCollisionEventArgs e)
    {
        if (e == null || e.contacts == null || !rightControllerInfo.grabbingEnabled && !leftControllerInfo.grabbingEnabled) return;

        // Reset every frame
        leftControllerInfo.Clear();
        rightControllerInfo.Clear();

        for (int i = 0; i < e.contacts.Length; ++i)
        {
            var contact = e.contacts[i];
            var pia = solver.particleToActor[e.contacts[i].particle];
            var actor = pia.actor;
            var particleIndex = pia.indexInActor;

            // make sure this is an actual contact
            if (contact.distance < 0.01f)
            {
                Collider collider = ObiColliderBase.idToCollider[contact.other] as Collider;

                if (ReferenceEquals(collider, leftControllerInfo.collider))
                {
                    leftControllerInfo.isTouchingParticle = true;
                    leftControllerInfo.touchingParticle = particleIndex;
                    leftControllerInfo.currentInteraction = actor.gameObject.GetComponent<VRTK_ObiRopeInteraction>();
                }
                else if (ReferenceEquals(collider, rightControllerInfo.collider))
                {
                    rightControllerInfo.isTouchingParticle = true;
                    rightControllerInfo.touchingParticle = particleIndex;
                    rightControllerInfo.currentInteraction = actor.gameObject.GetComponent<VRTK_ObiRopeInteraction>();
                }
            }
        }
    }

    #endregion

}