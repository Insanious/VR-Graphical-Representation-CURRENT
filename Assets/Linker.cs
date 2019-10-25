using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Linker : MonoBehaviour
{
    public enum RenderMode { SIBLINGS, LEVELS };

    public Container container;

    public class Container
    {
        public GameObject self;
        public GameObject line;

        public GameObject folderPrefab;
        public GameObject filePrefab;
        public GameObject linePrefab;

        public List<Container> children { get; set; }
        public List<Container> siblings { get; set; }
        public Container parent { get; set; }
        public Container root { get; set; }
        public bool isInstantiated { get; set; }
        public bool isDrawingLine { get; set; }
        public int id { get; set; }
        public int depth { get; set; }
        public int subtreeDepth { get; set; }
        public int maxDepth { get; set; }
        public Vector3 rootPosition { get; set; }
        public string name { get; set; }
        public int size { get; set; }
        public float weight { get; set; }
        public Color color { get; set; }

        public Color CalculateColor(Color baseColor, int maxSize)
        {
            Color newColor = new Color();
            float tint = 0;
            float nodeSize = 0;

            if (this.size == 0) // folder
                nodeSize = this.GetSize();
            //tint = (float)this.GetSize() / (float)maxSize;
            else
                nodeSize = this.size;
            //tint = (float)this.size / (float)maxSize;

            nodeSize = Mathf.Log(nodeSize);
            tint = nodeSize / (float)maxSize;
            

            newColor = baseColor * tint;
            return newColor;
        }

        private Linker.Container CopyNode()
        {
            Linker.Container copy = new Linker.Container();
            copy.color = new Color();
            copy.children = new List<Container>();
            copy.siblings = new List<Container>();
            copy.parent = null;
            copy.self = null;

            copy.folderPrefab = this.folderPrefab;
            copy.filePrefab = this.filePrefab;
            copy.linePrefab = this.linePrefab;
            copy.isInstantiated = false;
            copy.isDrawingLine = false;
            copy.id = this.id;
            copy.subtreeDepth = -1;
            copy.name = this.name;
            copy.size = this.size;
            copy.weight = this.weight;
            copy.color = this.color;

            return copy;
        }

        public void CopySubtree(Vector3 offset)
        {
            Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
            Queue<Linker.Container> newChildrenQueue = new Queue<Linker.Container>();
            Linker.Container parent;
            Linker.Container newParent;
            Linker.Container newChild;

            Vector3 position = self.transform.position;
            Vector3 size = self.transform.localScale;

            Vector3 newPosition = new Vector3(position.x + offset.x, position.y + offset.y, position.z + offset.z);
            Vector3 newSize = new Vector3(size.x, size.y, size.z);

            Linker.Container newRoot = this.CopyNode();

            newRoot.subtreeDepth = 0;
            newRoot.depth = newRoot.GetDepth();
            newRoot.rootPosition = newPosition;
            newRoot.root = newRoot;

            childrenQueue.Enqueue(this);
            newChildrenQueue.Enqueue(newRoot);
            while (childrenQueue.Count != 0)
            {
                parent = childrenQueue.Dequeue();
                newParent = newChildrenQueue.Dequeue();
                foreach (Linker.Container child in parent.children)
                {
                    newChild = child.CopyNode();
                    newChild.parent = newParent;
                    newChild.depth = newChild.GetDepth();
                    newChild.rootPosition = newRoot.rootPosition;
                    newChild.root = newRoot;
                    newChildrenQueue.Enqueue(newChild);

                    newParent.children.Add(newChild);
                    childrenQueue.Enqueue(child);
                }

                foreach (Linker.Container child in newParent.children) // Add siblings
                    foreach (Linker.Container sibling in newParent.children)
                        if (child.id != sibling.id)
                            child.siblings.Add(sibling);
            }
            SetMaxDepth(newRoot, GetMaxDepth(newRoot)); // Calculate max depth and set all nodes max depth to it in the new tree starting from the new root

            InstantiateNode(newRoot, newPosition, newSize);
        }

        private List<Linker.Container> GetInstantiatedNodes(int depth)
        {
            Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
            List<Linker.Container> nodes = new List<Linker.Container>();
            Linker.Container parent;

            childrenQueue.Enqueue(this);
            while (childrenQueue.Count != 0)
            {
                parent = childrenQueue.Dequeue();
                if (parent.depth == depth)
                {
                    if (parent.isInstantiated && parent.self.activeSelf)
                        nodes.Add(parent);
                }

                else if (parent.depth < depth)
                    foreach (Linker.Container child in parent.children)
                        childrenQueue.Enqueue(child);
            }

            return nodes;
        }

        private List<List<Linker.Container>> GetNodes(RenderMode mode, int depth)
        {
            switch (mode)
            {
                case RenderMode.SIBLINGS:
                    return GetAllSiblings(depth);
                case RenderMode.LEVELS:
                    if (depth == 1)
                    {
                        List<List<Linker.Container>> list = new List<List<Linker.Container>>();
                        list.Add(GetNextLevel());
                        return list;
                    }
                    return GetAllLevels(depth);
                default:
                    Debug.Log("Wrong mode.");
                    break;
            }
            return null;
        }

        private List<List<Linker.Container>> GetAllSiblings(int depth)
        {
            List<List<Linker.Container>> siblings = new List<List<Linker.Container>>();
            Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
            Linker.Container parent = this; // 'this' is the root
            Linker.Container child;
            int level = 0;
            bool depthReached = false;

            //siblings.Add(new List<Linker.Container>());
            //siblings[level++].Add(parent); // Add node as the single object at the first level
            if (depth == 0)
                return siblings;

            siblings.Add(new List<Linker.Container>());

            // Cover edge case so we don't need to check against root's parent, since it's parent is null
            foreach (Linker.Container grandchild in parent.children) // Add root's children to the queue
                childrenQueue.Enqueue(grandchild);

            while (childrenQueue.Count != 0)
            {
                child = childrenQueue.Dequeue();
                foreach (Linker.Container grandchild in child.children) // Add the parent's children to the queue
                    childrenQueue.Enqueue(grandchild);

                if (child.parent.id != parent.id) // If there is a new grandparent, increment level
                {
                    if (level == depth)
                        depthReached = true;
                    else
                    {
                        level++;
                        parent = child.parent;
                        siblings.Add(new List<Linker.Container>());
                    }
                }
                siblings[level].Add(child);

                if (depthReached)
                    break;
            }

            return siblings;
        }

        public void IncrementSubtree(RenderMode mode)
        {
            if (subtreeDepth + depth < maxDepth)
                InstantiateNextLevel(mode);
        }

        public void InstantiateNextLevel(RenderMode mode)
        {
            List<Linker.Container> nodes;
            List<Linker.Container> instantiatedNodes;
            Vector3 size;
            Vector3 position;
            Vector3 parentPosition = self.transform.position;
            int nrOfLevels = 0;
            int nrOfNodes = 0;
            int childDepth = 0;
            float heightMultiplier = 2;
            float radius = .1f;
            float deltaTheta = 0f;
            float theta = 0f;
            float nodeSeparation = 1.25f; // Separate nodes with a gap of one whole node inbetween
            float nodeSize = 0.25f;

            nodes = GetNextLevel();

            if (parent != null && nodes.Count != 0)
            {
                instantiatedNodes = root.GetInstantiatedNodes(nodes[0].depth);
                foreach (Linker.Container node in instantiatedNodes)
                    nodes.Add(node);
            }

            if (nodes.Count != 0)
                childDepth = nodes[0].depth - this.depth;

            nrOfNodes = nodes.Count;

            deltaTheta = (2f * Mathf.PI) / nrOfNodes;
            radius = nrOfNodes / (Mathf.PI / (nodeSize / nodeSeparation));

            for (int i = 0; i < nrOfNodes; i++) // Instantiate all nodes at level = i
            {
                size = new Vector3(.25f, .25f, .25f);
                position = new Vector3(rootPosition.x + radius * Mathf.Cos(theta), parentPosition.y + heightMultiplier * childDepth, rootPosition.z + radius * Mathf.Sin(theta));

                if (!nodes[i].isInstantiated)
                {

                    nodes[i].folderPrefab = this.folderPrefab;
                    nodes[i].filePrefab = this.filePrefab;
                    nodes[i].linePrefab = this.linePrefab;

                    InstantiateNode(nodes[i], position, size);
                }

                else
                {
                    nodes[i].self.SetActive(true);
                    nodes[i].self.transform.position = position;
                }

                theta += deltaTheta;
            }

            if (isDrawingLine)
                root.EnableSubtreeLines();
        }

        public void DecrementSubtree(RenderMode mode)
        {
            if (subtreeDepth != 0) // there is no subtree to decrement
                DestantiateSubtree(mode);
        }

        public void DestantiateSubtree(RenderMode mode)
        {
            List<Linker.Container> instantiatedNodes = new List<Linker.Container>();
            int nrOfInstantiated = 0;
            float deltaTheta = 0;
            float radius = 0;
            float theta = 0;
            Vector3 position;
            List<Linker.Container> levels = GetLastLevel();

            int depth = levels[0].depth;
            float nodeSize = 0.25f;
            float nodeSeparation = 1.25f;
            float height = levels[0].self.transform.position.y;
            foreach (Linker.Container leaf in levels)
            {
                leaf.self.SetActive(false);
                leaf.DisableNodeLine();
            }

            nrOfInstantiated = instantiatedNodes.Count;
            deltaTheta = (2f * Mathf.PI) / nrOfInstantiated;
            radius = nrOfInstantiated / (Mathf.PI / (nodeSize / nodeSeparation));

            for (int i = 0; i < nrOfInstantiated; i++)
            {
                position = new Vector3(rootPosition.x + radius * Mathf.Cos(theta), height, rootPosition.z + radius * Mathf.Sin(theta));
                instantiatedNodes[i].self.transform.position = position;

                theta += deltaTheta;
            }
            if (isDrawingLine)
                root.EnableSubtreeLines();
        }

        private List<Linker.Container> GetLastLevel()
        {
            Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
            List<Linker.Container> levels = new List<Linker.Container>();
            int combinedDepth = this.depth + this.subtreeDepth;
            Linker.Container parent;

            childrenQueue.Enqueue(this);
            while (childrenQueue.Count != 0)
            {
                parent = childrenQueue.Dequeue();
                if (parent.subtreeDepth == 1 && combinedDepth == parent.subtreeDepth + parent.depth) // Reached the 'leaf' nodes
                {
                    foreach (Linker.Container child in parent.children)
                        levels.Add(child);
                }
                else
                {
                    foreach (Linker.Container child in parent.children) // Keep going down subtree
                        childrenQueue.Enqueue(child);
                }
            }

            DescendAndAdd(levels, -1);

            return levels;
        }

        private void DescendAndAdd(List<Linker.Container> nodes, int value)
        {
            Dictionary<int, Linker.Container> dict = new Dictionary<int, Linker.Container>();
            Queue<Linker.Container> parentQueue = new Queue<Linker.Container>();
            Linker.Container parent;
            int parentCombinedDepth = 0;
            int childMax = -1;
            // first node is a leaf, combinedDepth that shit and then check the parents for it

            foreach (Linker.Container node in nodes) // Add the leaf nodes parents to queue
            {
                node.subtreeDepth += value;
                parentQueue.Enqueue(node.parent);
            }


            while (parentQueue.Count != 0) // Process all parents
            {
                parent = parentQueue.Dequeue();
                if (parent.children.Count == 1) // If there is only one child, update parent
                {
                    if (!dict.ContainsKey(parent.id))
                    {
                        dict.Add(parent.id, parent);
                        parent.subtreeDepth += value;
                    }
                }
                if (parent.children.Count > 1)
                {
                    parentCombinedDepth = parent.subtreeDepth + parent.depth;

                    foreach (Linker.Container child in parent.children) // Find max combined depth of all the children
                        if (child.depth + child.subtreeDepth > childMax)
                            childMax = child.depth + child.subtreeDepth;

                    if (value > 0)
                    {
                        if (childMax > parentCombinedDepth) // If max > parent, update parent
                        {
                            if (!dict.ContainsKey(parent.id))
                            {
                                dict.Add(parent.id, parent);
                                parent.subtreeDepth += value;
                            }
                        }
                    }

                    else if (value < 0)
                    {
                        if (childMax < parentCombinedDepth)  // If max < parent, update parent
                        {
                            if (!dict.ContainsKey(parent.id))
                            {
                                dict.Add(parent.id, parent);
                                parent.subtreeDepth += value;
                            }
                        }
                    }
                }

                if (parent.parent != null)
                    parentQueue.Enqueue(parent.parent);
            }
        }

        private List<Linker.Container> GetNextLevel()
        {
            Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
            Queue<Linker.Container> leafQueue = new Queue<Linker.Container>();
            List<Linker.Container> nodes = new List<Linker.Container>();
            int combinedDepth = this.depth + this.subtreeDepth;
            Linker.Container parent;

            if (this.parent == null)
                Debug.Log("its the root");
            childrenQueue.Enqueue(this);
            while (childrenQueue.Count != 0)
            {
                parent = childrenQueue.Dequeue();
                Debug.Log(parent.name + ", stD: " + parent.subtreeDepth + ", cmb: " + combinedDepth + ", p.cmb" + (parent.depth + parent.subtreeDepth));
                if (parent.subtreeDepth == 0 && combinedDepth == parent.depth + parent.subtreeDepth) // Reached the 'leaf' nodes
                    foreach (Linker.Container child in parent.children)
                        nodes.Add(child);

                else if (parent.subtreeDepth > 0)
                    foreach (Linker.Container child in parent.children)
                        childrenQueue.Enqueue(child);
            }

            DescendAndAdd(nodes, 1);

            return nodes;
        }

        private List<List<Linker.Container>> GetAllLevels(int depth)
        {
            List<List<Linker.Container>> levels = new List<List<Linker.Container>>();
            if (depth == 0)
                return levels;

            int newSubtreeDepth = -1;

            if (this.subtreeDepth > -1)
            {
                newSubtreeDepth = this.subtreeDepth + depth;
                this.subtreeDepth = newSubtreeDepth;
            }

            int depthOffset = this.depth + 1;
            Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
            Linker.Container child = this; // 'this' is the root
            int level = 0;
            bool depthReached = false;

            //levels.Add(new List<Linker.Container>());
            //levels[level++].Add(child); // Add root node on a single level

            levels.Add(new List<Linker.Container>());
            foreach (Linker.Container grandchild in child.children) // Add the parent's children to the queue
                childrenQueue.Enqueue(grandchild);

            while (childrenQueue.Count != 0)
            {
                child = childrenQueue.Dequeue();
                foreach (Linker.Container grandchild in child.children) // Add the parent's children to the queue
                    childrenQueue.Enqueue(grandchild);

                if (child.depth > depthOffset + level)
                {
                    if (level + 1 == depth)
                        depthReached = true;
                    else
                    {
                        level++;
                        levels.Add(new List<Linker.Container>());
                    }
                }
                if (depthReached)
                    break;

                child.subtreeDepth = newSubtreeDepth;
                levels[level].Add(child);
            }

            return levels;
        }

        public void ToggleSubtree(RenderMode mode)
        {
            if (this.size != 0) // A file has no subtree
                return;

            Linker.Container child = this.children[0];
            if (child.isInstantiated) // The tree has been instantiated
            {
                if (child.self.activeSelf) // GameObject is active
                {
                    DisableSubtree();
                    ToggleSubtreeLines();
                }
                else
                    EnableSubtree();
            }

            else
                InstantiateSubtree(mode, 1);
        }

        private void EnableSubtree()
        {
            int depth = 0;
            Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();

            foreach (Linker.Container child in this.children)
                childrenQueue.Enqueue(child);

            while (childrenQueue.Count != 0)
            {
                parent = childrenQueue.Dequeue();
                if (parent.subtreeDepth == 0) // Stop if we have reached end of subtree
                    return;
                else
                    depth = parent.depth;
                parent.self.SetActive(true);

                foreach (Linker.Container child in parent.children)
                    childrenQueue.Enqueue(child);
            }
        }

        private void DisableSubtree()
        {
            int depth = 0;
            Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();

            foreach (Linker.Container child in this.children)
                childrenQueue.Enqueue(child);

            while (childrenQueue.Count != 0)
            {
                parent = childrenQueue.Dequeue();
                if (parent.subtreeDepth == 0) // Stop if we have reached end of subtree
                    return;
                else
                    depth = parent.depth;
                parent.self.SetActive(false);

                foreach (Linker.Container child in parent.children)
                    childrenQueue.Enqueue(child);
            }
        }

        public void InstantiateSubtree(RenderMode mode, int depth) // Circular rendering
        {
            List<List<Linker.Container>> nodes;
            Vector3 size;
            Vector3 position;
            Vector3 parentPosition = self.transform.position;
            int nrOfLevels = 0;
            int nrOfNodes = 0;
            int childDepth = 0;
            float heightMultiplier = 2;
            float radius = .1f;
            float deltaTheta = 0f;
            float theta = 0f;
            float nodeSeparation = 1.25f; // Separate nodes with a gap of one whole node inbetween
            float nodeSize = 0.25f;

            nodes = GetNodes(mode, depth);

            nrOfLevels = nodes.Count;

            for (int i = 0; i < nrOfLevels; i++) // Create all folderPrefabs from the 2d list of nodes
            {
                if (nodes[i].Count != 0)
                    childDepth = nodes[i][0].depth - this.depth;

                nrOfNodes = nodes[i].Count;

                deltaTheta = (2f * Mathf.PI) / nrOfNodes;
                radius = nrOfNodes / (Mathf.PI / (nodeSize / nodeSeparation));

                for (int j = 0; j < nrOfNodes; j++) // Instantiate all nodes at level = i
                {
                    if (!nodes[i][j].isInstantiated)
                    {
                        size = new Vector3(.25f, .25f, .25f);
                        position = new Vector3(parentPosition.x + radius * Mathf.Cos(theta), parentPosition.y + heightMultiplier * childDepth, parentPosition.z + radius * Mathf.Sin(theta));

                        nodes[i][j].folderPrefab = this.folderPrefab;
                        nodes[i][j].filePrefab = this.filePrefab;
                        nodes[i][j].linePrefab = this.linePrefab;

                        InstantiateNode(nodes[i][j], position, size);
                    }
                    else
                        nodes[i][j].self.SetActive(true);

                    theta += deltaTheta;
                }
            }
        }

        public void InstantiateNode(Linker.Container node, Vector3 position, Vector3 size)
        {
            if (node.size == 0)
                node.self = Instantiate(node.folderPrefab, new Vector3(position.x, position.y, position.z), Quaternion.identity);
            else
                node.self = Instantiate(node.filePrefab, new Vector3(position.x, position.y, position.z), Quaternion.identity);

            node.line = Instantiate(node.linePrefab);
            node.isInstantiated = true;
            node.self.GetComponent<Linker>().container = node;
            node.self.transform.localScale = size; // Change size
            node.self.GetComponent<Renderer>().material.color = node.color; // Change color
        }

        public void ToggleSubtreeLines()
        {
            if (children.Count == 0)
                return;

            bool drawing = false;

            foreach (Linker.Container child in children)
            {
                if (child.self == null) // Null check
                    return;

                if (!child.self.activeSelf) // If tree is inactive, disable lines
                {
                    DisableSubtreeLines();
                    return;
                }

                if (child.isDrawingLine) // If any of the children are drawing lines, disable all drawing
                {
                    drawing = true;
                    break;
                }
            }

            if (drawing)
                DisableSubtreeLines();
            else
                EnableSubtreeLines();
        }

        private void DisableSubtreeLines()
        {
            Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
            Linker.Container parent;
            if (id == 0) // root
                isDrawingLine = false;

            childrenQueue.Enqueue(this);
            while (childrenQueue.Count != 0)
            {
                parent = childrenQueue.Dequeue();

                foreach (Linker.Container child in parent.children)
                {
                    if (child.isInstantiated && child.self.activeSelf)
                    {
                        childrenQueue.Enqueue(child);

                        child.line.GetComponent<LineRenderer>().positionCount = 0;
                        child.isDrawingLine = false;
                    }
                    else if (child.isDrawingLine && !child.self.activeSelf)
                    {
                        child.line.GetComponent<LineRenderer>().positionCount = 0;
                        child.isDrawingLine = false;
                    }
                }
            }
        }

        private void EnableSubtreeLines()
        {
            Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
            Linker.Container parent;
            Vector3 parentPos;
            Color parentColor;
            Vector3 childPos;
            Color childColor;

            if (id == 0) // root
                isDrawingLine = true;

            childrenQueue.Enqueue(this);
            while (childrenQueue.Count != 0)
            {
                parent = childrenQueue.Dequeue();

                parentPos = parent.self.transform.position;
                parentColor = parent.color;

                foreach (Linker.Container child in parent.children)
                {
                    if (child.isInstantiated && child.self.activeSelf)
                    {
                        Debug.Log(child.depth);
                        childrenQueue.Enqueue(child);
                        childPos = child.self.transform.position;

                        LineRenderer lineRenderer = child.line.GetComponent<LineRenderer>();
                        if (lineRenderer.positionCount == 0)
                        {
                            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                            lineRenderer.startColor = child.color;
                            lineRenderer.endColor = parentColor;
                        }
                        lineRenderer.positionCount = 0;
                        lineRenderer.positionCount = 2;
                        lineRenderer.SetPosition(0, new Vector3(childPos.x, childPos.y, childPos.z));
                        lineRenderer.SetPosition(1, new Vector3(parentPos.x, parentPos.y, parentPos.z));

                        child.isDrawingLine = true;
                    }
                    else if (child.isDrawingLine && !child.self.activeSelf)
                    {
                        child.line.GetComponent<LineRenderer>().positionCount = 0;
                        child.isDrawingLine = false;
                    }
                }
            }
        }

        private void EnableNodeLine()
        {
            Vector3 pos = self.transform.position;
            Vector3 parentPos = parent.self.transform.position;

            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 0; // reset line
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, new Vector3(pos.x, pos.y, pos.z));
            lineRenderer.SetPosition(1, new Vector3(parentPos.x, parentPos.y, parentPos.z));
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = color;
            lineRenderer.endColor = parent.color;

            isDrawingLine = true;
        }

        private void DisableNodeLine()
        {
            line.GetComponent<LineRenderer>().positionCount = 0;
            isDrawingLine = false;
        }

        private void Move(Vector3 increment)
        {
            self.transform.position += increment;
        }

        public void MoveSubtree(Vector3 increment)
        {
            Move(increment);

            if (children.Count == 0) // Basecase
                return;

            foreach (Linker.Container child in children) // Move all nodes recursively
                child.MoveSubtree(increment);
        }

        private int RecursiveDepth(Linker.Container node, int depth)
        {
            if (node.parent == null)
                return depth;

            return RecursiveDepth(node.parent, ++depth);
        }

        private int RecursiveSize(Linker.Container node, int size)
        {
            foreach (Linker.Container child in node.children)
            {
                if (child.size != 0) // leaf node, no children
                    size += child.size;
                else
                    size = RecursiveSize(child, size);
            }

            return size;
        }

        public int GetDepth()
        {
            return RecursiveDepth(this, 0);
        }

        public int GetSize()
        {
            if (size > 0) // File
                return size;

            return RecursiveSize(this, 0);
        }

        public void Print()
        {
            string output = System.String.Empty;
            if (size == 0) // a folder has no size, only files has
                output += "Type = folder";
            else
                output += "Type = file";

            output += ". Name = " + name;

            if (parent == null) // root
                output += ". Parent = null";
            else
                output += ". Parent = " + parent.name;
            output +=
            ". Id = " + id +
            ". Depth = " + depth +
            ". Subtree depth = " + subtreeDepth +
            ". Max depth = " + maxDepth +
            ". Size = " + size +
            ". Weight = " + weight +
            ". Number of children = " + children.Count +
            ". Number of siblings = " + siblings.Count;

            Debug.Log(output);
        }

        public string ToString()
        {
            string output = System.String.Empty;
            if (size == 0) // a folder has no size, only files has
                output += "Type = folder\n";
            else
                output += "Type = file\n";

            output += "Name = " + name + "\n";

            if (parent == null) // root
                output += "Parent = null\n";
            else
                output += "Parent = " + parent.name + "\n";
            output +=
            "Id = " + id + "\n" +
            "Depth = " + depth + "\n" +
            "Subtree depth = " + subtreeDepth + "\n" +
            "Max depth = " + maxDepth + "\n" +
            "Size = " + size + "\n" +
            "Weight = " + weight + "\n" +
            "Number of children = " + children.Count + "\n" +
            "Number of siblings = " + siblings.Count;

            return output;
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

        public int GetMaxFileSize()
        {
            Queue<Linker.Container> parentQueue = new Queue<Linker.Container>();
            List<Linker.Container> files = new List<Linker.Container>();
            Linker.Container parent = null;
            int maxFileSize = 0;

            parentQueue.Enqueue(this);
            while (parentQueue.Count != 0)
            {
                parent = parentQueue.Dequeue();
                foreach (Linker.Container child in parent.children)
                {
                    if (child.size != 0)
                        files.Add(child);
                    else
                        parentQueue.Enqueue(child);
                }
            }

            foreach (Linker.Container file in files)
                if (file.size > maxFileSize)
                    maxFileSize = file.size;

            return maxFileSize;
        }
    }
}