using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class is designed for Scene4 to be listening for when an object is dropped through the doors
/// </summary>
public class TriggerStage4 : MonoBehaviour {

    public delegate void Stage4Trigger();
    public static event Stage4Trigger e_Stage4Triggered;        // Event for Stage4 when the threshold recognizes an object goes through

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag.Contains("_PickUp_")) {
            
            // Notify subsribers that an object that can be picked up went through the threshold
            if(e_Stage4Triggered != null) {
                e_Stage4Triggered();
            }
        }
    }
}
