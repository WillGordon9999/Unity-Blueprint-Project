using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu
{
    class Item
    {
        public string text;
        public System.Action<object> func;
        public object data;

        public Item(string str, System.Action<object> function, object arg)
        {
            text = str;
            func = function;
            data = arg;
        }
    }

    List<Item> items;
    static Vector2 size = new Vector2(200, 200);
    Vector2 scrollPos;
    bool canDraw = false;
    Rect rect;

    public Menu()
    {
        items = new List<Item>();
    }

    public void AddItem(string text, System.Action<object> func, object data)
    {
        items.Add(new Item(text, func, data));
    }

    public void ProcessEvents(Event e)
    {
        if (e.button == 1 && !canDraw)
        {
            rect = new Rect(e.mousePosition, size);
            canDraw = true;
        }

        if (e.button == 0 && canDraw)
        {
            if (!rect.Contains(e.mousePosition))
                canDraw = false;
        }
    }

    public void DrawMenu()
    {
        if (!canDraw)
            return;

        GUILayout.BeginArea(rect);
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        foreach(Item item in items)
        {
            if (GUILayout.Button(item.text))
            {
                item.func(item.data);
                break;
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

}

//Could be a component of player or just singleton access
public class RealTimeEditor : MonoBehaviour
{
    static Rect enumSelect;
    static Rect defaultSize = new Rect(0, 0, 200, 20);
    static Vector2 enumScroll;    
    float zoomScale = 1.0f;
    const float minZoom = 1.0f;
    const float maxZoom = 10.0f;
    Menu contextMenu;

    bool isRunning = false;
    public int test = 0;
    public float test2 = 0.0f;
    public double test3 = 0.0;
    public Vector3 vec = Vector3.zero;
    public RigidbodyConstraints rbConst = RigidbodyConstraints.FreezePosition;
    // Start is called before the first frame update
    void Start()
    {        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isRunning = !isRunning;
            Time.timeScale = 1.0f - Time.timeScale;
        }

        if (isRunning && enumSelect == Rect.zero)
        {
            zoomScale += Input.GetAxis("Mouse ScrollWheel");
            
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

        //test = IntField(new Rect(500, 500, 300, 20), test);
        
        //vec = Vector3Field(new Rect(500 * zoomScale, 200 * zoomScale, 200 * zoomScale, 20 * zoomScale), vec);
        DrawLine(new Vector2(500, 200) * zoomScale, new Vector2(500, 500) * zoomScale, Color.green);
        DrawLine(new Vector2(700, 300) * zoomScale, new Vector2(800, 500) * zoomScale, Color.red);
        rbConst = (RigidbodyConstraints)EnumPopup(new Rect(500 * zoomScale, 200 * zoomScale, 200 * zoomScale, 20 * zoomScale), rbConst);

    }

    void ProcessEvents(Event e)
    {
        switch(e.type)
        {
            case EventType.MouseDown:
                if (e.button == 1)
                    ProcessContextMenu();
                break;
        }
    }

    void ProcessContextMenu()
    {
        //Menu menu = new Menu();
        //menu.AddItem("Testing", Test, "hi");
        //menu.DrawMenu();
    }

    public void Test(object input)
    {
        print("Test delegate with " + input.ToString());
    }
  
    public static int IntField(Rect rect, int value)
    {
        string init = value.ToString();
        init = GUI.TextField(rect, init);

        int result = 0;

        System.Int32.TryParse(init, out result);

        return result;
    }

    public static float FloatField(Rect rect, float value)
    {
        string init = value.ToString();
        init = GUI.TextField(rect, init);

        float result = 0.0f;

        System.Single.TryParse(init, out result);

        return result;
    }

    public static double DoubleField(Rect rect, double value)
    {
        string init = value.ToString();
        init = GUI.TextField(rect, init);

        double result = 0.0f;

        System.Double.TryParse(init, out result);

        return result;
    }

    public static Vector2 Vector2Field(Rect rect, Vector2 value)
    {
        GUILayout.BeginArea(rect);
        GUILayout.BeginHorizontal();

        string xStr = value.x.ToString();
        GUILayout.Label("X");
        xStr = GUILayout.TextField(xStr);

        GUILayout.Label("Y");
        string yStr = value.y.ToString();
        yStr = GUILayout.TextField(yStr);

        GUILayout.EndHorizontal();
       
        GUILayout.EndArea();

        float x = 0.0f;
        float y = 0.0f;

        System.Single.TryParse(xStr, out x);
        System.Single.TryParse(yStr, out y);

        return new Vector2(x, y);
    }

    public static Vector3 Vector3Field(Rect rect, Vector3 value)
    {
        GUILayout.BeginArea(rect);
        GUILayout.BeginHorizontal();

        string xStr = value.x.ToString();
        GUILayout.Label("X");
        xStr = GUILayout.TextField(xStr);

        GUILayout.Label("Y");
        string yStr = value.y.ToString();
        yStr = GUILayout.TextField(yStr);

        GUILayout.Label("Z");
        string zStr = value.z.ToString();
        zStr = GUILayout.TextField(zStr);

        GUILayout.EndHorizontal();

        GUILayout.EndArea();

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
        GUILayout.BeginArea(rect);
        GUILayout.BeginHorizontal();

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

        GUILayout.EndHorizontal();

        GUILayout.EndArea();

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

    public static System.Enum EnumPopup(Rect rect, System.Enum value)
    {
        string init = value.ToString();
        
        if (GUI.Button(rect, init) || enumSelect == rect)
        {          
            string[] names = System.Enum.GetNames(value.GetType());
            string newName = value.ToString();

            GUILayout.BeginArea(new Rect(rect.x, rect.y + defaultSize.height, rect.width, defaultSize.height * 10));
            enumScroll = GUILayout.BeginScrollView(enumScroll);
            
            foreach(string str in names)
            {
                if (GUILayout.Button(str))
                {
                    newName = str;
                    enumSelect = Rect.zero;
                    //To be safe
                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();
                    return (System.Enum)System.Enum.Parse(value.GetType(), newName);                    
                }
            }
            
            GUILayout.EndScrollView();            
            GUILayout.EndArea();
            enumSelect = rect;            
        }
        
        return value;
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
