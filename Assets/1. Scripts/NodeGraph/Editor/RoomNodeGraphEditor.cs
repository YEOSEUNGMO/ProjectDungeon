using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private GUIStyle roomNodeSelectedStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;

    private Vector2 graphOffset;
    private Vector2 graphDrag;

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

    //Grid Spacing
    private const float gridLarge = 100f;
    private const float gridSmall = 25f;



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
        if (currentRoomNodeGraph != null)
        {
            //그리드 그리기.
            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);
            //현재 드래그 중이라면 라인 그리기
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

    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize); // CeilToInt : int형으로 올림.
        int horizontalLineCount= Mathf.CeilToInt((position.height+ gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        graphOffset += graphDrag * 0.5f;

        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);
        //세로선 그리기.
        for(int i=0;i<verticalLineCount;i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset, new Vector3(gridSize * i, position.height + gridSize, 0f) + gridOffset);
        }

        //가로선 그리기
        for(int i=0;i<horizontalLineCount;i++)
        {
            Handles.DrawLine(new Vector3(-gridSize, gridSize * i, 0) + gridOffset, new Vector3(position.width + gridSize, gridSize * i, 0f) + gridOffset);
        }

        Handles.color = Color.white;
    }

    private void DrawDraggedLine()
    {
        if (currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            //노드로 부터 라인 그리기
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectingLineWidth);
        }
    }

    /// <summary>
    /// 노드 그래프 이벤트
    /// </summary>
    private void ProcessEvents(Event currentEvent)
    {
        //그래프 드래그 초기화.
        graphDrag = Vector2.zero;
        
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        //노드에 마우스 오버 되지 않았을 경우. 또는 노드에서 라인을 드래그 하고있는 경우.
        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        else
        {
            currentRoomNode.ProcessEvents(currentEvent);
        }

    }

    /// <summary>
    /// 노드 마우스 오버 체크.
    /// 전체 노드 리스트를 탐색하여 현재 마우스 위치가 각 노드들의 Rect에 포함되어있는지 확인.
    /// </summary>
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for (int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }
        return null;
    }

    private void ProcessRoomNodeGraphEvents(Event currentEvet)
    {
        switch (currentEvet.type)
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
        //현재 라인을 그리는 도중에 오른쪽 마우스를 뗏을경우.
        if (currentEvet.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            //현재 노드 위에 마우스가 올라가있는 경우.
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvet);

            if (roomNode != null)
            {
                if (currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
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
    /// 노드들 사이 연결 상태 그리기.
    /// </summary>
    private void DrawRoomConnections()
    {
        //모든 노드 탐색
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            //자식 노드들이 있는지 확인.
            if (roomNode.childRoomNodeIDList.Count > 0)
            {
                //모든 자식 노드 탐색.
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    if (currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);

                        GUI.changed = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 부모 노드와 자식 노드 사이 선 그리기.
    /// </summary>
    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        Vector2 startPos = parentRoomNode.rect.center;
        Vector2 endPos = childRoomNode.rect.center;

        //중간 지점 구하기.
        Vector2 midPos = (endPos + startPos) / 2;

        //연결 방향 벡터
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
    /// 마우스 드래그 이벤트
    /// </summary>
    private void ProcessMouseDragEvent(Event currentEvet)
    {
        //오른쪽 마우스 드래그 이벤트 - 라인 그리기.
        if (currentEvet.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvet);
        }
        //왼쪽 마우스 드래그 이벤트 - 그래프 드래그하기.
        if (currentEvet.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvet.delta);
        }

    }

    /// <summary>
    /// 왼쪽 마우스 드래그 - 노드 그래프 드래그
    /// </summary>
    private void ProcessLeftMouseDragEvent(Vector2 dragDelta)
    {
        graphDrag = dragDelta;

        //모든 노드 드래그.
        for(int i=0;i<currentRoomNodeGraph.roomNodeList.Count;i++)
        {
            currentRoomNodeGraph.roomNodeList[i].DragNode(dragDelta);
        }

        GUI.changed=true;
    }

    /// <summary>
    /// 오른쪽 마우스 드래그 이벤트 - 라인 그리기.
    /// </summary>
    private void ProcessRightMouseDragEvent(Event currentEvet)
    {
        if (currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
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
    /// 노드 그래프 마우스 다운 이벤트.
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        //오른쪽 마우스 클릭 이벤트
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
    /// Context Menu 열기
    /// </summary>
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select All Room Nodes"), false, SelectAllRoomNods);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Selected Room Node Links"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("Delete Selected Room Node"), false, DeleteSelectedRoomNodes);

        menu.ShowAsContext();
    }
    /// <summary>
    /// 마우스 위치에 노드 생성.
    /// </summary>
    private void CreateRoomNode(object mousePositionObject)
    {
        if (currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200, 200), roomNodeTypeList.list.Find(x => x.isEntrance));
        }

        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));
    }

    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        // 스크립터블 오브젝트 에셋 노드 생성.
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        //현재 노드 그래프 리스트에 노드 추가.
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        // 노드 정보 입력.
        roomNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        //노드 그래프 스크립터블 오브젝트에 노드 추가.
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);

        AssetDatabase.SaveAssets();

        //딕셔너리 리프레시.
        currentRoomNodeGraph.OnValidate();
    }

    /// <summary>
    /// 선택된 노드 삭제.
    /// </summary>
    private void DeleteSelectedRoomNodes()
    {
        Queue<RoomNodeSO> roomNodeDeletionQueue = new Queue<RoomNodeSO>();

        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            //선택된 노드 찾기.
            if(roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                roomNodeDeletionQueue.Enqueue(roomNode);

                //선택된 노드의 자식 노드 검색.
                foreach(string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRoomNodeID);

                    if(childRoomNode!=null)
                    {
                        //자식 노드와 연결 끊기.
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

                //선택된 노드의 부모 노드 검색
                foreach(string parentRoomNodeID in roomNode.parentRoomNodeIDList)
                {
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);

                    if (parentRoomNode != null)
                    {
                        //부모 노드와 연결 끊기.
                        parentRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        while(roomNodeDeletionQueue.Count > 0)
        {
            //삭제 할 노드 꺼내기.
            RoomNodeSO roomNodeToDelete = roomNodeDeletionQueue.Dequeue();

            //노드 딕셔너리에서 데이터 삭제.
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);

            //노드 리스트에서 데이터 삭제.
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);

            //Asset database에서 삭제.
            DestroyImmediate(roomNodeToDelete, true);

            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>
    /// 선택된 노드 간의 연결 끊기.
    /// </summary>
    private void DeleteSelectedRoomNodeLinks()
    {
        //모든 노드 탐색
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            //해당 노드가 선택되어있고 연결된 자식 노드가 있을경우.
            if (roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for (int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(roomNode.childRoomNodeIDList[i]);

                    //자식 노드도 선태된 상태라면.
                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        //선택된 현재 노드의 자식 노드 지우기
                        roomNode.RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                        //자식 노드의 부모 노드(선택된 노드) 지우기.
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        //모든 선택 취소.
        ClearAllSelectedRoomNodes();
    }

    private void ClearAllSelectedRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;

                GUI.changed = true;
            }
        }
    }

    private void SelectAllRoomNods()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = true;
        }
        GUI.changed = true;
    }

    /// <summary>
    /// 에디터 윈도우에 노드 그리기.
    /// </summary>
    private void DrawRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
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

        if (roomNodeGraph != null)
        {
            currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }
}