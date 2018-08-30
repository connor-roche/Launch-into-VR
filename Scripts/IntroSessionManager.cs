using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRStandardAssets.Utils;

public class IntroSessionManager : MonoBehaviour {

    public static readonly float c_TOAST_EXTRA = 6f;
    public static readonly float c_TOAST_LONG = 4f;                 // Lengths of time to display toast message to the Message Board
    public static readonly float c_TOAST_SHORT = 2f;

    public static readonly Color c_COLOR_LIGHTGRAY = new Color(.8f, .8f, .8f, 1f);

    private readonly float c_DoorForwardSpeed = 0.1f;               // Speed at which the doors on the button box move forward
    private readonly float c_DoorSideSpeed = 0.8f;                  // Speed at which the doors on the button box move sideways

    public static IntroSessionManager s_Instance = null;            // Reference to this singleton class 
    public static RaycasterVR s_RaycasterScript;                    // Reference to the raycaster scribt

    [SerializeField] private Dialogue m_DialogueInstructions;       // Dialogue object holding the audio and text instructions for the scene
    [SerializeField] private Dialogue musicClips;                   // Music for the stage
    [SerializeField] private GameObject m_Reticle;                  // A reference to the reticle
    [SerializeField] private GameObject m_Robot;                    // A reference to the robot
    [SerializeField] private GameObject m_CenterEyeAnchor;          // A reference to the CenterEyeAnchor game object in the scene on the OVRCameraRig. i.e. the camera the user is looking through.
    [SerializeField] private GameObject m_BallPrefab;               // A prefab of the ball to create in the scenes.
    [SerializeField] private GameObject m_ControllerModel;          // Model of the controller
    [SerializeField] private GameObject m_StartingPlatform;         // Platform that brings the robot into the scene.    
    [SerializeField] private GameObject m_SlidingDoorRight;         // Right door on the sliding doors.
    [SerializeField] private GameObject m_SlidingDoorLeft;          // Left door on the sliding doors.
    [SerializeField] private GameObject m_HomeButton;               // Home button on the controller
    [SerializeField] private GameObject m_TriggerButton;            // Trigger button on the controller
    [SerializeField] private GameObject m_ButtonBox;                // The button box used in Scene 3.                      
    [SerializeField] private GameObject m_LeftDoorButtonBox;        // The left sliding door on the button box
    [SerializeField] private GameObject m_RightDoorButtonBox;       // The right sliding door on the button box
    [SerializeField] private GameObject m_Button;                   // The button on the button box
    [SerializeField] private GameObject m_PlatformRaisedPosition;   // The position of the platform once raised
    [SerializeField] private GameObject m_RobotIntroDest1;          // The starting location of the robot in the introduction to the scene           
    [SerializeField] private GameObject m_RobotDefaultPosition;     // The default location in the scenes for the robot.
    [SerializeField] private GameObject m_MessagePanel;             // The message panel used to display messages to the user
    [SerializeField] private GameObject m_MessageBoardBorder;       // The border of the MessageBoard
    [SerializeField] private Material m_HighlightMaterial;          // Material used to highlight buttons on the controller model
    [SerializeField] private AudioSource m_RobotAudioSource;        // Audio source for dialogue coming from the robot
    [SerializeField] private AudioSource m_StageMusic;              // Audio source for the scene's ambient music
    [SerializeField] private Text m_GlobalMessageBoardText;         // The text for the global message board
    [SerializeField] private Text m_ClockText;                      // The text for the clock in the scene
    [SerializeField] private bool m_TestMode;                       // Check this box in the inspector to load right to the scene specified by startScene    
    [SerializeField] private int m_StartScene;                      // The scene to start at in TestMode
    [SerializeField] private Image m_InvalidOperationImage;         // Image for international prohibition sign (circle with line through it)
    [SerializeField] private Image m_ReticleOuterCircle;            // The reticle outer circle
    [SerializeField] private Image m_ReticleInnerCircle;            // The inner circle of the reticle.
    [SerializeField] private Image m_LoadingBar;                    // The loading bar


    private int i_CurrentScene = 0;                                 // The current scene we are on                               
    private Animator m_RobotAnimator;                               // Animator controller for the robot's animations
    private string[] m_Scenes;                                      // String array of the scenes to load through
    private bool m_OpeningSlidingDoors;                             // Flag for opening the sliding doors
    private bool m_ClosingSlidingDoors;                             // Flag for closing the sliding doors
    private bool m_IntroIn;                                         // Flag for starting the introduction to the tutorial
    private bool m_IntroOut;                                        // Flag for ending the introduction to the tutorial
    private bool m_OpeningBox;                                      // Flag for opening the button box
    private bool m_ClosingBox;                                      // Flag for closing the button box
    private bool m_BringBoxIntoScene;                               // Flag to bring the button box into the scene  
    private bool m_MoveBoxOutOfScene;                               // Flag to bring the button box into the scene 
    private Vector3 m_PlatformStartPos;                             // Starting position of the platform
    private GameObject m_BallClone;                                 // Reference to the current ball that is spawned, if any                     
    private bool m_ShowingToast;                                    // Whether or not a toast is being shown
    private Hashtable m_MaterialMap;                                // Maps an object to its material
    private IEnumerator m_InvalidOperationCoroutine;                // Reference to the coroutine showing the circle with a line through it. That way it can be stopped and they dont stack up.    
    private IEnumerator m_ToastCoroutine = null;                    // Reference to the coroutine showing the current toast.
    private IEnumerator m_ReticleStateCoroutine;                    // Reference to the coroutine for resetting the state of the reticle.
    private string m_PrevMessage;                                   // Previous message being displayed on the message board. Used for when a toast is shown to save the state
    private Color m_PrevColor;                                      // Previous color of the message board. Used for when a toast is shown to save the state


    // Use this for initialization
    void Awake() {

        if (s_Instance == null) {
            s_Instance = this;
            ChangeMusic(0);
            m_Scenes = new string[7] { "Scene1", "Scene2", "Scene3", "Scene4", "Scene5", "Scene6", "EndScene" };    // The order of the scenes
            m_PlatformStartPos = m_StartingPlatform.transform.position;
            m_RobotAnimator = m_Robot.GetComponent<Animator>();
            m_MaterialMap = new Hashtable();
            s_RaycasterScript = m_CenterEyeAnchor.GetComponent<RaycasterVR>();

            if (m_TestMode) {
                i_CurrentScene = m_StartScene - 1;
            }
        }
        else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        StartCoroutine(TutorialIntroduction()); // begin the tutorial
    }

    // Update is called once per frame
    void Update() {
        m_ClockText.text = System.DateTime.Now.TimeOfDay.ToString();        // Set the time on the in-game clock

        // Handle the introduction to the tutorial
        if (m_IntroIn) {
            StartCoroutine(IntroIn());
        }
        else if (m_IntroOut) {
            IntroOut();
        }

        // Handling bringing the button box into/out of the scene
        if (m_BringBoxIntoScene) {
            DoBringBoxIntoScene();
        }
        else if (m_MoveBoxOutOfScene) {
            DoMoveBoxOutOfScene();
        }

        // Handle opening/closing the box
        if (m_OpeningBox) {
            OpeningButtonBox();
        }
        else if (m_ClosingBox) {
            ClosingButtonBox();
        }

        // handle the sliding doors on the floor
        if (m_OpeningSlidingDoors) {
            OpeningSlidingDoors();
        }
        else if (m_ClosingSlidingDoors) {
            ClosingSlidingDoors();
        }

        if (m_BallClone == null && i_CurrentScene > 3) {
            // on any scene after scene 3, if there is no ball currently, spawn a new one.
            SpawnNewBall();
        }
        
    } // end Update

    /// <summary>
    /// Begins the introduction to the tutorial
    /// </summary>
    /// <returns>An IEnumerator that can reference this specific coroutine</returns>
    private IEnumerator TutorialIntroduction() {
        OpenSlidingDoors();
        yield return new WaitForSeconds(1);
        m_IntroIn = true;
    }

    /// <summary>
    /// Driver function to advance to the next scene with a specified delay
    /// </summary>
    /// <param name="delay">The delay to add between scene transitions</param>
    public void MoveToNextStage(float delay = 0) {
        StartCoroutine(AdvanceStage(delay));
    }

    /// <summary>
    /// Method is used as a coroutine to advance through the stage 
    /// </summary>
    /// <param name="delay">The time delay between scene trasitions</param>
    /// <returns>An IEnumerator that can reference this specific coroutine</returns>
    private IEnumerator AdvanceStage(float delay) {
        yield return new WaitForSeconds(delay);

        Destroy(GameObject.Find("RootGameObject"));
        SceneManager.LoadScene(m_Scenes[i_CurrentScene], LoadSceneMode.Additive);
        i_CurrentScene++;
    }

    /// <summary>
    /// Spawns a new ball and destroys the old one if one exists
    /// </summary>
    public void SpawnNewBall() {
        if (m_BallClone != null) {
            Destroy(m_BallClone);
        }
        m_BallClone = Instantiate(m_BallPrefab);
    }

    /// <summary>
    /// Method triggers the box to be opened 
    /// </summary>
    public void OpenButtonBox() {
        m_OpeningBox = true;
    }

    /// <summary>
    /// Method triggers the box to close
    /// </summary>
    public void CloseButtonBox() {
        m_ClosingBox = true;
    }

    /// <summary>
    /// Method triggers opening the sliding doors
    /// </summary>
    public void OpenSlidingDoors() {
        Debug.Log("ISM OPENINGSLIDINGDOORS");
        m_OpeningSlidingDoors = true;
        m_ClosingSlidingDoors = false;
    }
    /// <summary>
    /// Method triggers closing the sliding doors
    /// </summary>
    public void CloseSlidingDoors() {
        Debug.Log("ISM closing doors");
        m_ClosingSlidingDoors = true;
        m_OpeningSlidingDoors = false;

    }

    /// <summary>
    /// Method triggers bringing the box into the scene
    /// </summary>
    public void BringBoxIntoScene() {
        m_BringBoxIntoScene = true;
    }

    /// <summary>
    /// Method triggers bringing the box out of the scene
    /// </summary>
    public void MoveBoxOutOfScene() {
        m_MoveBoxOutOfScene = true;
        OpenSlidingDoors();
    }

    /// <summary>
    /// Method gets called in update. Opens the sliding doors one step per call.
    /// </summary>
    private void OpeningSlidingDoors() {
        if (m_SlidingDoorLeft.transform.localPosition.x >= -2.2f) {
            if (m_SlidingDoorLeft.transform.localPosition.y <= 0.15f) {
                m_SlidingDoorLeft.transform.Translate(Vector3.up * Time.deltaTime * .1f);
                m_SlidingDoorRight.transform.Translate(Vector3.up * Time.deltaTime * .1f);
            }
            else {
                m_SlidingDoorLeft.transform.Translate(-Vector3.right * Time.deltaTime);
                m_SlidingDoorRight.transform.Translate(Vector3.right * Time.deltaTime);
            }
        }
        else {
            m_OpeningSlidingDoors = false;
        }
    }

    /// <summary>
    /// Method gets called in update. Closes the sliding doors one step per call.
    /// </summary>
    private void ClosingSlidingDoors() {

        if (m_SlidingDoorLeft.transform.localPosition.y >= -0.033f) {
            if (m_SlidingDoorLeft.transform.localPosition.x <= -1.165f) {
                m_SlidingDoorLeft.transform.Translate(Vector3.right * Time.deltaTime * 1f);
                m_SlidingDoorRight.transform.Translate(-Vector3.right * Time.deltaTime * 1f);
            }
            else {
                m_SlidingDoorLeft.transform.Translate(-Vector3.up * Time.deltaTime * .1f);
                m_SlidingDoorRight.transform.Translate(-Vector3.up * Time.deltaTime * .1f);
            }
        }
        else {
            m_ClosingSlidingDoors = false;
        }


    }

    /// <summary>
    /// Method displays a Toast on the MessageBoard 
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="length"></param>
    public void Toast(string msg, float length) {
        // store current state of dialog box in order to revert back to after toast message times out
        if (m_MessageBoardBorder.GetComponent<Renderer>().material.color != Color.red) {
            m_PrevMessage = m_GlobalMessageBoardText.text;
            m_PrevColor = m_MessageBoardBorder.GetComponent<Renderer>().material.color;
        }

        // put toast message and new color in dialog box
        m_GlobalMessageBoardText.text = msg;
        m_MessageBoardBorder.GetComponent<Renderer>().material.color = Color.red;
        if (m_ToastCoroutine != null) {
            StopCoroutine(m_ToastCoroutine);
        }
        // toast stays in box for period of time
        m_ToastCoroutine = ShowToastLength(msg, length);
        StartCoroutine(m_ToastCoroutine);
    }


    /// <summary>
    /// Shows a Toast message on the Message Board for a duration of time.
    /// <para>Returns an IEnumerator that can reference a specific coroutine.</para>
    /// </summary>
    /// <param name="msg">The message to show</param>
    /// <param name="length">The duration to show the message</param>
    /// <returns>An IEnumerator that can reference a specific coroutine.</returns>
    private IEnumerator ShowToastLength(string msg, float length) {
        m_ShowingToast = true;
        yield return new WaitForSeconds(length);
        // revert back to pre toast state
        m_GlobalMessageBoardText.text = m_PrevMessage;
        m_MessageBoardBorder.GetComponent<Renderer>().material.color = m_PrevColor;
        m_ShowingToast = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerator IntroIn() {
        m_StartingPlatform.transform.position = Vector3.MoveTowards(m_StartingPlatform.transform.position, m_PlatformRaisedPosition.transform.position, Time.deltaTime * 2f);
        m_Robot.transform.position = Vector3.MoveTowards(m_Robot.transform.position, m_RobotIntroDest1.transform.position, Time.deltaTime * 2f);
        m_Robot.transform.LookAt(m_CenterEyeAnchor.transform);
        if (m_StartingPlatform.transform.position.y >= 2.5f) {
            m_IntroIn = false;
            m_RobotAnimator.SetBool("ActivateRobot", true);
            if (!m_TestMode) {
                yield return new WaitForSeconds(2);
                foreach (DialogueElement d in m_DialogueInstructions.DialogueElements) {
                    GlobalMessage(d.DialogueText);
                    m_RobotAudioSource.clip = d.PlayBackSoundFile;
                    m_RobotAudioSource.Play();
                    yield return new WaitForSeconds(d.PlayBackSoundFile.length);
                }
            }
            MoveRobotToPosition(m_RobotDefaultPosition.transform.position, 3.5f);
            m_IntroOut = true;
        }
    }

    /// <summary>
    /// Moves the robot to the Vector3 location passed in the param
    /// </summary>
    /// <param name="position">The Vector3 location to move the robot to</param>
    public void MoveRobotToPosition(Vector3 position, float speed = 5f) {
        m_Robot.GetComponent<MoveRobot>().StartMoving(position, speed);
    }

    /// <summary>
    /// Controls the flow of the tutorial introduction
    /// </summary>
    private void IntroOut() {
        if (m_IntroOut) {
            m_StartingPlatform.transform.position = Vector3.MoveTowards(m_StartingPlatform.transform.position, m_PlatformStartPos, Time.deltaTime * .3f);
            if (m_StartingPlatform.transform.position.y <= 1.5f) {
                Destroy(m_StartingPlatform);
                m_IntroOut = false;
                CloseSlidingDoors();
                MoveToNextStage(0);
            }
        }
    }

    /// <summary>
    /// Moves the box into the scene
    /// </summary>
    private void DoBringBoxIntoScene() {
        m_ButtonBox.transform.Translate(Vector3.up * Time.deltaTime, Space.World);
        if (m_ButtonBox.transform.position.y >= 1f) {
            m_BringBoxIntoScene = false;
            OpenButtonBox();
            CloseSlidingDoors();
        }
    }

    /// <summary>
    /// Move the box out of the scene
    /// </summary>
    private void DoMoveBoxOutOfScene() {
        CloseButtonBox();

        m_ButtonBox.transform.Translate(Vector3.down * Time.deltaTime, Space.World);
        if (m_ButtonBox.transform.position.y <= -4f) {
            m_MoveBoxOutOfScene = false;
            Destroy(m_ButtonBox);
        }
    }

    /// <summary>
    /// Opens the doors on the box and brings the button out of the box
    /// </summary>
    private void OpeningButtonBox() {
        if (m_LeftDoorButtonBox.transform.localPosition.x < -6.3f) {
            if (m_LeftDoorButtonBox.transform.localPosition.z < 10.1f) {
                m_LeftDoorButtonBox.transform.Translate(-Vector3.forward * Time.deltaTime * c_DoorForwardSpeed, Space.World);
                m_RightDoorButtonBox.transform.Translate(-Vector3.forward * Time.deltaTime * c_DoorForwardSpeed, Space.World);
            }
            else {
                m_LeftDoorButtonBox.transform.Translate(-Vector3.right * Time.deltaTime * c_DoorSideSpeed, Space.World);
                m_RightDoorButtonBox.transform.Translate(Vector3.right * Time.deltaTime * c_DoorSideSpeed, Space.World);
            }
        }
        else if (m_Button.transform.localPosition.z < 10.8f) {
            m_Button.transform.Translate(-Vector3.forward * Time.deltaTime * c_DoorSideSpeed, Space.World);
        }
        else {
            m_OpeningBox = false;
        }
    }


    /// <summary>
    /// Method closes the doors on the button box
    /// </summary>
    private void ClosingButtonBox() {
        if (m_Button.transform.localPosition.z >= 8.29f) {
            m_Button.transform.Translate(Vector3.forward * Time.deltaTime * c_DoorSideSpeed, Space.World);
        }
        else if (m_LeftDoorButtonBox.transform.localPosition.z >= 9.895f) {
            if (m_LeftDoorButtonBox.transform.localPosition.x >= -7.073f) {
                m_LeftDoorButtonBox.transform.Translate(Vector3.right * Time.deltaTime * c_DoorSideSpeed, Space.World);
                m_RightDoorButtonBox.transform.Translate(-Vector3.right * Time.deltaTime * c_DoorSideSpeed, Space.World);
            }
            else {
                m_LeftDoorButtonBox.transform.Translate(Vector3.forward * Time.deltaTime * c_DoorForwardSpeed, Space.World);
                m_RightDoorButtonBox.transform.Translate(Vector3.forward * Time.deltaTime * c_DoorForwardSpeed, Space.World);
            }
        }
        else {
            m_ClosingBox = false;
        }
    }


    /// <summary>
    /// Displays a message to the user on the message board.
    /// </summary>
    /// <param name="msg">Message that will be displayed on the message board in the game.</param>
    public void GlobalMessage(string msg) {

        if (m_ShowingToast) {
            StopCoroutine(m_ToastCoroutine);
            m_MessageBoardBorder.GetComponent<Renderer>().material.color = m_PrevColor;
            m_ShowingToast = false;
        }
        m_GlobalMessageBoardText.text = msg;
    }

    /// <summary>
    /// Driver function to display a message and audio from the <c>DialogElement<c/> parameter.
    /// </summary>
    /// <param name="ele">The DialogElement containing the text and audio clip</param>
    public void GlobalMessage(DialogueElement ele) {
        m_GlobalMessageBoardText.text = ele.DialogueText;
        m_RobotAudioSource.clip = ele.PlayBackSoundFile;
        m_RobotAudioSource.Play();
    }

    public void ChangeMusic(int track) {
        if (m_StageMusic != null) {
            m_StageMusic.clip = musicClips.DialogueElements[track].PlayBackSoundFile;
            m_StageMusic.Play();
        }
    }


    public void HighlightButtonOn(GameObject obj) {
        Renderer r = obj.GetComponent<Renderer>();
        if (!m_MaterialMap.ContainsKey(obj))
            m_MaterialMap.Add(obj, r.material);

        r.material = m_HighlightMaterial;

    }

    /// <summary>
    /// Turns glow effect off of obj 
    /// </summary>
    /// <param name="obj">GameObject to turn on glow.</param>
    public void HighlightButtonOff(GameObject obj) {
        if (m_MaterialMap[obj] is Material) {
            Material oldMat = m_MaterialMap[obj] as Material;
            obj.GetComponent<Renderer>().material = oldMat;
        }
    }

    /// <summary>
    /// Returns the current scene.
    /// </summary>
    /// <returns>The current scene.</returns>
    public int GetCurrentScene() {
        return i_CurrentScene;
    }

    /// <summary>
    /// Returns a reference to the <c>home_PLY</c> button.
    /// </summary>
    /// <returns>The home button</returns>
    public GameObject GetHomeButton() {
        return m_HomeButton;
    }

    /// <summary>
    /// Returns a reference to the <c>trigger_PLY</c> button.
    /// </summary>
    /// <returns>The trigger button</returns>
    public GameObject GetTriggerButton() {
        return m_TriggerButton;
    }

    /// <summary>
    /// Gets a reference to the reticle
    /// </summary>
    /// <returns>The Reticle</returns>
    public GameObject GetReticle() {
        return m_Reticle;
    }


    /// <summary>
    /// Returns a reference to the CenterEyeAnchor (Camera the player is looking through)
    /// </summary>
    /// <returns>A reference to the CenterEyeAnchor</returns>
    public GameObject GetCenterEyeAnchor() {
        return m_CenterEyeAnchor;
    }

    /// <summary>
    /// Returns a reference to the controller model
    /// </summary>
    /// <returns>A reference to the controller model</returns>
    public GameObject GetControllerModel() {
        return m_ControllerModel;
    }

    /// <summary>
    /// Returns a reference to the default position of the robot in the scenes
    /// </summary>
    /// <returns>A reference to the default position of the robot in the scenes</returns>
    public Vector3 GetRobotDefaultPosition() {
        return m_RobotDefaultPosition.transform.position;
    }

    /// <summary>
    /// Returns a reference to the loading bar image
    /// </summary>
    /// <returns>A reference to the loading bar image</returns>
    public Image GetLoadingBar() {
        return this.m_LoadingBar;
    }

    /*
     * Reticle Controls 
     */

    /// <summary>
    /// Function resets the state of the reticle to its default state. Pass in true to add a small delay before reverting to default.
    /// This could be used for when the user just clicks on an item instead of pressing and holding, so they have time to see the click
    /// was registered.
    /// </summary>
    public void ReticleSetDefaultState() {
        m_ReticleOuterCircle.enabled = false; // hide the outer circle
        m_ReticleInnerCircle.material.SetColor("_Color", c_COLOR_LIGHTGRAY);
    }

    /// <summary>
    /// Sets the reticle to its Hover state. Should be used when the user is hovering over an interactable item
    /// </summary>
    public void ReticleSetHoverState() {
        m_ReticleOuterCircle.enabled = true;    // show the outer circle, set both to white
        m_ReticleOuterCircle.material.SetColor("_Color", Color.white);
        m_ReticleInnerCircle.material.SetColor("_Color", Color.white);
    }

    public void ReticleSetInteracting() {
        m_ReticleOuterCircle.enabled = true;    // show the outer circle, set both to white
        m_ReticleOuterCircle.material.SetColor("_Color", Color.green);
    }

    /// <summary>
    /// Driver method to start a coroutine that shows an invalid operation on the reticle (circle with line through it)
    /// </summary>
    public void ReticleInvalidOperation() {

        if(m_InvalidOperationCoroutine != null) {
            StopCoroutine(m_InvalidOperationCoroutine);
        }
        m_InvalidOperationCoroutine = DoInvalidOperation();
        StartCoroutine(m_InvalidOperationCoroutine);
    }

    /// <summary>
    /// Displays the invalid operation image on the reticle for 1 second
    /// </summary>
    /// <returns>A reference to the coroutine</returns>
    private IEnumerator DoInvalidOperation() {
        m_InvalidOperationImage.gameObject.SetActive(true); // show the invalid op image
        yield return new WaitForSeconds(1);
        m_InvalidOperationImage.gameObject.SetActive(false); // hide the invalid op image

    }
}