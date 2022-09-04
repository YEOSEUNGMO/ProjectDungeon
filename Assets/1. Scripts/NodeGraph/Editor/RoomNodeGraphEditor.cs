using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private GUIStyle roomNodeSelectedStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;
    private RoomNodeSO currentRoomNode = null;
    private RoomNodeTypeListSO roomNodeTypeList;

    //Node layout values
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    //Connecting line values
    private const float connectingLineWidth = 3f;
    private const float connectingLineArrowSize = 6f;
    


    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph");
    }
    private void OnEnable()
    {
        Selection.selectionChanged += InspectorSelectionChanged;
        // Define node layout style.
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // Define selecte node style.
        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        roomNodeSelectedStyle.normal.textColor = Color.white;
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // Load Room node types
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    /// <summary>
    /// Open the room node graph editor window if a room node graph scriptable object asset is double clikced in the inspector.
    /// </summary>

    [OnOpenAsset(0)]    //Need the namespace UnityEditor.Callbacks
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            OpenWindow();

            currentRoomNodeGraph = roomNodeGraph;

            return true;
        }
        return false;
    }

    /// <summary>
    /// Draw Editor Window
    /// </summary>
    private void OnGUI()
    {
        if(currentRoomNodeGraph!=null)
        {
            //���� �巡�� ���̶�� ���� �׸���
            DrawDraggedLine();

            // Process Events
            ProcessEvents(Event.current);

            DrawRoomConnections();

            // Draw Room Nodes
            DrawRoomNodes();

            if (GUI.changed)
                Repaint();
        }
    }

    private void DrawDraggedLine()
    {
        if(currentRoomNodeGraph.linePosition!=Vector2.zero)
        {
            //���� ���� ���� �׸���
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectingLineWidth);
        }
    }

    /// <summary>
    /// ��� �׷��� �̺�Ʈ
    /// </summary>
    private void ProcessEvents(Event currentEvent)
    {
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        //��忡 ���콺 ���� ���� �ʾ��� ���. �Ǵ� ��忡�� ������ �巡�� �ϰ��ִ� ���.
        if(currentRoomNode==null || currentRoomNodeGraph.roomNodeToDrawLineFrom!=null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        else
        {
            currentRoomNode.ProcessEvents(currentEvent);
        }

    }

    /// <summary>
    /// ��� ���콺 ���� üũ.
    /// ��ü ��� ����Ʈ�� Ž���Ͽ� ���� ���콺 ��ġ�� �� ������ Rect�� ���ԵǾ��ִ��� Ȯ��.
    /// </summary>
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for(int i=currentRoomNodeGraph.roomNodeList.Count-1;i>=0;i--)
        {
            if(currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }
        return null;
    }

    private void ProcessRoomNodeGraphEvents(Event currentEvet)
    {
        switch(currentEvet.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvet);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvet);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvet);
                break;
            default:
                break;
        }
    }

    private void ProcessMouseUpEvent(Event currentEvet)
    {
        //���� ������ �׸��� ���߿� ������ ���콺�� �������.
        if(currentEvet.button==1&&currentRoomNodeGraph.roomNodeToDrawLineFrom!=null)
        {
            //���� ��� ���� ���콺�� �ö��ִ� ���.
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvet);

            if(roomNode!=null)
            {
                if(currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }

            ClearLineDrag();
        }
    }

    private void ClearLineDrag()
    {
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    /// <summary>
    /// ���� ���� ���� ���� �׸���.
    /// </summary>
    private void DrawRoomConnections()
    {
        //��� ��� Ž��
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            //�ڽ� ������ �ִ��� Ȯ��.
            if(roomNode.childRoomNodeIDList.Count>0)
            {
                //��� �ڽ� ��� Ž��.
                foreach(string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    if(currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);

                        GUI.changed = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// �θ� ���� �ڽ� ��� ���� �� �׸���.
    /// </summary>
    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        Vector2 startPos = parentRoomNode.rect.center;
        Vector2 endPos = childRoomNode.rect.center;

        //�߰� ���� ���ϱ�.
        Vector2 midPos = (endPos + startPos) / 2;

        //���� ���� ����
        Vector2 dir = endPos - startPos;

        Vector2 arrowTailPoint1 = midPos - new Vector2(-dir.y, dir.x).normalized * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPos + new Vector2(-dir.y, dir.x).normalized * connectingLineArrowSize;

        Vector2 arrowHeadPoint = midPos + dir.normalized * connectingLineArrowSize;

        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectingLineWidth);

        Handles.DrawBezier(startPos, endPos, startPos, endPos, Color.white, null, connectingLineWidth);

        GUI.changed = true;
    }

    /// <summary>
    /// ���콺 �巡�� �̺�Ʈ
    /// </summary>
    private void ProcessMouseDragEvent(Event currentEvet)
    {
        //������ ���콺 �巡�� �̺�Ʈ - ���� �׸���.
        if(currentEvet.button==1)
        {
            ProcessRightMouseDragEvent(currentEvet);
        }
    }

    /// <summary>
    /// ������ ���콺 �巡�� �̺�Ʈ - ���� �׸���.
    /// </summary>
    private void ProcessRightMouseDragEvent(Event currentEvet)
    {
        if(currentRoomNodeGraph.roomNodeToDrawLineFrom!=null)
        {
            DragConnectingLine(currentEvet.delta);
            GUI.changed = true;
        }
    }

    private void DragConnectingLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    /// <summary>
    /// ��� �׷��� ���콺 �ٿ� �̺�Ʈ.
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        //������ ���콺 Ŭ�� �̺�Ʈ
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        else if (currentEvent.button == 0) 
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    /// <summary>
    /// Context Menu ����
    /// </summary>
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu= new GenericMenu();
        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);

        menu.ShowAsContext();
    }
    /// <summary>
    /// ���콺 ��ġ�� ��� ����.
    /// </summary>
    private void CreateRoomNode(object mousePositionObject)
    {
        if(currentRoomNodeGraph.roomNodeList.Count==0)
        {
            CreateRoomNode(new Vector2(200, 200), roomNodeTypeList.list.Find(x => x.isEntrance));
        }

        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));
    }

    private void CreateRoomNode(object mousePositionObject,RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        // ��ũ���ͺ� ������Ʈ ���� ��� ����.
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        //���� ��� �׷��� ����Ʈ�� ��� �߰�.
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        // ��� ���� �Է�.
        roomNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        //��� �׷��� ��ũ���ͺ� ������Ʈ�� ��� �߰�.
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);

        AssetDatabase.SaveAssets();

        //��ųʸ� ��������.
        currentRoomNodeGraph.OnValidate();
    }
    private void ClearAllSelectedRoomNodes()
    {
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if(roomNode.isSelected)
            {
                roomNode.isSelected = false;

                GUI.changed = true;
            }
        }
    }

    /// <summary>
    /// ������ �����쿡 ��� �׸���.
    /// </summary>
    private void DrawRoomNodes()
    {
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.Draw(roomNodeSelectedStyle);
            }
            else
            {
                roomNode.Draw(roomNodeStyle);
            }
        }

        GUI.changed = true;
    }

    private void InspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;
        
        if(roomNodeGraph != null)
        {
            currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }
}