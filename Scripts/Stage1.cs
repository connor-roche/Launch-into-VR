using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRStandardAssets.Utils;

public class Stage1 : MonoBehaviour {

    private RaycasterVR m_RaycasterScript;                       // Reference to the script used for raycasting from the user's controls
    private IntroSessionManager m_Manager;                          // A reference to the manager script which acts as a library for the tutorial

    public static Stage1 s_Instance;                                // Instance of singleton class Stage1

    [SerializeField] private Dialogue m_DialogueInstructions;       // Dialogue object holding the audio and text instructions for the scene
    [SerializeField] private Dialogue m_DialogueFail;               // Dialogue object holding audio and text for scene failures
    [SerializeField] private GameObject[] 
        m_ArrowsObjectList = new GameObject[4];                     // GameObject array containing all of the arrows
    [SerializeField] private Transform m_CenterPosition;            // Location directly in front of the users view
    [SerializeField] private float m_ProgressBarLoadTime = 2f;      // Total time it should take to fill up the progress bar
    [SerializeField] private float m_Loaded = 0;                    // Current amount filled in the progress bar
    [SerializeField] private float m_ReticleScaleFactor;            // Factor to scale up reticle


    private Image m_LoadingBar;                                     // The radial progress bar image
    private string[] m_ArrowsList;                                  // String array containing all the names of the arrows
    private int i_CurrentArrow;                                     // The arrow that is supposed to be looked at next
    private GameObject m_Target;                                    // the GameObject the user is looking at, if applicable
    private bool m_WasLookingAtArrow;                               // Whether or not the puser was just looking at an arrow
    private bool m_IsStageComplete;                                 // Whether or not the stage has been completed
    private Hashtable m_ArrowMap;                                   // Maps the arrow objects to their index
    private bool m_LookingAtCompleted;                              // Was the user looking at a completed arrow
    private GameObject m_Reticle;                                   // Reference to the entire Reticle
    private GameObject m_CenterEyeAnchor;                           // Reference to the CenterEyeAnchor on the OVRCameraRig in Scene0




    // Use this for initialization
    void Start () {

        if (s_Instance == null) {

            s_Instance = this;
            m_Manager = IntroSessionManager.s_Instance;
            m_CenterEyeAnchor = m_Manager.GetCenterEyeAnchor();
            m_RaycasterScript = m_CenterEyeAnchor.GetComponent<RaycasterVR>();
            m_LoadingBar = m_Manager.GetLoadingBar();
            m_Reticle = m_Manager.GetReticle();

            m_Manager.GetControllerModel().SetActive(false);                                                // Make sure the controller model is hidden since it is shown after this scene.
            m_ArrowsList = new string[4] { "ArrowLeft", "ArrowUp", "ArrowRight", "ArrowDown"};              // The order in which the user is supposed to look at the arrows.
            i_CurrentArrow = 0;                                                                             // Starts at the left arrow
            m_RaycasterScript.m_OverrideDefaultReticleControls = true;                                      // Override the default controls of the reticle
            FaceArrowsToUser();                                                                             // Face the arrows towards the user

            m_Manager.GlobalMessage(m_DialogueInstructions.DialogueElements[0]);
        } else {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update() {

        if(!m_IsStageComplete && i_CurrentArrow > 3) {
            // User looked at all 4 arrows for the first time
            m_RaycasterScript.m_OverrideDefaultReticleControls = false;      // give control of the reticle back
            m_Manager.MoveToNextStage(2f);
            m_IsStageComplete = true;
        }

        m_Target = m_RaycasterScript.GetTarget();      // Gets the object that the user is looking at
        if (m_Target != null && !m_IsStageComplete) { 

            if (m_Target.tag.Contains("Arrow")) {
                // User is looking at an arrow
                m_WasLookingAtArrow = true;

                if (Array.IndexOf(m_ArrowsObjectList, m_Target.transform.parent.gameObject) < i_CurrentArrow) {
                    // User already looked at this arrow
                    m_LoadingBar.fillAmount = 1f;
                    m_LookingAtCompleted = true;
                }
                else {
                    m_LookingAtCompleted = false;
                    LoadProgressBar();
                }

            } else if(m_WasLookingAtArrow){
                // The user was looking at an arrow and looked away
                ResetProgressBar();
            }

            if(m_LoadingBar.fillAmount == 1 && !m_LookingAtCompleted) { 
                // 100% loaded
                if (m_Target.tag.Contains(m_ArrowsList[i_CurrentArrow])) {
                    // User is looking at the correct arrow
                    RegisterArrow();
                } else {
                    // User was looking at the wrong arrow
                    m_Manager.ReticleInvalidOperation();
                }
            } 
        } else if (m_WasLookingAtArrow) {
            // The user was looking at an arrow and looked away
            ResetProgressBar();

        }

    } // end update


    /// <summary>
    /// Registers the arrow as successfully looked at and increments <c>i_CurrentArrow</c>
    /// </summary>
    private void RegisterArrow() {       
        GameObject arrow = m_ArrowsObjectList[i_CurrentArrow];
        arrow.GetComponentInChildren<Renderer>().material.SetColor("_Color", Color.green);
        Destroy(Instantiate(Resources.Load("ArrowReachedEffect"), arrow.transform.position, arrow.transform.rotation), 2f);
        i_CurrentArrow++; // move to the next arrow
    }

   /// <summary>
   /// Resets the progress bar back to 0.
   /// </summary>
    private void ResetProgressBar() {
        m_Loaded = 0;
        m_LoadingBar.fillAmount = 0;
        m_WasLookingAtArrow = false;
    }

    /// <summary>
    /// Precondition: Expecting to be called in Update.
    /// Loads the progress bar by one step. 
    /// </summary>
    private void LoadProgressBar() {
        if (m_Loaded < m_ProgressBarLoadTime)
            m_Loaded += (Time.deltaTime);
        m_LoadingBar.fillAmount = m_Loaded / m_ProgressBarLoadTime;
    }

    /// <summary>
    /// Iterates through the arrows and makes them face the user
    /// </summary>
    private void FaceArrowsToUser() {
        foreach(GameObject arrow in m_ArrowsObjectList) {
            arrow.transform.LookAt(m_CenterEyeAnchor.transform);
        }
    }
}
