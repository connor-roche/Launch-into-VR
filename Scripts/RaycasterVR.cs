using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace VRStandardAssets.Utils
{
    // In order to interact with objects in the scene
    // this class casts a ray into the scene and if it finds
    // a VRInteractiveItem it exposes it for other classes to use.
    // This script should be generally be placed on the camera.
    public class RaycasterVR : MonoBehaviour {

        // Events
        public delegate void BallThrown();
        public static event BallThrown e_ObjectWasThrown;                 // Event for when a ball was thrown

        public delegate void BallPickedUp();
        public static event BallPickedUp e_ObjectWasPickedUp;             // Event for when a ball was picked up

        public delegate void BallDropped();
        public static event BallDropped e_ObjectWasDropped;               // Event for when a ball was dropped

        public delegate void Stage3ButtonClicked();
        public static event Stage3ButtonClicked e_ButtonClicked;        // Event for when a the button was clicked

        // end Events

        [SerializeField] private Transform m_Camera;                    // The camera to raycast out of (if not casting from controller)
        [SerializeField] private LayerMask m_ExclusionLayers;           // Layers to exclude from the raycast.
        [SerializeField] private Reticle m_Reticle;                     // The reticle, if applicable.
        [SerializeField] private bool m_ShowDebugRay;                   // Optionally show the debug ray.
        [SerializeField] private float m_DebugRayLength = 5f;           // Debug ray length.
        [SerializeField] private float m_DebugRayDuration = 1f;         // How long the Debug ray will remain visible.
        [SerializeField] private float m_RayLength = 500f;              // How far into the scene the ray is cast.
        [SerializeField] private GameObject m_Direction;                // direction to throw object
        [SerializeField] private float force = 1000f;                   // force applied to object being thrown.
        [SerializeField] private Transform m_TrackingSpace = null;      // Tracking space (for line renderer)
        [SerializeField] private GameObject m_ControllerModel;          // changed from private to protected to access in other scripts

        public bool m_HoldingObject = false;                            // Whether or not the user is holding an object
        public bool m_OverrideDefaultReticleControls;                   // if True the efault reticle controls will be overridden

        private Vector3 m_OffsetVector;                                 // Vector to keep the distance fir tge ball 
        private GameObject m_Target;                                    // The object that the user is pointing at (with controller or gaze)
        private GameObject m_ObjectHeld;                                // The current object being held by the user
        private float m_TriggerPullTimer = 0f;                          // A timer
        private bool m_ObjectReleased = true;                           // Whether or not an object was released
        private float m_ReticleStateTimer = 0f;                         // Timer to keep track of the reticle

        private enum ReticleState { DefaultState, HoverState, InteractingState, InvalidState};      // Enumerated type to classify the state of the reticle
        private ReticleState m_CurrentReticleState;                                                 // The current state of the reticle

        // Used for initialization
        private void Start() {
            m_CurrentReticleState = ReticleState.DefaultState;      // Reticle will begin in its default state
            IntroSessionManager.s_Instance.ReticleSetDefaultState();
        }


        // Called once per frame
        private void Update(){
            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger)) {
                // if the trigger button is pressed, increment the timer.
                m_TriggerPullTimer += Time.deltaTime;
            }
            else {
                // This means the user let go of the trigger, reset the timer.
                m_TriggerPullTimer = 0;
            }

            // Do the raycast
            Raycast();
        }


      /// <summary>
      /// Function will emit a raycast from the camera or controller. It will set <c>m_Target</c> to the current object being raycast on.
      /// Inside this function is also where the events are triggered and objects are interacted with (held/thrown/clicked on).
      /// </summary>
        private void Raycast(){

            // Show the debug ray if required
            if (m_ShowDebugRay){
                Debug.DrawRay(m_Camera.position, m_Camera.forward * m_DebugRayLength, Color.blue, m_DebugRayDuration);
            }

            // Create a ray that points forwards from the camera.
            Ray ray = new Ray(m_Camera.position, m_Camera.forward);
            RaycastHit hit;

            Vector3 worldStartPoint = Vector3.zero;
            Vector3 worldEndPoint = Vector3.zero;

            // If the controller is connected and we are past Scene 1, emit the raycast from the controller instead of the camera
            if (ControllerIsConnected && m_TrackingSpace != null && IntroSessionManager.s_Instance.GetCurrentScene() > 1 ) {
                if (!m_ControllerModel.activeSelf)
                {
                    m_ControllerModel.SetActive(true);
                }
                m_ControllerModel.SetActive(true);
                Matrix4x4 localToWorld = m_TrackingSpace.localToWorldMatrix;
                Quaternion orientation = OVRInput.GetLocalControllerRotation(Controller);

                Vector3 localStartPoint = OVRInput.GetLocalControllerPosition(Controller);
                Vector3 localEndPoint = localStartPoint + ((orientation * Vector3.forward) * 500.0f);

                worldStartPoint = localToWorld.MultiplyPoint(localStartPoint);
                worldEndPoint = localToWorld.MultiplyPoint(localEndPoint);

                // Create new ray
                ray = new Ray(worldStartPoint, worldEndPoint - worldStartPoint);
            }

            // Do the raycast forwards to see if we hit an interactive item
            if (Physics.Raycast(ray, out hit, m_RayLength, ~m_ExclusionLayers))
            {
                m_Target = hit.collider.gameObject;       // the target hit by the raycast

                m_OffsetVector = m_Target.transform.position - OVRInput.GetLocalControllerPosition(Controller);


                // Check if the reticle is in an interacting state
                if (m_CurrentReticleState == ReticleState.InteractingState && !m_HoldingObject) {
                    m_ReticleStateTimer += Time.deltaTime;
                    if (m_ReticleStateTimer >= 0.5f) {
                        // Timer is finished, revert back to default
                        IntroSessionManager.s_Instance.ReticleSetDefaultState();
                        m_CurrentReticleState = ReticleState.DefaultState;
                        m_ReticleStateTimer = 0;
                    }
                }


                // check to see if the raycast is hitting anything we can interact with and was not already aiming at one
                if (m_Target.tag.Contains("_Interactable_")) {
                    // Target is interactable

                    if (!m_OverrideDefaultReticleControls) {
                        if (m_CurrentReticleState == ReticleState.DefaultState) {
                            // The reticle is in its default state on the current frame
                            // so put the reticle in a hover state.
                            IntroSessionManager.s_Instance.ReticleSetHoverState();
                            m_CurrentReticleState = ReticleState.HoverState;
                        }
                    }

                    // Check in here for trigger being pulled
                    if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)) {
                        // Add code here to check if user clicks the trigger while targeting an object
                        IntroSessionManager.s_Instance.ReticleSetInteracting(); // Set the reticle to its interacting state
                        m_CurrentReticleState = ReticleState.InteractingState;

                        //if (m_Target.tag.Contains("_EnterTagOnObjectHere_"))


                        if (m_Target.tag.Contains("_Stage3Button_")) {
                            // User clicked on the button
                            AudioSource buttonSound = m_Target.GetComponent<AudioSource>();
                            if (buttonSound != null) {
                                buttonSound.Play();
                            }

                            // Notify any subscribers that the button has been clicked.
                            if (e_ButtonClicked != null) {
                                e_ButtonClicked();
                            }
                        } // End checking for Stage3Button

                    }   // End checking when user pulled trigger
                } else {
                    // The target is not interactable
                    if(m_CurrentReticleState == ReticleState.HoverState && !m_OverrideDefaultReticleControls) {
                        // On this frame the user is not looking targeting/interacting with an interactable object and the user was on the last frame
                        // Set the reticle state to default
                        IntroSessionManager.s_Instance.ReticleSetDefaultState();
                        m_CurrentReticleState = ReticleState.DefaultState;
                    }
                }


                if (m_ObjectHeld) {
                    // Currently holding an object
                    if (OVRInput.GetDown(OVRInput.Button.DpadUp) || OVRInput.GetDown(OVRInput.Button.DpadLeft) || OVRInput.GetDown(OVRInput.Button.DpadRight)) {
                        // User swiped, throw ball forward. This works if the user swipes up, left, or right. We found this made it easier for the user to throw the ball forward 
                        ThrowObject(1);
                    }
                    else if (OVRInput.GetDown(OVRInput.Button.DpadDown)) {
                        // User swiped down, throw ball backwards (towards the user)
                        ThrowObject(-1);
                    }
                    if (!OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger)) {
                        //Trigger button is released
                        ReleaseObject();
                        m_ObjectReleased = true;
                    }
                }
                else {
                    // not currently holding an object
                    if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger)) {
                        // User pulled the trigger in
                        if (m_Target.tag.Contains("_PickUp_")) {
                            // object being aimed at can be picked up
                            if (m_ObjectReleased) {
                                // User has released the trigger since the last time and now held the trigger down (for this frame). Pick up the object
                                m_ObjectHeld = m_Target;
                                GrabObject();
                                m_ObjectReleased = false;
                            }
                        }
                    }
                    else {
                        // User is not pressing the trigger
                        m_ObjectReleased = true;
                    }   
                }
               
                // Something was hit, set at the hit position.
                if (m_Reticle)
                    m_Reticle.SetPosition(hit);
            }
            else
            {
                m_Target = null;
                // Position the reticle at default distance.
                if (m_Reticle)
                    m_Reticle.SetPosition(ray.origin, ray.direction);
            }
        }

        /// <summary>
        /// Throws an object
        /// Precondition: param dir is 1 or -1. Use 1 for forward and -1 for backward.
        /// </summary>
        /// <param name="dir">Direction to throw the object. Use 1 for forward and -1 for backward.</param>
        private void ThrowObject(int dir) {   
            Rigidbody rb = m_ObjectHeld.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            m_ObjectHeld.transform.parent = null;
            rb.AddForce(this.m_Direction.transform.forward * dir * force); // the forward direction of the controller * dir (1 or -1) *
            m_ObjectHeld = null;

           // Notify subsribers that an object was thrown
            if (e_ObjectWasThrown != null) {
                e_ObjectWasThrown();
            }
        }


        /// <summary>
        /// Grabs an object and makes it a child of the controller model.
        /// </summary>
        public void GrabObject() {
            // put the reticle in a interacting state.
            IntroSessionManager.s_Instance.ReticleSetInteracting();
            m_CurrentReticleState = ReticleState.InteractingState;

            // Grab the object
            m_ObjectHeld.GetComponent<Rigidbody>().isKinematic = true;
            m_ObjectHeld.transform.parent = m_ControllerModel.transform;
            m_ObjectHeld.transform.position = OVRInput.GetLocalControllerPosition(Controller) + m_OffsetVector;
            m_HoldingObject = true;

            // Notify subscribers that an object has been picked up
            if (e_ObjectWasPickedUp != null) {
                e_ObjectWasPickedUp();
            }
        }

        /// <summary>
        /// Releases object
        /// </summary>
        public void ReleaseObject() {
            IntroSessionManager.s_Instance.ReticleSetDefaultState();
            m_CurrentReticleState = ReticleState.DefaultState;
            m_ObjectHeld.GetComponent<Rigidbody>().isKinematic = false;
            m_ObjectHeld.transform.parent = null;
            m_ObjectHeld.transform.position = OVRInput.GetLocalControllerPosition(Controller) + m_OffsetVector;
            m_HoldingObject = false;
            m_ObjectHeld = null;
            // Notify subscribers that an object has been picked up
            if (e_ObjectWasDropped != null) {
                e_ObjectWasDropped();
            }
        }

        /// <summary>
        /// Returns if a controller is connected
        /// </summary>
        public bool ControllerIsConnected {
            get {
                OVRInput.Controller controller = OVRInput.GetConnectedControllers() & (OVRInput.Controller.LTrackedRemote | OVRInput.Controller.RTrackedRemote);
                return controller == OVRInput.Controller.LTrackedRemote || controller == OVRInput.Controller.RTrackedRemote;
            }
        }

        /// <summary>
        /// Returns the controller that is connected
        /// </summary>
        public OVRInput.Controller Controller {
            get {
                OVRInput.Controller controller = OVRInput.GetConnectedControllers();
                if ((controller & OVRInput.Controller.LTrackedRemote) == OVRInput.Controller.LTrackedRemote) {
                    return OVRInput.Controller.LTrackedRemote;
                } else if ((controller & OVRInput.Controller.RTrackedRemote) == OVRInput.Controller.RTrackedRemote) {
                    return OVRInput.Controller.RTrackedRemote;
                }
                return OVRInput.GetActiveController();
            }
        }

        /// <summary>
        /// Returns the object the raycast is pointing to.
        /// Postcondition: Could return null if not raycasting on an object.
        /// </summary>
        /// <returns>The target being pointed at.</returns>
        public GameObject GetTarget() {
            return m_Target;
        }
    }
}