using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Class is designed to be used on the targets in Scene6. They have a voxel effect when they are hit by the ball.
/// </summary>
public class Target : MonoBehaviour {

    public delegate void BallHitTarget();
    public static event BallHitTarget e_BallHitTheTarget;           // Event to notify subscribers when the ball hits this target.

    private Rigidbody[] m_childrenRigidBodies;                      // The parts that build up the target
    private bool m_TargetHit;                                       // Whether or not this target was already hit

    private void Start() {
        m_childrenRigidBodies = GetComponentsInChildren<Rigidbody>();                               // Get the parts that make up the target
        transform.LookAt(IntroSessionManager.s_Instance.GetCenterEyeAnchor().transform);            // Make the target face the user
    }


    private void OnTriggerEnter(Collider collider) {
        if (!m_TargetHit) {
            // If the ball hits the target
            if (collider.tag.Contains("_Ball_")) {
                m_TargetHit = true;
                // Notify subscribers
                if (e_BallHitTheTarget != null) {
                    e_BallHitTheTarget();
                }

                // Destroy the ball
                Destroy(collider.gameObject, 1f);
                StartCoroutine(TargetHit());
            }
        }
    }

    /// <summary>
    /// Once the target gets hit, wait 2.5 seconds to allow the voxels to spread out, then turn gravity on for each one and let them fall.
    /// </summary>
    /// <returns>A reference to the coroutine</returns>
    private IEnumerator TargetHit() {
        yield return new WaitForSeconds(2.5f);
        foreach(Rigidbody rb in m_childrenRigidBodies) {
            rb.useGravity = true;
        }
    }




}
