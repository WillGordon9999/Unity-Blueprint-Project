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

    [MenuItem("Window/NodeEditor")]
    static void OpenWindow()
    {
        NodeEditor window = GetWindow<NodeEditor>();
        window.titleContent = new GUIContent("Node Editor");
    }

    private void OnEnable()
    {
        resize = new GUIStyle();
        //resize.normal.background = EditorGUIUtility.Load("icons/SomeTexture.png") as Texture2D;
    }

    private void OnGUI()
    {
        DrawResizer();
        ProcessEvents(Event.current);

        if (GUI.changed)
            Repaint();
    }

    //Still a little uncertain what this is exactly doing
    void DrawResizer()
    {
        resizeRect = new Rect(0, (position.height * sizeRatio) - 5.0f, position.width, 10.0f);
        GUILayout.BeginArea(new Rect(resizeRect.position + (Vector2.up * 5.0f), new Vector2(position.width, 2.0f)), resize);
        GUILayout.EndArea();
    }

    void ProcessEvents(Event e)
    {
        switch(e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0 && resizeRect.Contains(e.mousePosition))
                    isResizing = true;
                break;

            case EventType.MouseUp:
                isResizing = false;
                break;
        }

        Resize(e);
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
