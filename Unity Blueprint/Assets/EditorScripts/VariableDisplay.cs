using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VariableDisplay 
{
    Rect rect = new Rect(0.0f, 0.0f, 300.0f, 800.0f);
    Vector2 initFieldPos;
    Vector2 initFieldDim;
    public List<string> inputs;
    public GUIStyle style;
    public float offset = 15.0f;
    public InterpreterData metaData;
    float maxHeight;
    //public BlueprintData current;

    public VariableDisplay()
    {
        Vector2 lerp = new Vector2
        (
            Mathf.Lerp(rect.position.x, rect.position.x + rect.width, 0.1f), 
            Mathf.Lerp(rect.position.y, rect.position.y + rect.height, 0.05f)
        );

        initFieldPos = lerp - rect.position;
        initFieldDim = new Vector2(rect.width * 0.75f, rect.height * 0.0275f);

        inputs = new List<string>();        
        maxHeight = Screen.height;       
    }

    public Vector2 Update(Vector2 scrollPos)
    {
        //scrollPos = GUI.BeginScrollView(new Rect(0.0f, 0.0f, Screen.width * 0.18f, Screen.height), scrollPos, new Rect(0.0f, 0.0f, Screen.width * 0.2f, maxHeight), true, true);
        //GUILayout.BeginArea(new Rect(Screen.width * 0.1f, Screen.height * 0.1f, 500.0f, Screen.height));
        //scrollPos = GUILayout.BeginScrollView(scrollPos);
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.MaxWidth(300.0f), GUILayout.MaxHeight(Screen.height));

        //GUI.Box(rect, "", style);

        //Vector2 pos = Vector2.zero + initFieldPos - scrollPos;
        //Rect entry = new Rect(pos, initFieldDim);
        //
        for (int i = 0; i < inputs.Count; i++)
        {
            //if (i > 0)
            //    pos.y += initFieldDim.y + offset;
            //entry.position = pos;
        
            GUI.SetNextControlName(i.ToString());
            //inputs[i] = EditorGUI.TextField(entry, inputs[i]);
            inputs[i] = GUILayout.TextField(inputs[i]);
            
            if (Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == i.ToString())
            {
                Debug.Log("Call reflection here for variables");
                ProcessContextMenu(inputs[i], Rect.zero);
            }
        }
        //
        ////Add an input if button is pressed
        //if (GUI.Button(new Rect(entry.x, entry.y + initFieldDim.y + offset, entry.width, entry.height), "+"))
        //{
        //    inputs.Add("");
        //}
        //maxHeight = entry.y + (initFieldDim.y * 4.0f) + offset;

        if (GUILayout.Button("Add Variable", GUILayout.MaxWidth(300.0f)))
        {
            inputs.Add("");
        }



        GUILayout.EndScrollView();
        //GUILayout.EndArea();
        //GUI.EndScrollView();
        return scrollPos;       
    }

    public void ProcessContextMenu(string input, Rect rect)
    {
#if UNITY_EDITOR

        if (!Application.isPlaying)
        {
            GenericMenu menu = new GenericMenu();

            Interpreter.Instance.CreateVariable(NodeEditor.current, ref metaData, input);

            if (metaData != null && metaData.selectedType == null)
            {
                foreach (System.Type type in metaData.types)
                {
                    menu.AddItem(new GUIContent(type.ToString()), true, SetObjectType, type);
                }

                menu.DropDown(rect);
            }
        }
#endif
        if (Application.isPlaying)
        {
            Interpreter.Instance.CreateVariable(RealTimeEditor.Instance.current, ref metaData, input);

            if (metaData != null && metaData.selectedType == null)
            {
                foreach (System.Type type in metaData.types)
                {
                    RealTimeEditor.Instance.contextMenu.AddItem(type.ToString(), SetObjectType, type);
                }

                RealTimeEditor.Instance.contextMenu.canDraw = true;
            }

        }

    }

    public void SetObjectType(object input)
    {
        metaData.selectedType = (System.Type)input;
        metaData.selectedAsm = metaData.selectedType.Assembly;
#if UNITY_EDITOR
        if (!Application.isPlaying)
            Interpreter.Instance.CreateVariable(NodeEditor.current, ref metaData, metaData.input);
#endif
        if (Application.isPlaying)
            Interpreter.Instance.CreateVariable(RealTimeEditor.Instance.current, ref metaData, metaData.input);
    }

}

/* 
 * Useful stuff for reference
   if (EditorGUIUtility.editingTextField)
   {
       if (GUI.GetNameOfFocusedControl() == null)
            Debug.Log("Focus control name null");

       Debug.Log($"editing field {GUI.GetNameOfFocusedControl()}");
   }   
*/
