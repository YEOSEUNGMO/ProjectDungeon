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
#endif
    #endregion
}
