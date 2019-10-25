using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
	public float speed = 1.0f;

    // Start is called before the first frame update
	void Start() {}

	// Update is called once per frame

	void Update()
	{
		if(Input.GetKey("d"))
		{
			transform.Translate(new Vector3(speed * Time.deltaTime,0,0));
		}
		if(Input.GetKey("a"))
		{
			transform.Translate(new Vector3(-speed * Time.deltaTime,0,0));
		}
		if(Input.GetKey("s"))
		{
			transform.Translate(new Vector3(0,0,-speed * Time.deltaTime));
		}
		if(Input.GetKey("w"))
		{
			transform.Translate(new Vector3(0,0,speed * Time.deltaTime));
		}
	}
}
