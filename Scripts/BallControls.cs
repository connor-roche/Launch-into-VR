using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRStandardAssets.Utils;

public class BallControls : MonoBehaviour {
    
    [SerializeField] private AudioSource BallSoundBig;              // Sound effect to play when ball hits other objects
    [SerializeField] private AudioSource BallSoundSmall;            // Sound effect to play when ball hits other objects

    void OnCollisionEnter(Collision col){
        // if the ball hits the wall or floor play the sound effects depending on the velocity of the ball
        if((col.gameObject.tag == "_Floor_" || col.gameObject.tag == "_Wall_")  && col.relativeVelocity.magnitude > 10f){
            BallSoundBig.Play();
        }
        else if((col.gameObject.tag  == "_Floor_" || col.gameObject.tag == "_Wall_") && col.relativeVelocity.magnitude <= 10f){
            BallSoundSmall.Play();
            BallSoundSmall.volume *=  0.5f;
        }
    }

    private void OnDestroy() {
        // Create the effect when the ball destroys, but destroy that effect after 2 seconds to clean up the resources.
        Destroy(Instantiate(Resources.Load("DestroyBallEffect"), transform.position, transform.rotation), 2f);
    }

}