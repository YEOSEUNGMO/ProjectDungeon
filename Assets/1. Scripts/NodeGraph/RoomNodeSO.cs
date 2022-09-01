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

        int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

        int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypeToDisplay());

        roomNodeType = roomNodeTypeList.list[selection];

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);

        GUILayout.EndArea();
    }

    private string[] GetRoomNodeTypeToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];

        for(int i=0;i<roomNodeTypeList.list.Count;i++)
        {
            if(roomNodeTypeList.list[i].displayInNodeGraphEdtior)
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
        switch(currentEvent.type)
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
        if(currentEvent.button==0)
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

    private void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if(currentEvent.button==0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    private void ProcessLeftClickUpEvent()
    {
        if(isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if(currentEvent.button==0)
        {
            ProcessLeftClickDownEvent();
        }
    }

    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;

        //노드 선택 토글.
        isSelected = !isSelected;
    }

#endif
    #endregion
}
