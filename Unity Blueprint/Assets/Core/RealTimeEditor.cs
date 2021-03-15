using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class Menu
{
    class Item
    {
        public string text;
        System.Action func; 
        System.Action<object> func2;
        public object data;
        public bool isArgument = false;

        public Item(string str, System.Action function)
        {
            text = str;
            func = function;
            data = null;
        }

        public Item(string str, System.Action<object> function, object arg)
        {
            text = str;
            func2 = function;
            data = arg;
            isArgument = true;
        }

        public void Invoke()
        {
            if (isArgument)
                func2.Invoke(data);
            else
                func.Invoke();
        }

    }

    List<Item> items;
    public static Vector2 size = new Vector2(200, 200);
    Vector2 scrollPos;
    public bool canDraw = false;
    public bool changed;
    public Rect rect;

    public Menu()
    {
        items = new List<Item>();
    }

    public void AddItem(string text, System.Action function)
    {
        changed = true;
        items.Add(new Item(text, function));
    }

    public void AddItem(string text, System.Action<object> func, object data = null)
    {
        changed = true;
        items.Add(new Item(text, func, data));
    }

    public void ProcessEvents(Event e)
    {
        if (e.type == EventType.MouseDown)
        {
            if (e.button == 1 && !canDraw)
            {
                items.Clear();                
                rect = new Rect(e.mousePosition, size);
                canDraw = true;
                changed = true;
            }

            if (e.button == 0 && canDraw)
            {
                if (!rect.Contains(e.mousePosition))
                {
                    canDraw = false;
                    items.Clear();
                    changed = true;
                }
            }
        }

        else if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Return)
            {
                canDraw = false;
                items.Clear();
            }
        }

        else
        {
            changed = false;
        }
    }

    public void StopDraw()
    {
        items.Clear();
        canDraw = false;
    }

    public void Clear()
    {
        items.Clear();
    }

    public void DrawMenu()
    {
        if (!canDraw)
            return;

        if (!changed)
        {
            GUILayout.BeginArea(rect);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.MaxWidth(rect.width));

            foreach (Item item in items)
            {
                if (GUILayout.Button(item.text))
                {
                    item.Invoke();
                    StopDraw();
                    break;
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }

}

//Could be a component of player or just singleton access
public class RealTimeEditor : MonoBehaviour
{
    static RealTimeEditor mInstance;
    
    static string currentEnumParam;
    static Rect defaultSize = new Rect(0, 0, 200, 20);
    static Vector2 enumScroll;

    float zoomScale = 1.0f;
    const float minZoom = 1.0f;
    const float maxZoom = 2.0f;

    public Menu contextMenu;

    static Menu enumMenu;    
    static System.Enum retEnum;
    static bool enumReturned = false;
    static Rect currentEnum;
    static Parameter enumPar;

    GUIStyle nodeStyle;
    GUIStyle selectedNodeStyle;
    GUIStyle inStyle;
    GUIStyle outStyle;

    ConnectionPoint selectedInPoint;
    ConnectionPoint selectedOutPoint;

    Vector2 offset;
    Vector2 drag;

    string text;
    BlueprintData loadData;
    bool isLoading = false;
    bool isLoadingAssets = false;
    bool createNew = false;
    bool createNewAsset = false;
    bool enterPressed = false;
    bool connectionRemoved = false;
    Rect createLoadRect;
    string blueprintName;
    VariableDisplay varDisplay;
    Vector2 scrollPos;

    //General sizing variables - no more hard-coding
    float nodeWidth = 300.0f;
    float nodeHeight = 50.0f;
    Vector2 connectionLineTangent = Vector2.left * 50.0f;
    float connectionLineWidth = 2.0f;
    

    public bool isRunning = false;
    public List<string> deleteAsmPath;

    public static RealTimeEditor Instance { get { return mInstance; } set { } }

    public List<Node> nodes;
    public List<Connection> connections;
    public BlueprintData current;

    private void Awake()
    {
        if (mInstance == null)
            mInstance = this;

        deleteAsmPath = new List<string>();
        nodeStyle = new GUIStyle();        
        nodeStyle.border = new RectOffset(12, 12, 12, 12);

        selectedNodeStyle = new GUIStyle();        
        selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);

        inStyle = new GUIStyle();        
        inStyle.border = new RectOffset(4, 4, 12, 12);

        outStyle = new GUIStyle();        
        outStyle.border = new RectOffset(4, 4, 12, 12);

        varDisplay = new VariableDisplay();
        varDisplay.style = nodeStyle;

        //Interpreter.Instance.domain = System.AppDomain.CreateDomain("MyDomain");
    }


    // Start is called before the first frame update
    void Start()
    {
        contextMenu = new Menu();
        enumMenu = new Menu();
    }

    // Update is called once per frame
    void Update()
    {        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isRunning = !isRunning;
            Time.timeScale = 1.0f - Time.timeScale;
        }

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            current = null;            
        }

        if (isRunning)
        {
            zoomScale += Input.GetAxis("Mouse ScrollWheel") * 0.1f;
            
            if (zoomScale < minZoom)
                zoomScale = minZoom;
            
            if (zoomScale > maxZoom)
                zoomScale = maxZoom;
        }       
    }

    private void OnGUI()
    {
        if (!isRunning)
            return;
        
        //Background
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), new GUIContent());

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
                enterPressed = false;
                //current = Interpreter.Instance.LoadBlueprint(text);
                if (text != "" && !System.IO.File.Exists(Application.persistentDataPath + "/" + name + ".asset"))
                {
                    loadData = Interpreter.Instance.CreateBlueprint(text);

                    current = ScriptableObject.CreateInstance<BlueprintData>();
                    if (nodes != null) nodes.Clear();
                    if (connections != null) connections.Clear();
                    current.ComponentName = text;
                    blueprintName = text;
                    current.ID_Count = 0; //just to be sure                    
                    createNew = false;                    
                    text = "";
                    current.variables = new List<Var>();
                }

                else
                {
                    Debug.Log("A file with that name exists");
                }

            }

        }

#if UNITY_EDITOR

        if (createNewAsset)
        {
            text = GUI.TextField(createLoadRect, text);

            if (enterPressed)
            {
                enterPressed = false;

                current = Interpreter.Instance.LoadBlueprintAssets(text);


                if (current == null && text != "")
                {

                    loadData = Interpreter.Instance.CreateAsset<BlueprintData>("Assets/" + text + ".asset");

                    current = ScriptableObject.CreateInstance<BlueprintData>();
                    if (nodes != null) nodes.Clear();
                    if (connections != null) connections.Clear();

                    current.ComponentName = text;
                    blueprintName = text;
                    current.ID_Count = 0; //just to be sure                    
                    createNewAsset = false;
                    text = "";
                    current.variables = new List<Var>();
                }

                else
                {
                    Debug.Log("A file with that name exists");
                }

            }

        }

        if (isLoadingAssets)
        {
            //loadData = (BlueprintData)EditorGUI.ObjectField(createLoadRect, loadData, typeof(BlueprintData), true);
            //loadData = JsonUtility.FromJson<BlueprintData>(text);
            text = GUI.TextField(createLoadRect, text);

            if (text != "" && enterPressed)
                loadData = Interpreter.Instance.LoadBlueprintAssets(text);            

            if (loadData != null && enterPressed)
            {
                Debug.Log("Found blueprint");
                LoadBlueprint();
                isLoadingAssets = false;
                enterPressed = false;
                //varDisplay.current = current;
            }
        }
#endif
        if (isLoading)
        {
            //loadData = (BlueprintData)EditorGUI.ObjectField(createLoadRect, loadData, typeof(BlueprintData), true);
            //loadData = JsonUtility.FromJson<BlueprintData>(text);
            text = GUI.TextField(createLoadRect, text);

            if (text != "" && enterPressed)
            {
                loadData = Interpreter.Instance.LoadBlueprint(text);

                if (loadData == null)
                    enterPressed = false;
            }

            if (loadData != null && enterPressed)
            {
                Debug.Log("Found blueprint");
                LoadBlueprint();
                isLoading = false;
                enterPressed = false;
                //varDisplay.current = current;
            }
        }

    }

    void ProcessEvents(Event e)
    {        
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 1)
                    ProcessContextMenu(e);               
                break;

            case EventType.MouseDrag:
                if (e.button == 2)
                    OnDrag(e.delta);
                break;

            case EventType.KeyDown:
                if (e.keyCode == KeyCode.Return)                
                    enterPressed = true;
                break;
        }

        

        contextMenu.ProcessEvents(e);
        contextMenu.DrawMenu();

        enumMenu.ProcessEvents(e);
        enumMenu.DrawMenu();

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

    void ProcessContextMenu(Event e)
    {
        contextMenu.rect = new Rect(e.mousePosition, Menu.size);        
        contextMenu.StopDraw();
                
        if (current != null)
        {
            //contextMenu.AddItem("Add node", () => OnClickAddNode(e.mousePosition));
            //contextMenu.AddItem("Add Member node", () => OnClickAddNode(e.mousePosition, true));
            //contextMenu.AddItem("Save Blueprint", () => SaveBlueprint());

            contextMenu.AddItem("Add node", () => OnClickAddNode(contextMenu.rect.position));
            contextMenu.AddItem("Add Member node", () => OnClickAddNode(contextMenu.rect.position, true));

            contextMenu.AddItem("Save Blueprint", () => SaveBlueprint(false));
            contextMenu.AddItem("Save Blueprint Assets", () => SaveBlueprint(true));                        
        }

        contextMenu.AddItem("Load Blueprint", () => ToggleLoadBlueprintUI(contextMenu.rect.position, false));
        contextMenu.AddItem("Load Blueprint Assets", () => ToggleLoadBlueprintUI(contextMenu.rect.position, true));

        contextMenu.AddItem("New Blueprint", () => ToggleNewBlueprintUI(contextMenu.rect.position, false));
        contextMenu.AddItem("New Blueprint Assets", () => ToggleNewBlueprintUI(contextMenu.rect.position, true));
        contextMenu.AddItem("Compile Blueprint", () => Compile());
        contextMenu.AddItem("Add To Active Inventory", () => { ComponentInventory.Instance.AddClass(current.compiledClassType, current.compiledClassTypeAsmPath); });

        contextMenu.canDraw = true;
    }

    private void OnApplicationQuit()
    {
        current = null;
        nodes = null;
        connections = null;

        //foreach (string path in deleteAsmPath)
        //{
        //    print($"Deleting path {path}");
        //    System.IO.File.Delete(path);
        //}
    }

    void Compile()
    {
        Blueprint bp = new Blueprint();
        bp.name = current.ComponentName;
        bp.nodes = nodes;
        bp.dataRef = current;

        foreach (Node node in nodes)
        {
            if (node.isEntryPoint)
                bp.entryPoints[node.input] = node;
        }

        foreach (Var v in current.variables)
        {
            v.type = Interpreter.Instance.LoadVarType(v.strType, v.asmPath);
            bp.variables[v.name] = v;
        }

        if (!Interpreter.Instance.UseGameCompile)
            Interpreter.Instance.FullCompile(bp, typeof(MonoBehaviour));
        else
            Interpreter.Instance.FullCompile(bp, typeof(GameComponent));
    }

    void OnDrag(Vector2 delta)
    {
        drag = delta;

        if (nodes != null)
        {
            foreach (Node node in nodes)
            {
                node.Drag(delta);
            }
        }

        GUI.changed = true;
    }

    void ToggleNewBlueprintUI(Vector2 mousePos, bool isAsset)
    {
        createLoadRect = new Rect(mousePos.x, mousePos.y, nodeWidth, nodeHeight);

        if (isAsset)
        {
            createNew = false;
            createNewAsset = true;
        }

        else
        {
            createNew = true;
            createNewAsset = false;
        }
        
        isLoading = false;
        isLoadingAssets = false;
        contextMenu.StopDraw();
    }

    void ToggleLoadBlueprintUI(Vector2 mousePos, bool isAsset)
    {
        createLoadRect = new Rect(mousePos.x, mousePos.y, nodeWidth, nodeHeight);
        loadData = null;

        if (isAsset)
        {
            isLoading = false;
            isLoadingAssets = true;
        }
        else
        {
            isLoading = true;
            isLoadingAssets = false;
        }
        
        createNew = false;
        createNewAsset = false;
        contextMenu.StopDraw();
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
        contextMenu.StopDraw();
    }

    void OnClickRemoveNode(Node node)
    {
        if (connections != null)
        {
            List<Connection> old = new List<Connection>();
            
            foreach (Connection connection in connections)
            {
                if (connection.inPoint == node.inPoint || connection.outPoint == node.outPoint)
                    old.Add(connection);
            }

            foreach (Connection connection in old)
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

    void DrawNodes()
    {
        if (nodes != null)
        {
            foreach (Node node in nodes)
            {
                node.Draw(zoomScale);
            }
        }
    }

    void DrawConnections()
    {
        if (connections != null && !connectionRemoved)
        {
            foreach (Connection connection in connections)
            {
                connection.Draw();
            }
        }
    }

    void DrawConnectionLine(Event e)
    {
        if (selectedInPoint != null && selectedOutPoint == null)
        {                        
            //DrawLine(lineStart * zoomScale, e.mousePosition * zoomScale, Color.white);            
            DrawLine(selectedInPoint.rect.center, e.mousePosition, Color.white, connectionLineWidth);
            GUI.changed = true;
        }

        if (selectedOutPoint != null && selectedInPoint == null)
        {
            DrawLine(selectedOutPoint.rect.center, e.mousePosition, Color.white, connectionLineWidth);
            GUI.changed = true;
        }
    }

    void SaveBlueprint(bool editorSave)
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

        if (current.compiledClassType != null)
        {
            loadData.compiledClassType = current.compiledClassType;
        }

        if (current.compiledClassTypeAsmPath != null)
        {
            loadData.compiledClassTypeAsmPath = current.compiledClassTypeAsmPath;
        }

        loadData.ComponentName = current.ComponentName;

        //Actually pass it onto the real scriptable object
        if (editorSave)
        {
            loadData.ID_Count = current.ID_Count;

            //Nodes
            if (loadData.nodes == null)
                loadData.nodes = new List<NodeData>();
            else
                loadData.nodes.Clear();

            loadData.connections = new List<ConnectionData>();

            //Variables
            if (loadData.variables == null)
                loadData.variables = new List<Var>();
            else
                loadData.variables.Clear();

            //Pass In Params
            if (loadData.passInParams == null)
                loadData.passInParams = new List<Var>();
            else
                loadData.passInParams.Clear();

            //Entry Points
            if (loadData.entryPoints == null)
                loadData.entryPoints = new List<NodeData>();
            else
                loadData.nodes.Clear();

            //Witing starts here

            //Nodes
            foreach (NodeData node in current.nodes)
                loadData.nodes.Add(node);

            //Connections
            foreach (ConnectionData con in current.connections)
                loadData.connections.Add(con);

            //Variables
            foreach (Var v in current.variables)
                loadData.variables.Add(v);

            //Pass In Params
            foreach (Var v in current.passInParams)
                loadData.passInParams.Add(v);

            //Entry Points
            foreach (NodeData nodeData in current.entryPoints)
                loadData.entryPoints.Add(nodeData);

#if UNITY_EDITOR
            EditorUtility.SetDirty(loadData);
            AssetDatabase.Refresh();
#endif
        }

        else
            Interpreter.Instance.SaveBlueprint(current);
        
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

        if (current != null)
            current = null;

        //Need to make a copy of the loaded blueprintData, current will serve as this copy
        if (current == null)
            current = ScriptableObject.CreateInstance<BlueprintData>();

        current.ComponentName = loadData.ComponentName;
        current.ID_Count = loadData.ID_Count;
        current.nodes = new List<NodeData>();
        current.connections = new List<ConnectionData>();
        current.variables = new List<Var>();
        current.passInParams = new List<Var>();
        current.entryPoints = new List<NodeData>();
        //Load Nodes
        foreach (NodeData node in loadData.nodes)
            current.nodes.Add(node);

        //Load connections
        foreach (ConnectionData con in loadData.connections)
            current.connections.Add(con);

        //Ready variable display
        if (varDisplay.inputs == null)
            varDisplay.inputs = new List<string>();

        //Load Variables
        foreach (Var v in loadData.variables)
        {
            v.type = Interpreter.Instance.LoadVarType(v.strType, v.asmPath);

            //Add to VarDisplay
            if (v.name != null)
                if (v.name != "")
                    varDisplay.inputs.Add(v.input);

            current.variables.Add(v);
        }

        //if (current.passInParams == null)
        //{
        //    Debug.Log("Current Pass In Params is null allocating");
        //    current.passInParams = new List<Var>();
        //}

        //Load Pass In Params
        foreach (Var v in loadData.passInParams)
        {
            v.type = Interpreter.Instance.LoadVarType(v.strType, v.asmPath);
            current.passInParams.Add(v);
        }

        //Load Entry Points
        foreach (NodeData nodeData in loadData.entryPoints)
        {
            current.entryPoints.Add(nodeData);
        }

        //Do stuff

        //Construct nodes
        foreach (NodeData data in current.nodes)
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
        foreach (Node node in nodes)
        {
            if (node.nextID != -1)
            {
                foreach (Node node2 in nodes)
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
                foreach (Node node2 in nodes)
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

    //Drawable Functions
    public static bool Toggle(Rect rect, bool value)
    {
        if (value)
            value = GUI.Toggle(rect, value, "True");
        else
            value = GUI.Toggle(rect, value, "False");
        return value;
    }
  
    public static int IntField(Rect rect, int value)
    {
        string init = value.ToString();
        //init = GUI.TextField(rect, init);
        init = GUILayout.TextField(init);
        
        int result = 0;

        System.Int32.TryParse(init, out result);

        return result;
    }

    public static long LongField(Rect rect, long value)
    {
        string init = value.ToString();
        //init = GUI.TextField(rect, init);
        init = GUILayout.TextField(init);

        long result = 0;

        System.Int64.TryParse(init, out result);

        return result;
    }

    public static float FloatField(Rect rect, float value)
    {
        string init = value.ToString();
        //init = GUI.TextField(rect, init);

        init = GUILayout.TextField(init);

        float result = 0.0f;

        System.Single.TryParse(init, out result);

        return result;
    }

    public static double DoubleField(Rect rect, double value)
    {
        string init = value.ToString();
        //init = GUI.TextField(rect, init);
        init = GUILayout.TextField(init);

        double result = 0.0f;

        System.Double.TryParse(init, out result);

        return result;
    }

    public static Vector2 Vector2Field(Rect rect, Vector2 value)
    {
        //GUILayout.BeginArea(rect);
        //GUILayout.BeginHorizontal();

        string xStr = value.x.ToString();
        GUILayout.Label("X");
        xStr = GUILayout.TextField(xStr);

        GUILayout.Label("Y");
        string yStr = value.y.ToString();
        yStr = GUILayout.TextField(yStr);

        //GUILayout.EndHorizontal();
       
        //GUILayout.EndArea();

        float x = 0.0f;
        float y = 0.0f;

        System.Single.TryParse(xStr, out x);
        System.Single.TryParse(yStr, out y);

        return new Vector2(x, y);
    }

    public static Vector3 Vector3Field(Rect rect, Vector3 value)
    {
        //GUILayout.BeginArea(rect);
        //GUILayout.BeginHorizontal();

        string xStr = value.x.ToString();
        GUILayout.Label("X");
        xStr = GUILayout.TextField(xStr);

        GUILayout.Label("Y");
        string yStr = value.y.ToString();
        yStr = GUILayout.TextField(yStr);

        GUILayout.Label("Z");
        string zStr = value.z.ToString();
        zStr = GUILayout.TextField(zStr);

        //GUILayout.EndHorizontal();

        //GUILayout.EndArea();

        float x = 0.0f;
        float y = 0.0f;
        float z = 0.0f;

        System.Single.TryParse(xStr, out x);
        System.Single.TryParse(yStr, out y);
        System.Single.TryParse(zStr, out z);

        return new Vector3(x, y, z);
    }

    public static Vector4 Vector4Field(Rect rect, Vector4 value)
    {
        //GUILayout.BeginArea(rect);
        //GUILayout.BeginHorizontal();

        string xStr = value.x.ToString();
        GUILayout.Label("X");
        xStr = GUILayout.TextField(xStr);

        GUILayout.Label("Y");
        string yStr = value.y.ToString();
        yStr = GUILayout.TextField(yStr);

        GUILayout.Label("Z");
        string zStr = value.z.ToString();
        zStr = GUILayout.TextField(zStr);

        GUILayout.Label("W");
        string wStr = value.w.ToString();
        wStr = GUILayout.TextField(wStr);

        //GUILayout.EndHorizontal();

        //GUILayout.EndArea();

        float x = 0.0f;
        float y = 0.0f;
        float z = 0.0f;
        float w = 0.0f;

        System.Single.TryParse(xStr, out x);
        System.Single.TryParse(yStr, out y);
        System.Single.TryParse(zStr, out z);
        System.Single.TryParse(wStr, out w);

        return new Vector4(x, y, z, w);
    }

    public static Quaternion QuaternionField(Rect rect, Quaternion value)
    {
        //GUILayout.BeginArea(rect);
        //GUILayout.BeginHorizontal();

        string xStr = value.x.ToString();
        GUILayout.Label("X");
        xStr = GUILayout.TextField(xStr);

        GUILayout.Label("Y");
        string yStr = value.y.ToString();
        yStr = GUILayout.TextField(yStr);

        GUILayout.Label("Z");
        string zStr = value.z.ToString();
        zStr = GUILayout.TextField(zStr);

        GUILayout.Label("W");
        string wStr = value.w.ToString();
        wStr = GUILayout.TextField(wStr);

        //GUILayout.EndHorizontal();

        //GUILayout.EndArea();

        float x = 0.0f;
        float y = 0.0f;
        float z = 0.0f;
        float w = 0.0f;

        System.Single.TryParse(xStr, out x);
        System.Single.TryParse(yStr, out y);
        System.Single.TryParse(zStr, out z);
        System.Single.TryParse(wStr, out w);

        return new Quaternion(x, y, z, w);
    }

    public static Rect RectField(Rect rect, Rect value)
    {
        //GUILayout.BeginArea(rect);
        //GUILayout.BeginHorizontal();

        string xStr = value.x.ToString();
        GUILayout.Label("X");
        xStr = GUILayout.TextField(xStr);

        GUILayout.Label("Y");
        string yStr = value.y.ToString();
        yStr = GUILayout.TextField(yStr);

        GUILayout.Label("W");
        string zStr = value.width.ToString();
        zStr = GUILayout.TextField(zStr);

        GUILayout.Label("H");
        string wStr = value.height.ToString();
        wStr = GUILayout.TextField(wStr);

        //GUILayout.EndHorizontal();

        //GUILayout.EndArea();

        float x = 0.0f;
        float y = 0.0f;
        float width = 0.0f;
        float height = 0.0f;

        System.Single.TryParse(xStr, out x);
        System.Single.TryParse(yStr, out y);
        System.Single.TryParse(zStr, out width);
        System.Single.TryParse(wStr, out height);

        return new Rect(x, y, width, height);
    }

    public static Color ColorField(Rect rect, Color value)
    {
        //GUILayout.BeginArea(rect);
        //GUILayout.BeginHorizontal();

        string rStr = value.r.ToString();
        GUILayout.Label("R");
        rStr = GUILayout.TextField(rStr);

        GUILayout.Label("G");
        string gStr = value.g.ToString();
        gStr = GUILayout.TextField(gStr);

        GUILayout.Label("B");
        string bStr = value.b.ToString();
        bStr = GUILayout.TextField(bStr);

        GUILayout.Label("A");
        string aStr = value.a.ToString();

        //GUILayout.EndHorizontal();

        //GUILayout.EndArea();

        float r = 0.0f;
        float g = 0.0f;
        float b = 0.0f;
        float a = 0.0f;
       
        System.Single.TryParse(rStr, out r);
        System.Single.TryParse(gStr, out g);
        System.Single.TryParse(bStr, out b);
        System.Single.TryParse(aStr, out a);

        if (r > 1.0f) r = 1.0f;
        if (g > 1.0f) g = 1.0f;
        if (b > 1.0f) b = 1.0f;
        if (a > 1.0f) a = 1.0f;

        if (r < 0.0f) r = 0.0f;
        if (g < 0.0f) g = 0.0f;
        if (b < 0.0f) b = 0.0f;
        if (a < 0.0f) a = 0.0f;

        return new Color(r, g, b, a);
    }

    //RECT REALLY NEEDED
    //public static System.Enum EnumPopup(Rect rect, System.Enum value, float maxWidth)
    public static void EnumPopup(Rect rect, Parameter value, float maxWidth)
    {
        string init = value.arg.ToString();
        if (GUILayout.Button(init, GUILayout.MaxWidth(maxWidth)))
        {            
            string[] names = System.Enum.GetNames(value.arg.GetType());
            string newName = value.arg.ToString();

            enumMenu.Clear();

            foreach (string str in names)
            {                
                enumMenu.AddItem(str, () => { value.arg = (System.Enum)System.Enum.Parse(value.arg.GetType(), str); });
            }
            
            enumMenu.rect.position = new Vector2(rect.x, rect.y + rect.height);
            enumMenu.canDraw = true;                               
        }        
    }

    //Source http://wiki.unity3d.com/index.php?title=DrawLine

    public static Texture2D lineTex;

    public static void DrawLine(Rect rect) { DrawLine(rect, GUI.contentColor, 1.0f); }
    public static void DrawLine(Rect rect, Color color) { DrawLine(rect, color, 1.0f); }
    public static void DrawLine(Rect rect, float width) { DrawLine(rect, GUI.contentColor, width); }
    public static void DrawLine(Rect rect, Color color, float width) { DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.x + rect.width, rect.y + rect.height), color, width); }
    public static void DrawLine(Vector2 pointA, Vector2 pointB) { DrawLine(pointA, pointB, GUI.contentColor, 1.0f); }
    public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color) { DrawLine(pointA, pointB, color, 1.0f); }
    public static void DrawLine(Vector2 pointA, Vector2 pointB, float width) { DrawLine(pointA, pointB, GUI.contentColor, width); }
    public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
    {
        if (pointA == pointB)
            return;

        // Save the current GUI matrix, since we're going to make changes to it.
        Matrix4x4 matrix = GUI.matrix;

        // Generate a single pixel texture if it doesn't exist
        if (!lineTex) { lineTex = new Texture2D(1, 1); }

        // Store current GUI color, so we can switch it back later,
        // and set the GUI color to the color parameter
        Color savedColor = GUI.color;
        GUI.color = color;

        // Determine the angle of the line.
        float angle = Vector3.Angle(pointB - pointA, Vector2.right);

        // Vector3.Angle always returns a positive number.
        // If pointB is above pointA, then angle needs to be negative.
        if (pointA.y > pointB.y) { angle = -angle; }

        // Use ScaleAroundPivot to adjust the size of the line.
        // We could do this when we draw the texture, but by scaling it here we can use
        //  non-integer values for the width and length (such as sub 1 pixel widths).
        // Note that the pivot point is at +.5 from pointA.y, this is so that the width of the line
        //  is centered on the origin at pointA.                
        GUIUtility.ScaleAroundPivot(new Vector2((pointB - pointA).magnitude, width), new Vector2(pointA.x, pointA.y + 0.5f));

        // Set the rotation for the line.
        //  The angle was calculated with pointA as the origin.
        GUIUtility.RotateAroundPivot(angle, pointA);

        // Finally, draw the actual line.
        // We're really only drawing a 1x1 texture from pointA.
        // The matrix operations done with ScaleAroundPivot and RotateAroundPivot will make this
        //  render with the proper width, length, and angle.
        GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1, 1), lineTex);

        // We're done.  Restore the GUI matrix and GUI color to whatever they were before.
        GUI.matrix = matrix;
        GUI.color = savedColor;
    }
}
