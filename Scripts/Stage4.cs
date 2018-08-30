using System.Collections;
using System.Collections.Generic;
using VRStandardAssets.Utils;
using UnityEngine;

public class Stage4 : MonoBehaviour {


    private RaycasterVR m_RaycasterScript;                                   // Reference to the script used for raycasting from the user's controls
    private IntroSessionManager m_Manager;                                   // A reference to the manager script which acts as a library for the tutorial

    public static Stage4 s_Instance = null;                                  // instance of stage 4 script

    [SerializeField] private Dialogue m_DialogueInstructions;                // Dialogue object holding the audio and text instructions for the scene
    [SerializeField] private Dialogue m_DialogueFail;                        // Dialogue object holding audio and text for scene failures


    private int m_ballDroppedCount = 0;                                      // Count for how many times the ball is dropped
    private bool m_isEnd;                                                    // is the stage complete
    private float m_timer;                                                   // Keep track of time between ball drop fails



    // Use this for initialization
    void Start () {
        // Make the script a singleton
        if(s_Instance == null) {
            s_Instance = this;
            m_Manager = IntroSessionManager.s_Instance;
            m_RaycasterScript = IntroSessionManager.s_RaycasterScript;
            m_timer = 0;

            // Spawn a ball for the user
            m_Manager.SpawnNewBall(); 

            // Display text and playback audio for dialogue, then wait for the audio to finish before continuing
            m_Manager.GlobalMessage(m_DialogueInstructions.DialogueElements[0]);

            // Open the trap door and reset the timer
            Debug.Log("STAGE4 CALLING OPENING SLIDING DOORS");
            m_Manager.OpenSlidingDoors();

        }
        else {
            Destroy(gameObject);
        }
	}

    private void OnEnable(){
        RaycasterVR.e_ObjectWasDropped += BallDropped;                 // Subscribe to events when the ball is dropped
        RaycasterVR.e_ObjectWasPickedUp += BallPickedUp;               // Subscribe to events when the ball is picked up
        TriggerStage4.e_Stage4Triggered += NotifyBallTrigger;          // Subscribe to event when the ball gets dropped through the trap door 
    }

    private void OnDisable(){
        // Unsubscribe to all events
        RaycasterVR.e_ObjectWasDropped -= BallDropped;                  
        RaycasterVR.e_ObjectWasPickedUp -= BallPickedUp;
        TriggerStage4.e_Stage4Triggered -= NotifyBallTrigger;
    }


    /// <summary>
    /// Called when the ball has been dropped.
    /// </summary>
    private void BallDropped(){
        m_ballDroppedCount++;
    }

    /// <summary>
    /// Called when the ball is picked up
    /// </summary>
    private void BallPickedUp(){

    }


    // Update is called once per frame
    void Update()
    {
        m_timer += Time.deltaTime;          // Increment the timer

        if (m_RaycasterScript.GetTarget() != null) {
            // See if they are pointing at the ball and pressing any button other than the main trigger
            if (m_RaycasterScript.GetTarget().tag.Contains("_PickUp_") && OVRInput.GetDown(OVRInput.Button.Any) &&
                !OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) && !m_RaycasterScript.m_HoldingObject) {

                IntroSessionManager.s_Instance.Toast("Hold the trigger button down to hold the ball...", IntroSessionManager.c_TOAST_LONG);
            }

            // See if they are clicking on random things that arn't the ball
            if (!m_RaycasterScript.GetTarget().tag.Contains("_PickUp_") && OVRInput.GetDown(OVRInput.Button.Any) &&
                !m_RaycasterScript.m_HoldingObject) {

                IntroSessionManager.s_Instance.Toast("You're clicking on the wrong object, try pointing at the ball...", IntroSessionManager.c_TOAST_LONG);
            }
        }

        // If they are having trouble holding the ball then give them a tip
        if (m_ballDroppedCount % 3 == 0 && m_timer > 10f){
            IntroSessionManager.s_Instance.Toast("Be sure to hold the the trigger down to keep the ball held...", IntroSessionManager.c_TOAST_LONG);
            m_timer = 0;                // reset the timer
        }

       
    }


    /// <summary>
    /// Ends the stage, closes the trap doors, and advances to the next scene
    /// </summary>
    private void EndOfStage(){
        // If we are at the end of the scene, clean up and move to next stage
        if (m_isEnd){
            m_isEnd = false;
            m_Manager.CloseSlidingDoors();
            IntroSessionManager.s_Instance.MoveToNextStage(1f);
 
        }
    }

    /// <summary>
    /// Gets called when the user drops the ball through the trap doors
    /// </summary>
    public void NotifyBallTrigger() {
        m_isEnd = true;
        EndOfStage();
    }


}
