using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;


[CustomEditor(typeof(Node), true)]
class NodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //Node script = (ObjectBuilderScript)target;
        if (GUILayout.Button("Build Object"))
        {
            UpdateLayout();
        }
    }

    private EventTrigger.Entry GetTriggerOfType(List<EventTrigger.Entry> triggers, EventTriggerType type)
    {
        foreach(EventTrigger.Entry entry in triggers)
        {
            if (entry.eventID == type)
                return entry;
        }
        return null;
    }


    public virtual void UpdateLayout()
    {
        if (!(target is Node))
            throw new System.Exception("UpdateLayout called on an item that isn't a Node.");

        UpdateLayout((Node)target);
    }

    static public void UpdateLayout(Node node)
    {
        Node uninstantiatedNode = node;
        GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(node.gameObject);
        if (instantiatedPrefab == null)
            throw new System.Exception("Failed to instantiate prefab.");
        node = instantiatedPrefab.GetComponent<Node>();
        if (node == null)
            throw new System.Exception("The instantiate prefab doesn't have a Node component.");

        const float socketWidth = 20.0f;
        const float socketHeight = 20.0f;
        const float zOffset = -2.0f;
        const float labelHeight = 30.0f;
        const float rowHeight = 36.0f;
        const float fieldHeight = 30.0f;
        const float fieldWidth = 100f;
        const float nodeBorder = 0f;
        Vector2 defaultAnchor = new Vector2(0.5f, 0.5f);

        /*
            For self:
            - If it doesn't already have a background quad, create one.
            - Set the position of the background quad to (0,0,?).
            - Set the background quad size to the width and height of our rect transform.
            - Set the background mesh material to the node material.
            For both self and each child:
            - set alpha of any Image components to fully transparent (zero).
            - set all children to z position of -2.0f.
            - If sink/source socket, set the mesh material to the socket material.

            Not ready for this yet, but this code should probably do the positional layouts, too.
        */

        RectTransform rt = node.GetComponent<RectTransform>();
        rt.anchorMin = defaultAnchor;
        rt.anchorMax = defaultAnchor;

        int node_row_count = CountRows(node);
        int node_col_count = CountColumns(node);

        float height = node_row_count * rowHeight + labelHeight;
        float width = fieldWidth * node_col_count + 15f * 2;
        rt.sizeDelta = new Vector2(width, height);

        SerializedObject so = new SerializedObject(rt);
        SerializedProperty sp = so.FindProperty("m_SizeDelta");
        PrefabUtility.ApplyPropertyOverride(sp, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(rt), InteractionMode.UserAction);

        PrepareBackgroundMesh(node.gameObject, node.nodeBackgroundMaterial); // prepare background on the node
        node.gameObject.transform.position = Vector3.zero;

        Text title = GetTitle(node);
        if (title)
        {
            GameObject titleGo = title.gameObject;
            RectTransform titleRT = titleGo.GetComponent<RectTransform>();
            titleRT.anchorMin = defaultAnchor;
            titleRT.anchorMax = defaultAnchor;
            titleRT.sizeDelta = new Vector2(width, fieldHeight);
            titleRT.anchoredPosition3D = new Vector3(0f, height / 2 - (rowHeight / 2), zOffset);
            titleRT.pivot = defaultAnchor;
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
        }

        SetImageComponentTransparency(node.gameObject);

        int node_row_index = 0;
        foreach (NodeRow nr in node.GetComponents<NodeRow>())
        {
            if (nr.sink)
            {
                SetImageComponentTransparency(nr.sink.gameObject);

                GameObject backgroundMesh = PrepareBackgroundMesh(nr.sink.gameObject, node.socketBackgroundMaterial);

                RectTransform socketRT = nr.sink.GetComponent<RectTransform>();
                socketRT.anchorMax = defaultAnchor;
                socketRT.anchorMin = defaultAnchor;
                socketRT.sizeDelta = new Vector2(socketWidth, socketHeight);
                socketRT.pivot = defaultAnchor;
                Vector3 newPosition = Vector3.zero;
                newPosition.x = -width / 2; // left hand side of the node
                newPosition.y = (height / 2) - (labelHeight + node_row_index * rowHeight + rowHeight / 2);
                newPosition.z = zOffset;

                socketRT.anchoredPosition3D = newPosition;
            }
            if(nr.label)
            {
                RectTransform labelRT = nr.label.GetComponent<RectTransform>();
                labelRT.anchorMax = defaultAnchor;
                labelRT.anchorMin = defaultAnchor;
                labelRT.sizeDelta = new Vector2(fieldWidth, fieldHeight);
                labelRT.pivot = defaultAnchor;
                Vector3 newPosition = Vector3.zero;
                newPosition.x = -(fieldWidth - fieldWidth / node_col_count); // left-hand column
                newPosition.y = (height / 2) - (labelHeight + node_row_index * rowHeight + rowHeight / 2);
                newPosition.z = zOffset;
                labelRT.anchoredPosition3D = newPosition;
                Text labelText = nr.label.GetComponent<Text>();
                labelText.alignment = TextAnchor.MiddleLeft;
                labelText.fontStyle = FontStyle.Normal;
            }
            if (nr.field)
            {
                if (nr.field.GetComponent<Button>() || nr.field.GetComponent<InputField>() || nr.field.GetComponent<Dropdown>() || (nr.field.GetComponent<Text>() && nr.field.GetComponent<Text>() != title))
                {
                    RectTransform fieldRT = nr.field.GetComponent<RectTransform>();
                    fieldRT.anchorMax = defaultAnchor;
                    fieldRT.anchorMin = defaultAnchor;
                    fieldRT.sizeDelta = new Vector2(fieldWidth, fieldHeight);
                    fieldRT.pivot = defaultAnchor;
                    Vector3 newPosition = Vector3.zero;
                    newPosition.x = fieldWidth - fieldWidth / node_col_count;
                    newPosition.y = (height / 2) - (labelHeight + node_row_index * rowHeight + rowHeight / 2);
                    newPosition.z = zOffset;
                    fieldRT.anchoredPosition3D = newPosition;
                }
            }
            if (nr.source)
            {
                SetImageComponentTransparency(nr.source.gameObject);

                GameObject backgroundMesh = PrepareBackgroundMesh(nr.source.gameObject, node.socketBackgroundMaterial);

                RectTransform socketRT = nr.source.GetComponent<RectTransform>();
                socketRT.anchorMax = defaultAnchor;
                socketRT.anchorMin = defaultAnchor;
                socketRT.sizeDelta = new Vector2(socketWidth, socketHeight);
                socketRT.pivot = defaultAnchor;
                Vector3 newPosition = Vector3.zero;
                newPosition.x = width / 2; // right hand side of the node
                newPosition.y = (height / 2) - (labelHeight + node_row_index * rowHeight + rowHeight / 2);
                newPosition.z = zOffset;

                socketRT.anchoredPosition3D = newPosition;
            }

            node_row_index++;
        }

        PrefabUtility.ApplyPrefabInstance(node.gameObject, InteractionMode.UserAction);
        DestroyImmediate(node.gameObject);
    }


    //int CalculateLengthOfMessage(string message)
    //{
    //    int totalLength = 0;

    //    Font myFont = chatText.font;  //chatText is my Text component
    //    CharacterInfo characterInfo = new CharacterInfo();

    //    char[] arr = message.ToCharArray();

    //    foreach (char c in arr)
    //    {
    //        myFont.GetCharacterInfo(c, out characterInfo, chatText.fontSize);

    //        totalLength += characterInfo.advance;
    //    }

    //    return totalLength;
    //}



    private bool IsTitle(GameObject go)
    {
        return go.GetComponent<Text>();
    }


    private bool IsTitle(Transform t)
    {
        return IsTitle(t.gameObject);
    }


    static private Text GetTitle(Node node)
    {
        foreach(Transform child in node.transform)
        {
            Text retVal = child.GetComponent<Text>();
            if (retVal)
                return retVal;
        }
        return null;
    }


    /*
      Returns the number of rows in the node.
      Doesn't include the title.
    */
    static private int CountRows(Node node)
    {
        return node.GetComponents<NodeRow>().Length;
    }


    static private int CountColumns(Node node)
    {
        foreach(NodeRow nodeRow in node.GetComponents<NodeRow>())
        {
            if (nodeRow.label && nodeRow.field)
                return 2;
        }
        return 1;
    }


    static private void SetImageComponentTransparency(GameObject go)
    {
        Image imageComponent = go.GetComponent<Image>();
        if (imageComponent)
        {
            Color c = imageComponent.color;
            if (go.GetComponent<InputField>() || go.GetComponent<Button>() || go.GetComponent<Dropdown>())
                c.a = 1.0f;
            else
                c.a = 0.0f;
            imageComponent.color = c;
        }
    }


    static private GameObject GetBackgroundMesh(GameObject go)
    {
        /*
            A background mesh is a game object which has
            a mesh that is a child of the "go" argument.
        */
        GameObject backgroundMeshObject = null;
        bool found = false;
        foreach(Transform child in go.transform)
        {
            if (child.name == "Background Mesh" && found == false)
            {
                backgroundMeshObject = child.gameObject;
                found = true;
            }
            else if (child.name == "Background Mesh" && found == true)
                throw new System.Exception("There are multiple child game objects named \"Background Mesh.\"  There should be only one.");
        }
        return backgroundMeshObject;
    }


    static private GameObject PrepareBackgroundMesh(GameObject go, Material mat)
    {
        MeshFilter mf = null;
        MeshRenderer mr = null;
        RoundedQuadMesh rqm = null;

        GameObject backgroundMesh = GetBackgroundMesh(go);
        if (backgroundMesh != null)
        {
            mf = backgroundMesh.GetComponent<MeshFilter>();
            mr = backgroundMesh.GetComponent<MeshRenderer>();
            rqm = backgroundMesh.GetComponent<RoundedQuadMesh>();
        }
        else
        {
            backgroundMesh = new GameObject("Background Mesh");
        }

        if (mf == null)
            mf = backgroundMesh.AddComponent<MeshFilter>();
        if (mr == null)
            mr = backgroundMesh.AddComponent<MeshRenderer>();
        if (rqm == null)
            rqm = backgroundMesh.AddComponent<RoundedQuadMesh>();

        mf.mesh = null;

        backgroundMesh.transform.SetParent(go.transform);

        RectTransform parentRect = go.GetComponent<RectTransform>();

        backgroundMesh.transform.localScale = Vector3.one;
        backgroundMesh.transform.localPosition = Vector3.zero;
        backgroundMesh.transform.localRotation = Quaternion.identity;

        rqm.AutoUpdate = false;
        rqm.UsePercentage = false;
        rqm.RoundEdges = 10f;
        rqm.rect.x = -parentRect.rect.width / 2f;
        rqm.rect.y = -parentRect.rect.height / 2f;
        rqm.rect.width = parentRect.rect.width;
        rqm.rect.height = parentRect.rect.height;
        
        mr.material = mat;

        return backgroundMesh;
    }
}


[CustomEditor(typeof(NodeBuilder), true)]
class NodeBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //Node script = (ObjectBuilderScript)target;
        if (GUILayout.Button("Build All Nodes"))
        {
            BuildAllNodes();
        }
    }


    public void BuildAllNodes()
    {
        NodeBuilder nodeBuilder = (NodeBuilder)target;
        foreach(GameObject nodeGO in nodeBuilder.nodes)
        {
            Node node = nodeGO.GetComponent<Node>();
            NodeEditor.UpdateLayout(node);
        }
    }
}
