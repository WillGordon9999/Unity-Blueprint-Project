using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextCompiler : MonoBehaviour
{
    public Font font;
    //public UnityEngine.UI.Text text;

    public int width = 600;
    public int height = 800;

    public Rect rect;

    GUIStyle style;
    string theText;
    string finalText;    
    Texture2D tex;
    string[] keywords = new string[]
        {
         "public",
         "private",
         "protected",
         "abstract",
         "virtual",
         "override",
         "const",
         "static",
         "delegate",
         "event",
         "var",
         "void",
         "bool",
         "char",
         "short",
         "int",
         "float",
         "long",
         "double",
         "uchar",
         "ushort",
         "uint",
         "ufloat",
         "ulong",
         "udouble",
         "string",
         "if",
         "else",
         "switch",
         "case",
         "goto",
         "break",
         "continue",
         "for",
         "foreach",
         "while",
         "yield",
         "return",
         "null",
         "default",
         "try",
         "catch",
         "throw",
         "new",
         "true",
         "false",
         "out",
         "ref",
         "enum",
         "class",
         "struct"
        };
    // Start is called before the first frame update
    void Start()
    {
        //text.supportRichText = true;
        //text.text = "Hello I am <color=green>green</color> text!";     
        style = new GUIStyle();
        style.richText = true;
        style.font = font;
        style.wordWrap = true;
        //rect = new Rect(Screen.width * 0.5f, Screen.height * 0.5f, width, height);
        
        theText = "";       
        tex = new Texture2D(width, height);        
    }

    private void OnValidate()
    {
        bool changed = false;

        if (width != rect.width)
        {
            width = (int)rect.width;
            changed = true;
        }

        if (height != rect.height)
        {
            height = (int)rect.height;
            changed = true;
        }

        if (changed)        
            tex = new Texture2D(width, height);        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {        
        GUI.DrawTexture(rect, tex);

        GUI.SetNextControlName("Original");
        theText = GUI.TextArea(rect, theText, style);                
        finalText = theText;

        foreach(string key in keywords)
        {
            if (theText.Contains(key))
                finalText = finalText.Replace(key, $"<color=blue>{key}</color>");
        }

        //if (Event.current.keyCode == KeyCode.Space)
        //{
        //    print("Space confirmed");
        //    
        //    //Apparently the isKey will stop numerous events from firing
        //    if (Event.current.isKey)
        //        print("space isKey");
        //
        //    //Key down does not work
        //    if (Event.current.type == EventType.KeyDown)
        //        print("space key down");
        //
        //    //Key up also seems to work appropriately
        //    if (Event.current.type == EventType.KeyUp)
        //        print("space key up");
        //
        //}


        GUI.TextArea(rect, finalText, style);

        GUI.FocusControl("Original");

    }
}
