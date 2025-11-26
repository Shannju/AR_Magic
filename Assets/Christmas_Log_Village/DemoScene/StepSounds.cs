using UnityEngine;
using System.Collections;

public class StepSounds : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if(Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") !=0){
			
			if(!this.GetComponent<AudioSource>().isPlaying){
				this.GetComponent<AudioSource>().Play();
			}
		}
		else{
			this.GetComponent<AudioSource>().Stop();
		}
	
	}
}
