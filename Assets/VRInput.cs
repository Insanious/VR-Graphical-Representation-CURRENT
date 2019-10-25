
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class VRInput : MonoBehaviour
{
	public GameObject selectedObj;
	public Text output;

    public SteamVR_Input_Sources handType;
    public GameObject otherHand;

    private LineRenderer laser;
    private float targetLength;
    private float movementSpeed;
    public SteamVR_Behaviour_Pose controllerPose;

    public SteamVR_Action_Boolean toggleSubtreeLines;
	public SteamVR_Action_Boolean selectSubtree;
	public SteamVR_Action_Boolean copySubtree;
    public SteamVR_Action_Vector2 move;
    public SteamVR_Action_Vector2 changeSubtreeHeight;

    public float threshold;
    public float current;
    

    void Start()
	{
        output.text = string.Empty;

        laser = GetComponent<LineRenderer>();

        targetLength = 100f;
        threshold = 0.2f;
        current = 0;
        movementSpeed = 1000f;

        selectSubtree.AddOnStateDownListener(SelectSubtree, handType);
		copySubtree.AddOnStateDownListener(CopySubtree, handType);
		toggleSubtreeLines.AddOnStateDownListener(ToggleSubtreeLines, handType);
	}

    private void UpdateLine()
    {
        Vector3 endPosition = controllerPose.transform.position + (controllerPose.transform.forward * targetLength);

        laser.SetPosition(0, controllerPose.transform.position);
        laser.SetPosition(1, endPosition);
    }

    public void Move(SteamVR_Action_Vector2 action)
	{
		if (selectedObj == null)
		{
            //Debug.Log("Move");
            //Debug.Log("Before: " + transform.position.x);
            //Camera.main.transform.position += Camera.main.transform.forward * Time.deltaTime * movementSpeed;
            //Debug.Log("res: " + Camera.main.transform.forward * Time.deltaTime * movementSpeed);
           
            //transform.position += Camera.main.transform.forward * Time.deltaTime * movementSpeed;
            //Debug.Log("after: " + transform.position.x);
            //otherHand.transform.position += Camera.main.transform.forward * Time.deltaTime * movementSpeed;
        }
	}

	public void SelectSubtree(SteamVR_Action_Boolean action, SteamVR_Input_Sources handType)
	{
        GameObject obj = RayCastToGameObject(controllerPose.transform.position, controllerPose.transform.forward);

        if (obj != null && (obj.name == "DataBall(Clone)" || obj.name == "Cube(Clone)"))
        {
            selectedObj = obj;
            otherHand.GetComponent<VRInput>().selectedObj = obj;
        }
        else
        {
            selectedObj = null;
            otherHand.GetComponent<VRInput>().selectedObj = null;
        }
	}

	public void ChangeSubtreeHeight(SteamVR_Action_Vector2 action)
    {
        if (selectedObj != null)
		{
            float yAxis = action.GetAxis(handType).y;
            if (yAxis > 0)
                current += action.GetAxis(handType).y * Time.deltaTime;
            else
                current += -action.GetAxis(handType).y * Time.deltaTime;

            if (current > threshold)
            {
                current = 0;

                if (yAxis > 0)
                    selectedObj.GetComponent<Linker>().container.IncrementSubtree(Linker.RenderMode.LEVELS);
                else if (yAxis < 0)
                    selectedObj.GetComponent<Linker>().container.DecrementSubtree(Linker.RenderMode.LEVELS);
            }
		}
	}

	public void CopySubtree(SteamVR_Action_Boolean action, SteamVR_Input_Sources handType)
	{
        if (selectedObj != null)
			selectedObj.GetComponent<Linker>().container.CopySubtree(new Vector3(10f, 0f, 10f));
	}

	public void ToggleSubtreeLines(SteamVR_Action_Boolean action, SteamVR_Input_Sources handType)
	{
        if (selectedObj != null)
            selectedObj.GetComponent<Linker>().container.ToggleSubtreeLines();
	}

	private GameObject RayCastToGameObject(Vector3 origin, Vector3 direction)
	{
		RaycastHit hit;

		if (Physics.Raycast(origin, direction, out hit, Mathf.Infinity))
			return hit.transform.gameObject;

		return null;
	}

	void Update()
	{
        UpdateLine();

        if (selectedObj != null)
            output.text = selectedObj.GetComponent<Linker>().container.ToString();
        else
            output.text = string.Empty;
        
        if (move.GetAxis(handType).x != 0f || move.GetAxis(handType).y != 0f)
            Move(move);
        
        if (changeSubtreeHeight.GetAxis(handType).y != 0f)
            ChangeSubtreeHeight(changeSubtreeHeight);
    }
}
