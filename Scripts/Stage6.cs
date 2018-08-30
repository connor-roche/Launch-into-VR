using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRStandardAssets.Utils;

public class Stage6 : MonoBehaviour {

    private IntroSessionManager m_Manager;                              // A reference to the manager script which acts as a library for the tutorial

    public static Stage6 s_Instance = null;                             // Instance for all of Stage6

    [SerializeField] private GameObject m_Target1;                      // Target 1
    [SerializeField] private GameObject m_Target2;                      // Target 2
    [SerializeField] private GameObject m_Target3;                      // Target 3

    [SerializeField] private Dialogue m_DialogueInstructions;           // Dialogue object holding the audio and text instructions for the scene

    private int m_TargetsAlive = 3;                                     // Keep count of targets that have not been destroyed

    // Use this for initialization
    void Start () {
        if (s_Instance == null) {
            s_Instance = this;
            m_Manager = IntroSessionManager.s_Instance;
            m_Manager.ChangeMusic(1);

            Run();
        }
        else {
            Destroy(gameObject);
        }

	}

    private void OnEnable() {
        Target.e_BallHitTheTarget += NotifyTargetHit;     // subsribe to events when the ball htis the targets
    }

    private void OnDisable() {
        Target.e_BallHitTheTarget -= NotifyTargetHit;     // unsubscribe to events
    }

    /// <summary>
    /// Controls the flow of the scene.
    /// </summary>
    /// <returns>A reference to the coroutine</returns>
    private void Run() {

        StartCoroutine(ShowTargets());
        m_Manager.GlobalMessage(m_DialogueInstructions.DialogueElements[0]);

    }

    /// <summary>
    /// Generate all three targets. 
    /// <para>Returns IEnumerator Coroutine</para>
    /// </summary>
    /// <returns>IEnumerator Coroutine</returns>
    private IEnumerator ShowTargets() {
        m_Target1.SetActive(true);
        yield return new WaitForSeconds(1f);
        m_Target2.SetActive(true);
        yield return new WaitForSeconds(1f);
        m_Target3.SetActive(true);
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// Is an event that keeps track of targets that are hit.
    /// If all targets are hit, calls end stage.
    /// </summary>
    public void NotifyTargetHit() {
        m_TargetsAlive--;
        if(m_TargetsAlive <= 0) {
            StartCoroutine(EndStage());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerator EndStage() {
        yield return new WaitForSeconds(4f); // Allow animation to play out for last target before moving on
        IntroSessionManager.s_Instance.MoveToNextStage(0);
    }

}
