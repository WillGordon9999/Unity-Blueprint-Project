using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[Serializable]
public class ConnectionData
{
    public ConnectionPointData inPoint;
    public ConnectionPointData outPoint;

    public ConnectionData(ConnectionPointData inData, ConnectionPointData outData)
    {
        inPoint = inData;
        outPoint = outData;
    }
}

public class Connection 
{
    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;
    public Action<Connection> OnClickRemoveConnection;

    public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint, Action<Connection> OnClickRemoveConnection)
    {
        this.inPoint = inPoint;
        this.outPoint = outPoint;
        this.OnClickRemoveConnection = OnClickRemoveConnection;
    }

    public void Draw(float zoomScale = 1.0f)
    {
#if UNITY_EDITOR

        if (!Application.isPlaying)
        {
            Handles.DrawBezier(inPoint.rect.center * zoomScale, outPoint.rect.center * zoomScale, (inPoint.rect.center * zoomScale) + Vector2.left * 50.0f, (outPoint.rect.center * zoomScale) - Vector2.left * 50.0f, Color.white, null, 2.0f);

            //Original: (Handles.Button((inPoint.rect.center + outPoint.rect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleCap)
            if (Handles.Button(((inPoint.rect.center + outPoint.rect.center) * zoomScale) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
            {
                if (OnClickRemoveConnection != null)
                {
                    OnClickRemoveConnection.Invoke(this);
                }
            }
            return;
        }
#endif        
        //RealTimeEditor.DrawLine(inPoint.rect.center * zoomScale, outPoint.rect.center * zoomScale, Color.white, 2.0f);
        RealTimeEditor.DrawLine(inPoint.rect.center, outPoint.rect.center, Color.white, 2.0f);

        //Rect remove = new Rect((inPoint.rect.center * zoomScale + outPoint.rect.center * zoomScale) * 0.5f, new Vector2(25, 25));
        Rect remove = new Rect((inPoint.rect.center + outPoint.rect.center) * 0.5f, new Vector2(25, 25));
        remove.position -= new Vector2(remove.width * 0.5f, remove.height * 0.5f);

        if (GUI.Button(remove, ""))
        {
            if (OnClickRemoveConnection != null)
            {
                OnClickRemoveConnection.Invoke(this);
            }
        }
    }
}
