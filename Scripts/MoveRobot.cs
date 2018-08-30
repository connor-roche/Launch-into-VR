using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRobot : MonoBehaviour {

    [SerializeField] private bool m_IsMoving;                   // is the robot moving
    [SerializeField] private bool m_FirstCall;                  // is this the first call to move
    [SerializeField] private bool m_IsStopping;                 // is the robot stopping
    [SerializeField] private float m_Speed = 5f;                // speed at which to mvoe the robot
    [SerializeField] private Vector3 m_Dest;                    // destination to move to
    [SerializeField] private GameObject m_TargetLookAt;         // target to look at 
    [SerializeField] private float m_StoppingDistance;          // the distance in which to begin the stopping animation

    private Animator m_RobotAnimator;                           // The animator controller for the robots animations


	// Use this for initialization
	void Start () {
        m_RobotAnimator = GetComponent<Animator>();
        m_FirstCall = true;
	}
	
	// Update is called once per frame
	void Update () {

        // if the robot is moving
        if (m_IsMoving) {

            // if this is the first call to move
            if (m_FirstCall) {

                // Start the moving animation
                m_RobotAnimator.SetBool("WillMove", false);
                m_FirstCall = false;
            }

            // Move the robot 1 step
            float step = m_Speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, m_Dest, step);
            transform.LookAt(m_Dest);
            
            // If the robot is approaching its destination begin the stopping animation
            if (Mathf.Abs((transform.position - m_Dest).magnitude) <= m_StoppingDistance && !m_IsStopping) {
                m_RobotAnimator.SetBool("WillStop", true);
                m_IsStopping = true;
            }else if(transform.position == m_Dest) {
                // robot is stopped, look at the user.
                transform.LookAt(m_TargetLookAt.transform);
                m_IsMoving = false;
                m_FirstCall = true;
                m_IsStopping = false;
            }
        }
	}

    /// <summary>
    /// Moves the robot to a destination specified in the param at a speed specified in the param (default 5f)
    /// </summary>
    /// <param name="destination">destination to mvoe the robot to</param>
    /// <param name="speed">speed at which to move the robot</param>
    public void StartMoving(Vector3 destination, float speed = 5f) {
        m_RobotAnimator.SetBool("WillMove", true);
        m_Dest = destination;
        m_IsMoving = true;
        m_Speed = speed;
    }
}
