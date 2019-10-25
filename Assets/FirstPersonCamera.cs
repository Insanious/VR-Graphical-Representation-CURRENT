using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
	public float speedH = 10.0f;
	public float speedV = 10.0f;

	private float yaw = 0.0f;
	private float pitch = 0.0f;


	void Start()
	{
		Cursor.visible = true;
	}

	void Update () {
		yaw += speedH * Input.GetAxis("Mouse X");
		pitch -= speedV * Input.GetAxis("Mouse Y");

		if (Input.GetKey(KeyCode.Escape))
			Cursor.lockState = CursorLockMode.None;
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
		}
		Cursor.visible = true;
	}
}
