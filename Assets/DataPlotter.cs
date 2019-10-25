using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Valve.VR;
using SFB;

using System;

public class DataPlotter : MonoBehaviour
{

    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean loadFile;

    public enum RenderType { PLANAR, CONE };
    public enum RenderMode { SIBLINGS, LEVELS };

    public GameObject folderPrefab;
    public GameObject filePrefab;
    public GameObject linePrefab;
    public int depth;
    public string file;

    private List<List<Linker.Container>> nodes;
    private Queue<Linker.Container> childrenQueue;
    private Linker.Container root;

    private void Start()
    {
        loadFile.AddOnStateDownListener(Init, handType);
    }

    public void Init(SteamVR_Action_Boolean action, SteamVR_Input_Sources handType)
    {
        Debug.Log("load file");
        var path = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);
        root = JSONParser.Read(path[0]);
        nodes = new List<List<Linker.Container>>();

       //root = JSONParser.Read(file);

        InitializeNodes();
        InstantiateRoot(root, new Vector3(0, 0, 0), new Vector3(0.25f, 0.25f, 0.25f));
    }

    public void InstantiateRoot(Linker.Container root, Vector3 startPosition, Vector3 size)
    {
        GameObject prefab;
        if (root.size == 0)
            prefab = folderPrefab;
        else
            prefab = filePrefab;

        root.self = Instantiate(prefab, new Vector3(startPosition.x, startPosition.y, startPosition.z), Quaternion.identity);
        root.line = Instantiate(linePrefab);
        root.isInstantiated = true;
        root.rootPosition = new Vector3(0, 0, 0);
        root.self.transform.localScale = size;
        root.self.GetComponent<Renderer>().material.color = root.color;

        root.folderPrefab = folderPrefab;
        root.filePrefab = filePrefab;
        root.linePrefab = linePrefab;

        root.self.GetComponent<Linker>().container = root;
    }

    private int GetMaxDepth(Linker.Container root)
    {
        Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
        Linker.Container parent;
        int maxDepth = 0;

        childrenQueue.Enqueue(root);
        while (childrenQueue.Count != 0) // Get max depth
        {
            parent = childrenQueue.Dequeue();
            if (parent.depth > maxDepth)
                maxDepth = parent.depth;

            foreach (Linker.Container child in parent.children)
                childrenQueue.Enqueue(child);
        }
        return maxDepth;
    }

    private void SetMaxDepth(Linker.Container root, int maxDepth)
    {
        Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
        Linker.Container parent;

        childrenQueue.Enqueue(root);
        while (childrenQueue.Count != 0) // set max depth
        {
            parent = childrenQueue.Dequeue();
            parent.maxDepth = maxDepth;

            foreach (Linker.Container child in parent.children)
                childrenQueue.Enqueue(child);
        }
    }

    public void InitializeNodes()
    {
        Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
        Linker.Container parent;
        int id = 0;
        float colorTint = 0.2f;
        int maxFileSize = (int)Mathf.Log(root.GetMaxFileSize());
        int maxFolderSize = (int)Mathf.Log(root.GetSize());

        Debug.Log(maxFileSize + ", " + maxFolderSize);

        Color fileColor = new Color();
        fileColor = Color.red;

        root.color = new Color();
        root.color = Color.white;
        root.parent = null;
        root.root = root;
        root.siblings = new List<Linker.Container>();
        root.depth = root.GetDepth();
        root.id = id++;
        root.isInstantiated = false;
        root.isDrawingLine = false;
        root.subtreeDepth = 0;

        childrenQueue.Enqueue(root);
        while (childrenQueue.Count != 0)
        {
            parent = childrenQueue.Dequeue();
            foreach (Linker.Container child in parent.children)
            {
                child.siblings = new List<Linker.Container>();
                foreach (var sibling in parent.children) // Add all siblings to all children
                    if (child.id != sibling.id)
                        child.siblings.Add(sibling);

                child.parent = parent;
                child.depth = child.GetDepth();
                child.id = id++;
                child.root = root;
                child.isInstantiated = false;
                child.isDrawingLine = false;
                child.subtreeDepth = -1;
                child.rootPosition = root.rootPosition;

                if (child.size == 0) // folder
                    child.color = child.CalculateColor(root.color, maxFolderSize);
                else
                    child.color = child.CalculateColor(fileColor, maxFileSize);

                /*
				if (child.id > 0 && child.id < 4)
				{
					child.color = new Color();
					switch (child.id)
					{
						case 1:
							child.color = Color.red;
							break;
						case 2:
							child.color = Color.green;
							break;
						case 3:
							child.color = Color.blue;
							break;
					}
				}

				else
					child.color = new Color(parent.color.r + colorTint, parent.color.g + colorTint, parent.color.b + colorTint);
				*/

                childrenQueue.Enqueue(child);
            }
        }

        SetMaxDepth(root, GetMaxDepth(root));
    }
}