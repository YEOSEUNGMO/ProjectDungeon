using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id;
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    #region Editor Core
#if UNITY_EDITOR

    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging;
    [HideInInspector] public bool isSelected;

    public void Initialise(Rect _rect, RoomNodeGraphSO _nodeGraph, RoomNodeTypeSO _roomNodeType)
    {
        rect = _rect;
        id = Guid.NewGuid().ToString();
        name = "RoomNode";
        roomNodeGraph = _nodeGraph;
        roomNodeType = _roomNodeType;

        // Load room node type list
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// ��� �׸���
    /// </summary>
    public void Draw(GUIStyle _nodeStyle)
    {
        //BeginArea�� �̿��� ��� �ڽ� �׸���.
        GUILayout.BeginArea(rect, _nodeStyle);

        EditorGUI.BeginChangeCheck();

        //�θ� ��尡 �ְų� ��� Ÿ���� entrance �� ���, �󺧸� ǥ���ϱ�.
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypeToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];

            //���� ����� ���õ� �� Ÿ���� Ÿ�Կ� ���� ����� �ڽ�,�θ� ��� ���� ����.
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor || !roomNodeTypeList.list[selected].isCorridor &&
                roomNodeTypeList.list[selection].isCorridor || !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom)
            {
                if (childRoomNodeIDList.Count > 0)
                {
                    for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                    {
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                        if (childRoomNode != null)
                        {
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                            childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);
                        }
                    }
                }
            }
        }


        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);

        GUILayout.EndArea();
    }

    private string[] GetRoomNodeTypeToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];

        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEdtior)
            {
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }

        return roomArray;
    }

    /// <summary>
    /// ��� �̺�Ʈ
    /// </summary>
    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;

        DragNode(currentEvent.delta);
        GUI.changed = true;
    }


    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        //mouse left down
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }

        //mouse right down
        if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;

        //��� ���� ���.
        isSelected = !isSelected;
    }
    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    /// <summary>
    /// ��� �巡��(��ġ �̵�)
    /// </summary>
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// ��忡 �ڽ� ID �߰��ϱ�.
    /// </summary>
    /// <returns></returns>
    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }
        return false;
    }

    private bool IsChildRoomValid(string childID)
    {
        bool isConnectedBossNodeAlready = false;

        //��� �׷��� ������ ���� ��� ����� ���� �ִ��� ���� Ȯ��.
        foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            if(roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count>0)
            {
                isConnectedBossNodeAlready = true;
            }
        }

        //�� �ڽ� ��尡 ���� ���̰� ���� ���� �̹� ������ �Ǿ��������.
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
            return false;

        //�ڽ� ����� Ÿ���� None �� ���.
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
            return false;

        //�� �ڽ� ��忡 �̹� ���ԵǾ����� ���.
        if (childRoomNodeIDList.Contains(childID))
            return false;

        //�� ID �� �ڽ� ��� ID �� ���� ���
        if (id == childID)
            return false;

        //�̹� ���� �θ� ����� ���.
        if (parentRoomNodeIDList.Contains(childID))
            return false;

        //�ڽĳ�忡 �̹� �θ� ���� ���.
        if (roomNodeGraph.GetRoomNode(childID).parentRoomNodeIDList.Count > 0)
            return false;

        //�ڽ� ����� Ÿ�԰� ���� ��� Ÿ���� �Ѵ� Corridor �� ���. (���� �� ��� �Ѵ� Corridor �̸� �ȵ�)
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
            return false;

        //�ڽ� ����� �԰� ���� ��� Ÿ���� �Ѵ� Corridor �� �ƴ� ���. (���� �� ��� �Ѵ� Corridor �� �ƴϸ� �ȵ�)
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
            return false;

        //�ڽ� ����� Ÿ���� Corridor �ε� ���� �ڽ� ��� ������ �ƽ��� ���.
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
            return false;

        //�ڽ� ����� Ÿ���� Entrance �� ���. Entrance�� �׻� �ֻ��� ������ �θ𿩾� �Ѵ�.
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
            return false;

        //�ڽ� ����� Ÿ���� Corridor�� �ƴѵ� ������ �̹� �ٸ� �ڽ� ��尡 ���� �Ǿ����� ���.
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
            return false;

        return true;
    }

    /// <summary>
    /// �θ� ��� �߰�
    /// </summary>
    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    /// <summary>
    /// �ڽ� ��� ����
    /// </summary>
    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        if (childRoomNodeIDList.Contains(childID))
        {
            childRoomNodeIDList.Remove(childID);
            return true;
        }
        return false;
    }

    /// <summary>
    /// �θ� ��� ����
    /// </summary>
    public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)
    {
        if(parentRoomNodeIDList.Contains(parentID))
        {
            parentRoomNodeIDList.Remove(parentID);
            return true;
        }
        return false;
    }

#endif
    #endregion
}
