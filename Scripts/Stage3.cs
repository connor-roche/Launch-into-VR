using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRStandardAssets.Utils;

public class Stage3 : MonoBehaviour {

    private IntroSessionManager m_Manager;                                  // A reference to the manager script which acts as a library for the tutorial
    private RaycasterVR m_RaycasterScript;                               // Reference to the script used for raycasting from the user's controls

    private readonly Vector3
        c_RobotLocationNearButton = new Vector3(2.68f, 3.13f, 6.22f);       // Location that the robot moves to 

    public static Stage3 s_Instance = null;

    [SerializeField] private Dialogue m_DialogueInstructions;               // Dialogue object holding the audio and text instructions for the scene
    [SerializeField] private Dialogue m_DialogueFail;                       // Dialogue object holding audio and text for scene failures

    private int m_ButtonClicks;                                             // Counts the number of times trigger is pressed
    private bool m_CanHitButton = true;                                     // allows user to register one click for ending the stage

    // Use this for initialization
    void Start () {
        if (s_Instance == null) {
            s_Instance = this;
            m_Manager = IntroSessionManager.s_Instance;
            m_RaycasterScript = IntroSessionManager.s_RaycasterScript;
            StartCoroutine(Run());                                                       // Begin the stage
        }
        else {
            Destroy(gameObject);
        }
	}

    private void OnEnable() {
        RaycasterVR.e_ButtonClicked += ButtonWasClicked;                                         // Subscribe to event for when the user clicks the button
    }

    private void OnDisable() {
        RaycasterVR.e_ButtonClicked -= ButtonWasClicked;                                         // Subscribe to event for when the user clicks the button
    }


    /// <summary>
    /// This gets called when the user clicks the button
    /// </summary>
    private void ButtonWasClicked() {
        if (m_CanHitButton)
        {
            m_CanHitButton = false;

            // End the stage
            m_Manager.HighlightButtonOff(m_Manager.GetTriggerButton());
            m_Manager.MoveBoxOutOfScene();
            m_Manager.MoveToNextStage(2f);
        }
        
    }

    // Update is called once per frame
    void Update () {

        // if the user is not pressing the trigger, highlight it so they know which one to press
        if (!OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger)) {
            m_Manager.HighlightButtonOn(m_Manager.GetTriggerButton());
        }

        // See if they are pointing at the ball and pressing any button other than the main trigger
        if (m_RaycasterScript.GetTarget() != null) {
            if (m_RaycasterScript.GetTarget().tag.Contains("_Stage3Button_") &&
                OVRInput.GetDown(OVRInput.Button.Any) &&
                !OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)) {
                IntroSessionManager.s_Instance.Toast("Be sure your clicking the trigger button...", IntroSessionManager.c_TOAST_LONG);
            }

            // See if they are clicking on random things that arn't the ball
            if (!m_RaycasterScript.GetTarget().tag.Contains("_Stage3Button_") && OVRInput.GetDown(OVRInput.Button.Any)) {
                IntroSessionManager.s_Instance.Toast("You're clicking on the wrong object, try pointing at the button...", IntroSessionManager.c_TOAST_LONG);
            }
        }
    }

    /// <summary>
    /// Controls the flow of the scene.
    /// </summary>
    /// <returns>A reference to the coroutine</returns>
    private IEnumerator Run() {

        m_Manager.HighlightButtonOn(m_Manager.GetTriggerButton());
        m_Manager.GlobalMessage(m_DialogueInstructions.DialogueElements[0]);

        // wait for half of the dialogue to play before beginning to open the doors
        yield return new WaitForSeconds(m_DialogueInstructions.DialogueElements[0].PlayBackSoundFile.length / 2); 

        // Open the sliding doors and bring the box into the scene
        m_Manager.OpenSlidingDoors();
        m_Manager.BringBoxIntoScene();

        // Wait until audio finishes playing before moving the robot
        yield return new WaitForSeconds(m_DialogueInstructions.DialogueElements[0].PlayBackSoundFile.length / 2 + 1f);            
        m_Manager.MoveRobotToPosition(c_RobotLocationNearButton);   // Move the robot near the button
        
    }
}
