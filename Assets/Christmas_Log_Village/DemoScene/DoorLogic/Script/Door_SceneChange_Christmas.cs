using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace ChristmasLogvillage
{


    public class Door_SceneChange_Christmas : MonoBehaviour {

	//player will look at the Z axis of this trigger when going back to the mainScene

	public Animator _animator;	

	public int level_id;
	private bool canFade = true;
	private GameManager_Christmas gameManager;

	public ChristmasHouseTypesEnum HouseType;

	

	public void Start(){
		gameManager = GameObject.Find("__GameManager").GetComponent("GameManager_Christmas") as GameManager_Christmas;
	}
	

	public void OnTriggerEnter(Collider Col){

		if(Col.gameObject.tag == "Player" && canFade){
			canFade = false;
			_animator.SetTrigger("Fade");
			if(gameManager.lastLevel == 1){

				Vector3 Pos = this.transform.position + this.transform.forward*2;
				Pos.y = Col.transform.position.y;
				gameManager.LastDoor= Pos;
				gameManager.PlayerRotation = this.transform.rotation;

				if(this.gameObject.GetComponent<Door_SceneChange_Christmas>()){
					gameManager.HouseTypeTarget =this.gameObject.GetComponent<Door_SceneChange_Christmas>().HouseType;
				}
			}
			StartCoroutine(PlayOneShot());
		}
	}


	
	public IEnumerator PlayOneShot ()
	{
		yield return new WaitForSeconds(1);
		Application.LoadLevel(level_id);
	}


}
}
