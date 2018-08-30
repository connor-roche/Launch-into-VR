using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndScene : MonoBehaviour {

    private IntroSessionManager m_Manager;                          // A reference to the manager script which acts as a library for the tutorial

    public static EndScene s_Instance = null;                       // Reference to this singleton class

    [SerializeField] private Dialogue m_DialogueInstructions;       // Dialogue object holding the audio and text instructions for the scene
    [SerializeField] private GameObject m_RobotEndPosition;         // Position for the robot to mvoe to 
    [SerializeField] private Image m_Black;                         // Black image to fade out with
    [SerializeField] private Animator m_Anim;                       // Animator to fade to black


	// Use this for initialization
	void Start () {
		if (s_Instance == null) {
			s_Instance = this;
            m_Manager = IntroSessionManager.s_Instance;
			StartCoroutine(Run());
		} else {
			Destroy(gameObject);
		}
	}
	

    /// <summary>
    /// Controls the flow of the scene.
    /// </summary>
    /// <returns>A reference to the coroutine</returns>
    private IEnumerator Run() {

        m_Manager.MoveRobotToPosition(m_RobotEndPosition.transform.position);

        m_Manager.GlobalMessage(m_DialogueInstructions.DialogueElements[0]);

		yield return new WaitForSeconds(m_DialogueInstructions.DialogueElements[0].PlayBackSoundFile.length);

		StartCoroutine(FadingOut());
	}

    /// <summary>
    /// Two animations: but FadingIn is a transparent image
    /// </summary>
    /// <returns>A reference to the coroutine</returns>
    private IEnumerator FadingOut() {
		m_Anim.SetBool("Fade", true);
		yield return new WaitUntil(() => m_Black.color.a == 1);	
	}
}
