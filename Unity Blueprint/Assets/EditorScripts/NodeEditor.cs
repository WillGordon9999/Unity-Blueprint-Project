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

    public static List<Node> nodes;
    public static List<Connection> connections;
    public static List<Var> vars;

    //General sizing variables - no more hard-coding
    float nodeWidth = 300.0f;
    float nodeHeight = 50.0f;
    Vector2 connectionLineTangent = Vector2.left * 50.0f;
    float connectionLineWidth = 2.0f;

    //FILE I/O
    
    public static BlueprintData current;

    string text;
    BlueprintData loadData;
    bool isLoading = false;
    bool createNew = false;
    bool enterPressed = false;
    bool connectionRemoved = false;
    Rect createLoadRect;
    string blueprintName;
    VariableDisplay varDisplay;
    Vector2 scrollPos;

    [MenuItem("Window/NodeEditor")]
    static void OpenWindow()
    {
        NodeEditor window = GetWindow<NodeEditor>();
        window.titleContent = new GUIContent("Node Editor");       
    }

    private void OnDestroy()
    {
        Debug.Log("On destroy editor");
        //Automatically does a null check
        current = null;
        text = "";
        nodes?.Clear();
        connections?.Clear();
        EditorUtility.SetDirty(current);
        AssetDatabase.Refresh();
    }

    private void OnEnable()
    {        
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

        varDisplay = new VariableDisplay();
        varDisplay.style = nodeStyle;
        //varDisplay.style.alignment = TextAnchor.UpperLeft;
        //varDisplay.style.contentOffset = Vector2.one * 30.0f;
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

        if (current != null)
        {
            DrawConnections();
            DrawNodes();
            DrawConnectionLine(Event.current);
            scrollPos = varDisplay.Update(scrollPos);
        }
            
        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        connectionRemoved = false;

        if (createNew)
        {
            text = GUI.TextField(createLoadRect, text);

            if (enterPressed)
            {                
                current = Interpreter.Instance.LoadBlueprint(text);

                if (current == null)
                {
                    //current = Interpreter.Instance.CreateAsset<BlueprintData>("Assets/" + text + ".asset");
                    loadData = Interpreter.Instance.CreateAsset<BlueprintData>("Assets/" + text + ".asset");

                    current = ScriptableObject.CreateInstance<BlueprintData>();

                    current.ComponentName = text;
                    blueprintName = text;
                    current.ID_Count = 0; //just to be sure                    
                    createNew = false;
                    enterPressed = false;
                    text = "";
                }

                else
                {
                    Debug.Log("A file with that name exists");
                }

            }

        }

        if (isLoading)
        {
            loadData = (BlueprintData)EditorGUI.ObjectField(createLoadRect, loadData, typeof(BlueprintData), true);

            if (loadData != null)
            {
                Debug.Log("Found blueprint");                               
                LoadBlueprint();
                isLoading = false;
                //varDisplay.current = current;
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
    }

    void DrawConnections()
    {
        if (connections != null && !connectionRemoved)
        {
            foreach(Connection connection in connections)
            {
                connection.Draw();
            }
        }      
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
    }

    void ProcessContextMenu(Vector2 mousePos)
    {
        GenericMenu menu = new GenericMenu();
        if (current != null)
        {            
            menu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePos));
            menu.AddItem(new GUIContent("Add Member node"), false, () => OnClickAddNode(mousePos, true));
            menu.AddItem(new GUIContent("Save Blueprint"), false, () => SaveBlueprint());
            menu.AddItem(new GUIContent("Load Blueprint"), false, () => ToggleLoadBlueprintUI(mousePos));
            menu.AddItem(new GUIContent("New Blueprint"), false, () => ToggleNewBlueprintUI(mousePos));
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
        else
            current.nodes.Clear();
        //Nodes
        current.ID_Count = 0;

        foreach (Node node in nodes)
        {                                
            node.ID = current.ID_Count;
            current.ID_Count++;                                
        }

        foreach (Node node in nodes)
        {
            if (node.nextNode != null)
                node.nextID = node.nextNode.ID;

            if (node.prevNode != null)
                node.prevID = node.prevNode.ID;

            if (node.falseNode != null)
                node.falseID = node.falseNode.ID;

            current.nodes.Add(new NodeData(node));
        }

        //Connections
        if (current.connections == null)
            current.connections = new List<ConnectionData>();

        for (int i = 0; i < current.nodes.Count; i++)        
            current.connections.Add(new ConnectionData(current.nodes[i].inPoint, current.nodes[i].outPoint));
        
        //Actually pass it onto the real scriptable object
        
        loadData.ID_Count = current.ID_Count;

        if (loadData.nodes == null)
            loadData.nodes = new List<NodeData>();
        else
            loadData.nodes.Clear();

        //loadData.connections = new List<ConnectionData>();

        if (loadData.variables == null)
            loadData.variables = new List<Var>();
        else
            loadData.variables.Clear();

        foreach (NodeData node in current.nodes)
            loadData.nodes.Add(node);

        //foreach (ConnectionData con in current.connections)
        //    loadData.connections.Add(con);

        //Variables
        foreach (Var v in current.variables)            
            loadData.variables.Add(v);
        
        EditorUtility.SetDirty(loadData);
        AssetDatabase.Refresh();               
    }

    //TO-DO: Gotta finish this, but for now we need to focus on making sure code can in fact execute
    void LoadBlueprint()
    {
        if (nodes != null)
            nodes.Clear();
        else
            nodes = new List<Node>();

        if (connections != null)
            connections.Clear();
        else
            connections = new List<Connection>();

        //Need to make a copy of the loaded blueprintData, current will serve as this copy
        if (current == null)        
            current = ScriptableObject.CreateInstance<BlueprintData>();
        
        current.ComponentName = loadData.ComponentName;
        current.ID_Count = loadData.ID_Count;
        current.nodes = new List<NodeData>();
        current.connections = new List<ConnectionData>();
        current.variables = new List<Var>();

        foreach (NodeData node in loadData.nodes)
            current.nodes.Add(node);

        foreach (ConnectionData con in loadData.connections)
            current.connections.Add(con);

        //Ready variable display
        if (varDisplay.inputs == null)
            varDisplay.inputs = new List<string>();

        foreach (Var v in loadData.variables)
        {           
            v.type = Interpreter.Instance.LoadVarType(v.strType, v.asmPath);

            //Add to VarDisplay
            if (v.name != null)
                if (v.name != "")
                    varDisplay.inputs.Add(v.input);

            current.variables.Add(v);
        }

        //Do stuff

        //Construct nodes
        foreach(NodeData data in current.nodes)
        {
            Node newNode = new Node(data, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode);
            newNode.style = nodeStyle;
            newNode.defaultNodeStyle = nodeStyle;
            newNode.selectedNodeStyle = selectedNodeStyle;
            nodes.Add(newNode);
            //nodes.Add(new Node(data, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode));            
        }

        //Construct Connections

        //Unfortunately I don't think we can assume the next node will be after the node in question
        foreach(Node node in nodes)
        {
            if (node.nextID != -1)
            {
                foreach(Node node2 in nodes)
                {
                    if (node2.ID == node.nextID)
                    {
                        connections.Add(new Connection(node2.inPoint, node.outPoint, OnClickRemoveConnection));
                        node2.prevNode = node;
                        node.nextNode = node2;
                    }
                }
            }

            if (node.falseID != -1)
            {
                foreach(Node node2 in nodes)
                {
                    if (node2.ID == node.falseID)
                    {
                        connections.Add(new Connection(node2.inPoint, node.falsePoint, OnClickRemoveConnection));
                        node2.prevNode = node;
                        node.falseNode = node2;
                    }
                }
            }
        }            
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
        loadData = null;
        isLoading = true;
        createNew = false;
    }

    void OnClickAddNode(Vector2 mousePos, bool context = false)
    {
        if (nodes == null)
        {
            nodes = new List<Node>();
        }
        Node newNode = new Node(mousePos, nodeWidth, nodeHeight, nodeStyle, selectedNodeStyle, inStyle, outStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode);
        newNode.ID = current.ID_Count;
        newNode.nextID = -1;
        newNode.falseID = -1;
        newNode.isContextual = context;
        current.ID_Count++;
        nodes.Add(newNode);
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

    void OnClickFalsePoint(ConnectionPoint falsePoint)
    {
        
    }

    void OnClickRemoveConnection(Connection connection)
    {      
        connections.Remove(connection);
        connectionRemoved = true;
    }

    void CreateConnection()
    {
        if (connections == null)
            connections = new List<Connection>();

        Connection con = new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection);
        
        if (selectedOutPoint.type == ConnectionPointType.False)        
            selectedOutPoint.node.falseNode = selectedInPoint.node;
        
        else
            selectedOutPoint.node.nextNode = selectedInPoint.node;

        selectedInPoint.node.prevNode = selectedOutPoint.node;

        connections.Add(con);       
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
