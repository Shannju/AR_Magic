using UnityEngine;
using System.Collections;

public class UvScrollingAnimation : MonoBehaviour {


	public Vector2 UvSpeed;

	void Start () {
	
	}
	

	void Update () {

		this.GetComponent<Renderer>().material.mainTextureOffset += UvSpeed;
	
	}
}
