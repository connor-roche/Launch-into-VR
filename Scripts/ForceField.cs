using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class is used to destroy objects when they are thrown too far
/// This is done by determining when the object exits the forcefield (OnTriggerExit) and when that happens it will be destroyed.
/// </summary>
public class ForceField : MonoBehaviour {

    [SerializeField] private Animator m_Animator;        // The animator that controls the animation on the force field

    private bool m_InTrigger;                             // if the ball is inside the force field
    private GameObject m_Ball;                            // the ball



    // Update is called once per frame
    void Update() {

        if (!m_InTrigger) {
            if (m_Ball != null) {
                // this means the ball exists and it outside of the forcefield
                m_Animator.SetTrigger("BallCollision");
                Destroy(m_Ball);
            }
        }

    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag.Contains("_PickUp_")) {
            // the ball is inside the force field
            m_InTrigger = true;
            if(!m_Ball)
                m_Ball = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.tag.Contains("_PickUp_")) {
            m_InTrigger = false;
            if (!m_Ball)
                m_Ball = other.gameObject;
        }
    }


}
