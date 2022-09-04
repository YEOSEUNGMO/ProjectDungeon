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
    /// 노드 그리기
    /// </summary>
    public void Draw(GUIStyle _nodeStyle)
    {
        //BeginArea를 이용해 노드 박스 그리기.
        GUILayout.BeginArea(rect, _nodeStyle);

        EditorGUI.BeginChangeCheck();

        //부모 노드가 있거나 노드 타입이 entrance 일 경우, 라벨만 표시하기.
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypeToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];

            //현재 노드의 선택된 방 타입의 타입에 따라 연결된 자식,부모 노드 연결 삭제.
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
    /// 노드 이벤트
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

        //노드 선택 토글.
        isSelected = !isSelected;
    }
    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    /// <summary>
    /// 노드 드래그(위치 이동)
    /// </summary>
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// 노드에 자식 ID 추가하기.
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

        //노드 그래프 내에서 보스 방과 연결된 방이 있는지 유무 확인.
        foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            if(roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count>0)
            {
                isConnectedBossNodeAlready = true;
            }
        }

        //내 자식 노드가 보스 방이고 보스 방이 이미 연결이 되어있을경우.
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
            return false;

        //자식 노드의 타입이 None 일 경우.
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
            return false;

        //내 자식 노드에 이미 포함되어있을 경우.
        if (childRoomNodeIDList.Contains(childID))
            return false;

        //내 ID 와 자식 노드 ID 가 같을 경우
        if (id == childID)
            return false;

        //이미 나의 부모 노드일 경우.
        if (parentRoomNodeIDList.Contains(childID))
            return false;

        //자식노드에 이미 부모가 있을 경우.
        if (roomNodeGraph.GetRoomNode(childID).parentRoomNodeIDList.Count > 0)
            return false;

        //자식 노드의 타입과 나의 노드 타입이 둘다 Corridor 일 경우. (연결 될 대상 둘다 Corridor 이면 안됨)
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
            return false;

        //자식 노드의 입과 나의 노드 타입이 둘다 Corridor 가 아닐 경우. (연결 될 대상 둘다 Corridor 가 아니면 안됨)
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
            return false;

        //자식 노드의 타입이 Corridor 인데 나의 자식 노드 개수가 맥스일 경우.
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
            return false;

        //자식 노드의 타입이 Entrance 일 경우. Entrance는 항상 최상의 레벨의 부모여야 한다.
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
            return false;

        //자식 노드의 타입이 Corridor가 아닌데 나에게 이미 다른 자식 노드가 연결 되어있을 경우.
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
            return false;

        return true;
    }

    /// <summary>
    /// 부모 노드 추가
    /// </summary>
    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    /// <summary>
    /// 자식 노드 삭제
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
    /// 부모 노드 삭제
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
