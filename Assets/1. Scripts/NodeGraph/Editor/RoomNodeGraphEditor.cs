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
    /// ��� �׷��� �̺�Ʈ
    /// </summary>
    private void ProcessEvents(Event currentEvent)
    {
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        //��忡 ���콺 ���� ���� �ʾ��� ���.
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
    /// ��� ���콺 ���� üũ.
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
    /// ��� �׷��� ���콺 �ٿ� �̺�Ʈ.
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        //������ ���콺 Ŭ�� �̺�Ʈ
        if(currentEvent.button==1)
        {
            ShowContextMenu(currentEvent.mousePosition);
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
    }

    /// <summary>
    /// ������ �����쿡 ��� �׸���.
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
