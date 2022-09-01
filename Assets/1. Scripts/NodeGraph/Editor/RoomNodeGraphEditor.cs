using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;
    private RoomNodeSO currentRoomNode = null;
    private RoomNodeTypeListSO roomNodeTypeList;

    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;


    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph");
    }
    private void OnEnable()
    {
        // Define node layout style.
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // Load Room node types
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// Open the room node graph editor window if a room node graph scriptable object asset is double clikced in the inspector.
    /// </summary>
    /// <param name="instanceID"></param>
    /// <param name="line"></param>
    /// <returns></returns>

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
            // Process Events
            ProcessEvents(Event.current);

            // Draw Room Nodes
            DrawRoomNodes();

            if (GUI.changed)
                Repaint();
        }
    }
    /// <summary>
    /// 노드 그래프 이벤트
    /// </summary>
    private void ProcessEvents(Event currentEvent)
    {
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        //노드에 마우스 오버 되지 않았을 경우.
        if(currentRoomNode==null)
        {
            ProcessMouseDownEvent(currentEvent);
        }
        else
        {
            currentRoomNode.ProcessEvents(currentEvent);
        }

    }

    /// <summary>
    /// 노드 마우스 오버 체크.
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
    /// <summary>
    /// 노드 그래프 마우스 다운 이벤트.
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        //오른쪽 마우스 클릭 이벤트
        if(currentEvent.button==1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
    }
    /// <summary>
    /// Context Menu 열기
    /// </summary>
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu= new GenericMenu();
        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);

        menu.ShowAsContext();
    }
    /// <summary>
    /// 마우스 위치에 노드 생성.
    /// </summary>
    private void CreateRoomNode(object mousePositionObject)
    {
        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));
    }

    private void CreateRoomNode(object mousePositionObject,RoomNodeTypeSO roomNodeType)
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
    }

    /// <summary>
    /// 에디터 윈도우에 노드 그리기.
    /// </summary>
    private void DrawRoomNodes()
    {
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.Draw(roomNodeStyle);
        }

        GUI.changed = true;
    }
}
