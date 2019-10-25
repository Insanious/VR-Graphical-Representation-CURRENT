using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectClicker : MonoBehaviour
{
    public GameObject obj = null;
    public GameObject selectedObj = null;
    public bool hasSelected = false;
    public Vector3 oldVec;
    public float rotationSpeed = 2f;
    public float radius = 0f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            obj = RayCastToGameObject();
            if (obj != null && (obj.name == "DataBall(Clone)" || obj.name == "Cube(Clone)"))
            {
                obj.GetComponent<Linker>().container.DecrementSubtree(Linker.RenderMode.LEVELS);
                //obj.GetComponent<Linker>().container.Print();
                //obj.GetComponent<Linker>().container.ToggleSubtreeLines();
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (selectedObj == null)
            {
                obj = RayCastToGameObject();
                if (obj != null && (obj.name == "DataBall(Clone)" || obj.name == "Cube(Clone)"))
                {
                    selectedObj = obj;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Vector3 origin = new Vector3();
                    origin = ray.origin;
                    /*if (origin.x < 0)
						origin.x *= -1;
					if (origin.z < 0)
						origin.z *= -1;*/

                    Vector3 start = new Vector3();
                    start = ray.origin - selectedObj.transform.position;
                    radius = start.magnitude;
                }
            }
            else
            {
                /*
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				Debug.DrawRay(ray.origin, ray.direction * 10, Color.red, 1);
				Vector3 origin = new Vector3();
				origin = ray.origin;
				/*if (origin.x < 0)
					origin.x *= -1;
				if (origin.z < 0)
					origin.z *= -1;

				Vector3 objPos = selectedObj.transform.position;
				Vector3	start = new Vector3();
				Vector3 end = new Vector3();
				Vector3 final = new Vector3();
				Vector3 offset = new Vector3();

				float deltaY = Input.GetAxis("Mouse Y");

				start = new Vector3(ray.origin.x - objPos.x, 0, ray.origin.z - objPos.z);
				end = start;
				end.x += deltaX;

				offset = Vector3.ClampMagnitude(end, radius);
				final = start - offset;

				//Debug.Log("deltaX=" + deltaX + "final.x=" + final.x);
				//Debug.Log(ray.direction.ToString());
				if (deltaX < 0 && final.x > 0)
					final.x *= -1;
				if (deltaX > 0 && final.x < 0)
					final.x *= -1;

				//Debug.Log(start.ToString() + ", " + end.ToString() + ", " + offset.ToString() + ", " + final.ToString());



				Vector3 camPos = Camera.main.transform.position;

				oldVec = new Vector3(camPos.x - objPos.x, 0, camPos.z - objPos.z);



				Vector3 newVec = new Vector3(oldVec.x + deltaX, oldVec.y, oldVec.z);


				newVec *= oldVec.magnitude / newVec.magnitude;
				Debug.Log("old.m=" + oldVec.magnitude + ", new.m=" + newVec.magnitude);
				Vector3	offset = new Vector3(newVec.x - oldVec.x, 0, newVec.z - oldVec.z);
				*/

                float deltaX = Input.GetAxis("Mouse X");
                //if (deltaX != 0)
                //selectedObj.GetComponent<Linker>().container.MoveSubtree(new Vector3(deltaX, 0, 0));
            }
        }
        else
            selectedObj = null;





        if (Input.GetMouseButtonDown(1))
        {
            obj = RayCastToGameObject();
            if (obj != null && (obj.name == "DataBall(Clone)" || obj.name == "Cube(Clone)"))
            {
                if (obj.GetComponent<Linker>().container.children.Count != 0)
                {
                    //int currentDepth = obj.GetComponent<Linker>().container.depth;
                    obj.GetComponent<Linker>().container.IncrementSubtree(Linker.RenderMode.LEVELS);
                    //obj.GetComponent<Linker>().container.InstantiateSubtree(Linker.RenderMode.LEVELS, 1);
                }
            }
        }

        if (Input.GetMouseButtonDown(2))
        {
            obj = RayCastToGameObject();
            if (obj != null && (obj.name == "DataBall(Clone)" || obj.name == "Cube(Clone)"))
                //obj.GetComponent<Linker>().container.ToggleSubtreeLines();
                //obj.GetComponent<Linker>().container.Print();
                obj.GetComponent<Linker>().container.CopySubtree(new Vector3(10f, 0f, 10f));
        }
    }

    GameObject RayCastToGameObject()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 100.0f))
            return hit.transform.gameObject;

        return null;
    }
}