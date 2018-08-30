using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRStandardAssets.Utils;

public class Stage5 : MonoBehaviour {

    private IntroSessionManager m_Manager;                          // A reference to the manager script which acts as a library for the tutorial
    private RaycasterVR m_RaycasterScript;                       // Reference to the script used for raycasting from the user's controls


    public static Stage5 s_Instance = null;                         // Singleton instance of this class

    [SerializeField] private Dialogue m_DialogueInstructions;       // Dialogue object holding the audio and text instructions for the scene
    [SerializeField] private Dialogue m_DialogueFail;               // Dialogue object holding audio and text for scene failures


    private bool m_IsBallThrown;                                    // Flag to check if ball has been thrown 
    private bool m_IsEnd;                                           // Flag to check if stage is completed
    private int m_BallDroppedCount = 0;                             // Check How many times the ball has been dropped



    // Use this for initialization
    void Start() {
        if (s_Instance == null) {
            s_Instance = this;
            m_Manager = IntroSessionManager.s_Instance;
            m_RaycasterScript = IntroSessionManager.s_RaycasterScript;     // Setting m_RaycasterScript 

            // Start by making the robot give directions and then momentarilly dropping the ball
            Run();
        }
        else {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Called when the script is enabled, subscribes to events triggered in VREyeRaycaster
    /// </summary>
    private void OnEnable() {
        RaycasterVR.e_ObjectWasThrown += CompleteStage;
        RaycasterVR.e_ObjectWasDropped += BallDropped;
        RaycasterVR.e_ObjectWasPickedUp += BallPickedUp;
    }

    /// <summary>
    /// Called when the script is enabled, unsubscribes to events triggered in VREyeRaycaster
    /// </summary>
    private void OnDisable() {
        RaycasterVR.e_ObjectWasThrown -= CompleteStage;
        RaycasterVR.e_ObjectWasDropped -= BallDropped;
        RaycasterVR.e_ObjectWasPickedUp -= BallPickedUp;
    }


    /// <summary>
    /// Called when the ball is thrown and the event is triggered. Will end the stage.
    /// </summary>
    private void CompleteStage() {
        // called when the ball is thrown from VREyeRaycaster
        m_IsBallThrown = true;
        m_IsEnd = true;
        EndScene();
    }

    /// <summary>
    /// Triggered by an event when the ball has been dropped
    /// </summary>
    private void BallDropped() {
        m_BallDroppedCount++;
    }

    /// <summary>
    /// Triggered by an event when the ball has been picked up
    /// </summary>
    private void BallPickedUp() {

    }

    // Update is called once per frame
    void Update() {

        // See if they are pointing at the ball and pressing any button other than the main trigger

        if (m_RaycasterScript.GetTarget() != null) {
            if (m_RaycasterScript.GetTarget().tag.Contains("_PickUp_") &&
                OVRInput.GetDown(OVRInput.Button.Any) &&
                !OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) &&
                !m_RaycasterScript.m_HoldingObject) {

                IntroSessionManager.s_Instance.Toast("Hold the trigger button down to hold the ball...", IntroSessionManager.c_TOAST_LONG);
            }

            // See if they are clicking on random things that arn't the ball
            if (!m_RaycasterScript.GetTarget().tag.Contains("_PickUp_") &&
                OVRInput.GetDown(OVRInput.Button.Any) &&
                !m_RaycasterScript.m_HoldingObject) {

                IntroSessionManager.s_Instance.Toast("You're clicking on the wrong object, try pointing at the ball...", IntroSessionManager.c_TOAST_LONG);
            }
        }

        if(m_BallDroppedCount % 3 == 0 && m_BallDroppedCount != 0) {
            IntroSessionManager.s_Instance.Toast("Be sure to hold the the trigger down to keep the ball held...", IntroSessionManager.c_TOAST_LONG);
        }
    }


    /// <summary>
    /// Controls the flow of the scene.
    /// </summary>
    private void Run(){
        m_Manager.GlobalMessage(m_DialogueInstructions.DialogueElements[0]);
        StartCoroutine(CheckForFails());
    }

    /// <summary>
    /// Waits and checks if the user hasn't picked up the ball yet and informs them to do so after 15 seconds
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckForFails(){
        // if they are holding the ball and after 15 seconds they havn't thrown it remind them how to
        yield return new WaitUntil(() => m_RaycasterScript.m_HoldingObject);
        yield return new WaitForSeconds(15f);
        if (!m_IsBallThrown){
            IntroSessionManager.s_Instance.Toast(m_DialogueFail.DialogueElements[1].DialogueText, IntroSessionManager.c_TOAST_LONG);
        }
    }

    /// <summary>
    /// Ends the scene and moves on to the next one
    /// </summary>
    private void EndScene(){
        if (m_IsEnd) {
            m_IsEnd = false;
            StopCoroutine(CheckForFails());
            IntroSessionManager.s_Instance.MoveToNextStage(1f);
        }
    }
}