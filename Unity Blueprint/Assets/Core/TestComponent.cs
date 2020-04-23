//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System;
//using System.Reflection;
////using UnityEditor;
////
//public class TestComponent : MonoBehaviour
//{
//    System.Type type;
//        
//    // Start is called before the first frame update
//    void Start()
//    {
//        var lambdaSetter = new Action<GameObject, object>((o, m) => o.active = (bool)m);                        
//    }
//
//    // Update is called once per frame
//    void Update()
//    {
//        
//    }
//
//    //Testing to see what happens when you change parameters
//    //The result is that it's no longer a message, it becomes a local definition function
//    private void OnCollisionEnter(Collision collision)
//    {
//        
//    }
//
//    private void OnGUI()
//    {
//        //GUI.SetNextControlName("Float");        
//        //result = GUI.TextField(new Rect(Screen.width / 2, Screen.height / 2, 300, 100), RealTimeWindow.result.ToString());
//        //EditorGUI.FocusTextInControl("Float");
//        //toolbarInt = GUI.Toolbar(new Rect(25, 25, 250, 30), toolbarInt, new string[] { "Option 1", "Option 2", "Option 3", "Option 4"});
//    }
//}

//public class RealTimeWindow : EditorWindow
//{
//    public static RealTimeWindow Instance;
//    public static float result;
//
//    public static void OpenWindow()
//    {
//        Debug.Log("Instantiating window");
//        Instance = GetWindow<RealTimeWindow>();
//        Instance.titleContent = new GUIContent("Test");
//    }
//
//    private void OnGUI()
//    {
//        Debug.Log("In OnGUI for window");
//        GUI.SetNextControlName("Test");
//        result = EditorGUI.FloatField(new Rect(Screen.width / 2, Screen.height / 2, 300, 100), result);
//        GUI.FocusControl("Test");
//    }
//
//    private void Update()
//    {       
//    }
//}