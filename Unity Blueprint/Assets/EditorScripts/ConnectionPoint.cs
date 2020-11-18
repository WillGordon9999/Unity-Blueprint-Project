using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ConnectionPointType { In, Out, False }

[Serializable]
public class ConnectionPointData
{
    public Rect rect;
    public ConnectionPointType type;
    public int enumVal; //backup
    public NodeData node;

    public ConnectionPointData(Rect rectangle, ConnectionPointType pointType, NodeData data)
    {
        rect = rectangle;
        type = pointType;
        enumVal = (int)pointType;
        node = data;
    }

    public ConnectionPointData(ConnectionPoint point, NodeData data)
    {
        rect = point.rect;
        type = point.type;
        node = data;
    }
}


public class ConnectionPoint 
{
    public Rect rect;
    public ConnectionPointType type;
    public Node node;
    public GUIStyle style;
    public Action<ConnectionPoint> OnClickConnectionPoint;

    public ConnectionPoint(Node node, ConnectionPointType type, GUIStyle style, Action<ConnectionPoint> OnClickConnectionPoint)
    {
        this.node = node;
        this.type = type;
        this.style = style;
        this.OnClickConnectionPoint = OnClickConnectionPoint;

        if (!Application.isPlaying)
            rect = new Rect(0.0f, 0.0f, 10.0f, 20.0f);
        else
            rect = new Rect(0.0f, 0.0f, 30.0f, 20.0f);
    }

    public ConnectionPoint(ConnectionPointData data, Node target, GUIStyle connectStyle, Action<ConnectionPoint> clickConnection)
    {
        node = target;
        rect = data.rect;        
        type = data.type;
        style = connectStyle;
        OnClickConnectionPoint = clickConnection;
    }

    public void Draw(float zoomScale = 1.0f)
    {
        //rect.y = node.rect.y + (node.rect.height * 0.5f) - rect.height * 0.5f;
        rect.y = (node.rect.y * zoomScale) + (node.rect.height * zoomScale * 0.5f) - rect.height * zoomScale * 0.5f;
        //rect.y = (node.rect.y * zoomScale) + ((node.rect.height * 0.5f) * zoomScale) - (rect.height * 0.5f) * zoomScale;

        switch (type)
        {
            case ConnectionPointType.In:
                //rect.x = node.rect.x - rect.width + 8.0f;
                rect.x = (node.rect.x * zoomScale) - (rect.width * zoomScale) + 8.0f;
                //rect.x = (node.rect.x * zoomScale) - (rect.width + 8.0f) * zoomScale;
                break;

            case ConnectionPointType.Out:
                //rect.x = node.rect.x + node.rect.width - 8.0f;
                rect.x = (node.rect.x * zoomScale) + (node.rect.width * zoomScale) - 8.0f;
                //rect.x = (node.rect.x * zoomScale) + (node.rect.width - 8.0f) * zoomScale;
                break;

            case ConnectionPointType.False:
                //rect.x = node.rect.x + node.rect.width - 8.0f;
                rect.x = (node.rect.x * zoomScale) + (node.rect.width * zoomScale) - 8.0f;

                rect.y = (node.rect.y * zoomScale) + 70.0f;
                //rect.y = (node.rect.y += 70.0f) * zoomScale;
                //rect.y = node.rect.y + 70.0f;
                break;
        }

        //Rect final = new Rect(rect.position * zoomScale, rect.size * zoomScale);

        if (!Application.isPlaying)
        {
            if (GUI.Button(rect, "", style))
            {
                if (OnClickConnectionPoint != null)
                {
                    //OnClickConnectionPoint(this);
                    OnClickConnectionPoint.Invoke(this);
                }
            }
        }

        else
        {
            if (GUI.Button(rect, ""))
            {
                if (OnClickConnectionPoint != null)
                {
                    //OnClickConnectionPoint(this);
                    OnClickConnectionPoint.Invoke(this);
                }
            }
        }
    }

}
