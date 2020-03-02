using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ConnectionPointType { In, Out }

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
        rect = new Rect(0.0f, 0.0f, 10.0f, 20.0f);
    }

    public ConnectionPoint(ConnectionPointData data, GUIStyle connectStyle, Action<ConnectionPoint> clickConnection)
    {
        rect = data.rect;        
        type = data.type;
        style = connectStyle;
        OnClickConnectionPoint = clickConnection;
    }


    public void Draw()
    {
        rect.y = node.rect.y + (node.rect.height * 0.5f) - rect.height * 0.5f;

        switch (type)
        {
            case ConnectionPointType.In:
                rect.x = node.rect.x - rect.width + 8.0f;
                break;

            case ConnectionPointType.Out:
                rect.x = node.rect.x + node.rect.width - 8.0f;
                break;
        }

        if (GUI.Button(rect, "", style))
        {
            if (OnClickConnectionPoint != null)
            {
                //OnClickConnectionPoint(this);
                OnClickConnectionPoint.Invoke(this);
            }
        }
    }

}
