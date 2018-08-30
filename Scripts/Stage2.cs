using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRStandardAssets.Utils;



public class Stage2 : MonoBehaviour {

    private IntroSessionManager m_Manager;                          // A reference to the manager script which acts as a library for the tutorial

    public static Stage2 s_Instance = null;                         // Singleton instance of this class

    [SerializeField] private Dialogue m_DialogueInstructions;       // Dialogue object holding the audio and text instructions for the scene
    [SerializeField] private Dialogue m_DialogueFail;               // Dialogue object holding audio and text for scene failures

    private int i_ErrorCounter = 0;                                 // used to count the times the user clicked the wrong button
    private int m_NumberOfTries = 2;                                // the amount of wrong clicks the user gets before a message shows
    private bool m_CheckRecenter = true;                            // if the user has recentered correctly 
    private bool m_IntroNotStarted = true;                          // If the scene has begun yet or not

    // Use this for initialization
    void Start() {
        if (s_Instance == null) {
            s_Instance = this;
            m_Manager = IntroSessionManager.s_Instance;
        }
        else {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update() {
        // Check if the user has their controller connected 
        if (OVRInput.IsControllerConnected(OVRInput.Controller.RTrackedRemote) || OVRInput.IsControllerConnected(OVRInput.Controller.LTrackedRemote)) {
            if (m_IntroNotStarted) {
                // Begin the scene
                m_Manager.HighlightButtonOn(m_Manager.GetHomeButton());
                m_Manager.GlobalMessage(m_DialogueInstructions.DialogueElements[0]);
                m_IntroNotStarted = false;
            }
        }
        else {
            m_Manager.GlobalMessage("Please connect your controller!");
        }

        //checks if other buttons are pressed for fail point
        if (OVRInput.GetDown(OVRInput.Button.PrimaryTouchpad) || OVRInput.GetDown(OVRInput.Button.Back) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)) {
            i_ErrorCounter++;
            if (i_ErrorCounter % m_NumberOfTries == 0)
                IntroSessionManager.s_Instance.Toast("Look down at the controller to see what button to press.", IntroSessionManager.c_TOAST_SHORT);
        }

        // Check if the user has recentered correctly 
        if (m_CheckRecenter)
        {
            if (OVRInput.GetControllerWasRecentered(OVRInput.Controller.RTrackedRemote) ||
         OVRInput.GetControllerWasRecentered(OVRInput.Controller.LTrackedRemote))
            {
                // End the scene
                m_CheckRecenter = false;
                m_Manager.HighlightButtonOff(m_Manager.GetHomeButton());
                m_Manager.MoveToNextStage(1f);
            }
        }
    }
}