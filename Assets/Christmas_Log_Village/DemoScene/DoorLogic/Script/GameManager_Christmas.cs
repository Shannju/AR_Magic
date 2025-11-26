using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace ChristmasLogvillage
{


public enum ChristmasHouseTypesEnum
{
	HouseA,HouseB,HouseC,HouseD,HouseE,HouseF
}

public class GameManager_Christmas : MonoBehaviour {
	

	public Vector3 LastDoor;

	public Quaternion PlayerRotation;
	
	//houses
	public Transform HouseASpawn;
	public Transform HouseBSpawn;
	public Transform HouseCSpawn;
	public Transform HouseDSpawn;
	public Transform HouseESpawn;
	public Transform HouseFSpawn;

	public ChristmasHouseTypesEnum HouseTypeTarget;

	[HideInInspector]
	public int lastLevel = 0;

	public AudioClip MainMusic;



	// Use this for initialization
	void Start () {

		DontDestroyOnLoad(transform.gameObject);

		Application.LoadLevel(1);
		SceneManager.sceneLoaded += OnSceneWasLoaded;
	
	}

	public void OnSceneWasLoaded(Scene scene, LoadSceneMode mode){

        GameObject player = FindPlayer();

        if (scene.buildIndex == 1){
			
			if(lastLevel ==0){
				PlayerRotation = Quaternion.Euler(new Vector3(0,PlayerRotation.y,0));
			}

			player.transform.rotation = PlayerRotation;
			player.transform.position = LastDoor;

			lastLevel = 1;

			this.GetComponent<AudioSource>().volume = 0.5f;

			//this.audio.clip = MainMusic;
				//this.audio.Play ();



		}

		if(scene.buildIndex == 2){
			lastLevel = 2;

			//this.audio.clip = InteriorMusic;
			//this.audio.Play ();

			this.GetComponent<AudioSource>().volume = 0.3f;

			if(this.HouseTypeTarget == ChristmasHouseTypesEnum.HouseA)
				player.transform.position = HouseASpawn.position;
			else if(this.HouseTypeTarget == ChristmasHouseTypesEnum.HouseB)
				player.transform.position = HouseBSpawn.position;
			else if(this.HouseTypeTarget == ChristmasHouseTypesEnum.HouseC)
				player.transform.position = HouseCSpawn.position;
			else if(this.HouseTypeTarget == ChristmasHouseTypesEnum.HouseD)
				player.transform.position = HouseDSpawn.position;
			else if(this.HouseTypeTarget == ChristmasHouseTypesEnum.HouseE)
				player.transform.position = HouseESpawn.position;
			else if(this.HouseTypeTarget == ChristmasHouseTypesEnum.HouseF)
				player.transform.position = HouseFSpawn.position;


		}


	}

    public GameObject FindPlayer()
    {
        GameObject o;

        //THIS NEED TO BE YOUR PLAYER
        o = GameObject.FindGameObjectWithTag("Player");

        return o;
    }

}

}
