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

    List<Node> nodes;
    List<Connection> connections;

    //General sizing variables - no more hard-coding
    float nodeWidth = 300.0f;
    float nodeHeight = 50.0f;
    Vector2 connectionLineTangent = Vector2.left * 50.0f;
    float connectionLineWidth = 2.0f;

    [MenuItem("Window/NodeEditor")]
    static void OpenWindow()
    {
        NodeEditor window = GetWindow<NodeEditor>();
        window.titleContent = new GUIContent("Node Editor");
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
        //DrawResizer();
        
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
        if (connections != null)
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
        menu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePos));
        menu.ShowAsContext();
    }

    void OnClickAddNode(Vector2 mousePos)
    {
        if (nodes == null)
        {
            nodes = new List<Node>();
        }
        
        nodes.Add(new Node(mousePos, nodeWidth, nodeHeight, nodeStyle, selectedNodeStyle, inStyle, outStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode));
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

    void OnClickRemoveConnection(Connection connection)
    {
        connections.Remove(connection);
    }

    void CreateConnection()
    {
        if (connections == null)
            connections = new List<Connection>();

        connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
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
