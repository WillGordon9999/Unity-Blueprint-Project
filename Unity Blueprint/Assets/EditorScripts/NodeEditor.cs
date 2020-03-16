using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeEditor : EditorWindow
{
    GUIStyle resize;    
    Rect resizeRect;
    bool isResizing;
    float sizeRatio = 0.5f;

    GUIStyle nodeStyle;
    GUIStyle selectedNodeStyle;
    GUIStyle inStyle;
    GUIStyle outStyle;

    ConnectionPoint selectedInPoint;
    ConnectionPoint selectedOutPoint;

    Vector2 offset;
    Vector2 drag;

    static List<Node> nodes;
    static List<Connection> connections;

    //General sizing variables - no more hard-coding
    float nodeWidth = 300.0f;
    float nodeHeight = 50.0f;
    Vector2 connectionLineTangent = Vector2.left * 50.0f;
    float connectionLineWidth = 2.0f;

    //FILE I/O

    public string filePath = "Assets/BlueprintCollection.asset";    
    BlueprintData current;

    string text;
    BlueprintData loadData;
    bool isLoading = false;
    bool createNew = false;
    bool enterPressed = false;
    Rect createLoadRect;
    string blueprintName;


    [MenuItem("Window/NodeEditor")]
    static void OpenWindow()
    {
        NodeEditor window = GetWindow<NodeEditor>();
        window.titleContent = new GUIContent("Node Editor");       
    }

    private void OnDestroy()
    {
        Debug.Log("On destroy editor");
        nodes.Clear();
        connections.Clear();
        EditorUtility.SetDirty(current);
        AssetDatabase.Refresh();
    }

    private void OnEnable()
    {
        Debug.Log("Editor enable");     
        
        if (nodes != null)
        {
            Debug.Log("nodes is not null in editor");
        }

        if (connections != null)
        {
            Debug.Log("connections is not null in editor");
        }

        resize = new GUIStyle();
        resize.normal.background = EditorGUIUtility.Load("icons/d_AvatarBlendBackground.png") as Texture2D;

        nodeStyle = new GUIStyle();
        nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        nodeStyle.border = new RectOffset(12, 12, 12, 12);

        selectedNodeStyle = new GUIStyle();
        selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);

        inStyle = new GUIStyle();
        inStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
        inStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        inStyle.border = new RectOffset(4, 4, 12, 12);

        outStyle = new GUIStyle();
        outStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
        outStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
        outStyle.border = new RectOffset(4, 4, 12, 12);
    }

    private void OnDisable()
    {
       
    }

    private void Update()
    {
        
    }

    private void OnGUI()
    {
       
        DrawGrid(20.0f, 0.2f, Color.gray);
        DrawGrid(100.0f, 0.4f, Color.gray);

        DrawNodes();
        DrawConnections();
        DrawConnectionLine(Event.current);
        
        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        if (createNew)
        {
            text = GUI.TextField(createLoadRect, text);

            if (enterPressed)
            {                
                current = Interpreter.Instance.LoadBlueprint(text);

                if (current == null)
                {
                    current = Interpreter.Instance.CreateAsset<BlueprintData>("Assets/" + text + ".asset");
                    current.ComponentName = text;
                    blueprintName = text;
                    current.ID_Count = 0; //just to be sure
                    createNew = false;
                    enterPressed = false;
                }

                else
                    Debug.Log("A file with that name already exists");

            }

        }

        if (isLoading)
        {
            loadData = (BlueprintData)EditorGUI.ObjectField(createLoadRect, loadData, typeof(BlueprintData), true);

            if (loadData != null)
            {
                Debug.Log("Found blueprint");               
                current = loadData;
                LoadBlueprint();
                isLoading = false;
            }
        }

        if (GUI.changed) Repaint();
    }

    //Still a little uncertain what this is exactly doing
    void DrawResizer()
    {
        resizeRect = new Rect(0, (position.height * sizeRatio) - 5.0f, position.width, 10.0f);
        GUILayout.BeginArea(new Rect(resizeRect.position + (Vector2.up * 5.0f), new Vector2(position.width, 2.0f)), resize);
        GUILayout.EndArea();
    }

    void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int width = Mathf.CeilToInt(position.width / gridSpacing);
        int height = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0.0f);

        for (int i = 0; i < width; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0.0f) + newOffset, new Vector3(gridSpacing * i, position.height, 0.0f) + newOffset);
        }

        for (int j = 0; j < height; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    void DrawNodes()
    {
        if (nodes != null)
        {
            foreach(Node node in nodes)
            {
                node.Draw();
            }
        }

        //if (current == null)
        //    return;

        //if (current.nodes != null)
        //{
        //    foreach (Node node in current.nodes)
        //        node.Draw();
        //}

    }

    void DrawConnections()
    {
        if (connections != null)
        {
            foreach(Connection connection in connections)
            {
                connection.Draw();
            }
        }

        //if (current == null)
        //    return;

        //if (current.connections != null)
        //{
        //    foreach (Connection connection in current.connections)
        //        connection.Draw();
        //}

    }

    void DrawConnectionLine(Event e)
    {
        if (selectedInPoint != null && selectedOutPoint == null)
        {
            Handles.DrawBezier(selectedInPoint.rect.center, e.mousePosition, selectedInPoint.rect.center + connectionLineTangent, e.mousePosition - connectionLineTangent, Color.white, null, connectionLineWidth);
            GUI.changed = true;
        }

        if (selectedOutPoint != null && selectedInPoint == null)
        {
            Handles.DrawBezier(selectedOutPoint.rect.center, e.mousePosition, selectedOutPoint.rect.center - connectionLineTangent, e.mousePosition + connectionLineTangent, Color.white, null, connectionLineWidth);
            GUI.changed = true;
        }
    }

    void ProcessEvents(Event e)
    {
        drag = Vector2.zero;
      
        switch(e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0 && resizeRect.Contains(e.mousePosition))
                    isResizing = true;

                if (e.button == 1)
                {
                    ProcessContextMenu(e.mousePosition);
                }

                break;

            case EventType.MouseUp:
                isResizing = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 2)
                    OnDrag(e.delta);
                break;
            case EventType.KeyDown:
                if (e.keyCode == KeyCode.Return)
                {                   
                    enterPressed = true;
                }
                break;
        }

        Resize(e);
    }

    void OnDrag(Vector2 delta)
    {
        drag = delta;

        if (nodes != null)
        {
            foreach(Node node in nodes)
            {
                node.Drag(delta);
            }
        }

        //if (current == null)
        //{
        //    //Debug.Log("Current is null in OnDrag");
        //    return;
        //}

        //if (current.nodes != null)
        //{
        //    foreach (Node node in current.nodes)
        //        node.Drag(delta);
        //}

        GUI.changed = true;
    }

    void ProcessNodeEvents(Event e)
    {
        if (nodes != null)
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                bool guiChanged = nodes[i].ProcessEvents(e);
        
                if (guiChanged)
                    GUI.changed = true;
            }
        }

        //if (current == null)
        //{
        //    //Debug.Log("Current is null in process node events");
        //    return;
        //}

        //if (current.nodes != null)
        //{
        //    for (int i = current.nodes.Count - 1; i >= 0; i--)
        //    {
        //        bool guiChanged = current.nodes[i].ProcessEvents(e);
        //
        //        if (guiChanged)
        //            GUI.changed = true;
        //    }
        //}

    }

    void ProcessContextMenu(Vector2 mousePos)
    {
        GenericMenu menu = new GenericMenu();
        if (current != null)
        {            
            menu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePos));
            menu.AddItem(new GUIContent("Save Blueprint"), false, () => SaveBlueprint());
            menu.AddItem(new GUIContent("Compile Blueprint"), false, () => CompileBlueprint());
            menu.ShowAsContext();
        }

        else
        {
            menu.AddItem(new GUIContent("New Blueprint"), false, () => ToggleNewBlueprintUI(mousePos));
            menu.AddItem(new GUIContent("Load Blueprint"), false, () => ToggleLoadBlueprintUI(mousePos));
            menu.ShowAsContext();
        }
    }

    void SaveBlueprint()
    {
        //Save nodes
        if (current.nodes == null)
            current.nodes = new List<NodeData>();

        foreach(Node node in nodes)
        {
            current.nodes.Add(new NodeData(node));
        }

        if (current.connections == null)
            current.connections = new List<ConnectionData>();

        for (int i = 0; i < current.nodes.Count; i++)
        {
            current.connections.Add(new ConnectionData(current.nodes[i].inPoint, current.nodes[i].outPoint));            
        }
        
        EditorUtility.SetDirty(current);
        AssetDatabase.Refresh();
    }

    //TO-DO: Gotta finish this, but for now we need to focus on making sure code can in fact execute
    void LoadBlueprint()
    {
        if (nodes != null)
            nodes.Clear();
        
        if (connections != null)
            connections.Clear();

        //Do stuff

        //Blueprint test;
        ////if (Application.isPlaying)
        //if (blueprints != null && !blueprints.TryGetValue(compName, out test))
        //{
        //    print("In construction of blueprint");
        //    Blueprint bp = new Blueprint();
        //    bp.name = data.ComponentName;
        //
        //    foreach(NodeData node in data.nodes)
        //    {
        //        Node newNode = new Node(node, null, null, null);
        //        
        //        if (node.isEntryPoint)
        //        {
        //            //Debug.Log("Entry point confirmed");
        //            bp.entryPoints[node.input] = newNode;
        //        }
        //
        //        bp.nodes.Add(newNode);
        //    }
        //   
        //    foreach(Node node in bp.nodes)
        //    {
        //        //Interpreter.Instance.CompileNode(node);
        //        
        //        foreach(Node node2 in bp.nodes)
        //        {
        //            if (node.nextID == node2.ID)
        //            {
        //                node.nextNode = node2;
        //                break;
        //            }
        //        }
        //    }
        //    
        //
        //    BlueprintManager.blueprints[bp.name] = bp;
        //    //bpTest.Add(bp);
        //}
    }
   
    //Not sure if this is really needed
    void CompileBlueprint()
    {
        Debug.Log("Compiling blueprint");

        if (BlueprintManager.blueprints == null)
        {
            Debug.Log("Instantiating blueprint dictionary");
            //BlueprintManager.blueprints = new Dictionary<string, Blueprint>();
            BlueprintManager.blueprints = new Dictionary<BlueprintData, Blueprint>();
        }
        
        //Blueprint bp = new Blueprint();
        //bp.name = blueprintName;
        //
        //foreach(Node node in nodes)
        //{
        //    Interpreter.Instance.CompileNode(node);
        //    if (node.isEntryPoint)
        //    {
        //        Debug.Log("Entry point confirmed");
        //        bp.entryPoints[node.input] = node;
        //    }
        //
        //    bp.nodes.Add(node);
        //}
        //
        //foreach(Connection con in connections)
        //{
        //    bp.connections.Add(con);
        //}

        //BlueprintManager.blueprints[blueprintName] = bp;        
    }

    void ToggleNewBlueprintUI(Vector2 mousePos)
    {
        createLoadRect = new Rect(mousePos.x, mousePos.y, nodeWidth, nodeHeight);
        createNew = true;
        isLoading = false;
    }

    void ToggleLoadBlueprintUI(Vector2 mousePos)
    {
        createLoadRect = new Rect(mousePos.x, mousePos.y, nodeWidth, nodeHeight);
        isLoading = true;
        createNew = false;
    }

    void OnClickAddNode(Vector2 mousePos)
    {
        if (nodes == null)
        {
            nodes = new List<Node>();
        }
        Node newNode = new Node(mousePos, nodeWidth, nodeHeight, nodeStyle, selectedNodeStyle, inStyle, outStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode);
        newNode.ID = current.ID_Count;
        newNode.nextID = -1;
        current.ID_Count++;
        nodes.Add(newNode);

        //if (current == null)
        //{
        //    Debug.Log("Current is null add node");
        //    return;
        //}

        //if (current.nodes == null)
        //{
        //    current.nodes = new List<Node>();
        //}
        //
        //Node node = new Node(mousePos, nodeWidth, nodeHeight, nodeStyle, selectedNodeStyle, inStyle, outStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode);
        //node.blueprint = current;
        //
        //current.nodes.Add(node);
    }

    void OnClickRemoveNode(Node node)
    {
        if (connections != null)
        {
            List<Connection> old = new List<Connection>();
        
            foreach(Connection connection in connections)
            {
                if (connection.inPoint == node.inPoint || connection.outPoint == node.outPoint)
                    old.Add(connection);
            }
        
            foreach(Connection connection in old)
            {
                connections.Remove(connection);
            }
        
            old = null;
        }
        
        nodes.Remove(node);

        //if (current == null)
        //{
        //    //Debug.Log("Current is null remove node");
        //    return;
        //}

        //if (current.connections != null)
        //{
        //    List<Connection> old = new List<Connection>();
        //
        //    foreach(Connection connection in current.connections)
        //    {
        //        if (connection.inPoint == node.inPoint || connection.outPoint == node.outPoint)
        //            old.Add(connection);
        //    }
        //
        //    foreach (Connection connection in old)
        //        current.connections.Remove(connection);
        //}

        //current.nodes.Remove(node);
    }

    void OnClickInPoint(ConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;

        if (selectedOutPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection();
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    void OnClickOutPoint(ConnectionPoint outPoint)
    {
        selectedOutPoint = outPoint;

        if (selectedInPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection();
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    void OnClickRemoveConnection(Connection connection)
    {
        //if (current == null)
        //{
        //    //Debug.Log("Current is null remove connection");
        //    return;
        //}

        connections.Remove(connection);
        //current.connections.Remove(connection);
    }

    void CreateConnection()
    {
        if (connections == null)
            connections = new List<Connection>();

        Connection con = new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection);
        selectedOutPoint.node.nextID = selectedInPoint.node.ID;
        connections.Add(con);

        //if (current == null)
        //{
        //    Debug.Log("Current is null create connection");
        //    return;
        //}

        //if (current.connections == null)
        //    current.connections = new List<Connection>();
        //
        //current.connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));

    }

    void ClearConnectionSelection()
    {
        selectedInPoint = null;
        selectedOutPoint = null;
    }
    
    void Resize(Event e)
    {
        if (isResizing)
        {
            sizeRatio = e.mousePosition.y / position.height;
            Repaint();
        }
    }
}
