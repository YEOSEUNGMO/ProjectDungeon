using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeType_", menuName = "Scriptable Objects/Dungeon/Room Node Type")]
public class RoomNodeTypeSO : MonoBehaviour
{
    public string roomNodeTypeName;

    #region Header
    [Header("Only flag the RoomNodeTypes that should be visible in the editor")]
    #endregion Header
    public bool displayInNodeGraphEdtior = true;
    #region Header
    [Header("One Type Should Be A Corridor")]
    #endregion Header
    public bool isCorridor;
    #region Header
    [Header("One Type Shold Be A CorridorNS")]
    #endregion Header
    public bool isCorridorNS;
    #region Header
    [Header("One Type Shold Be A CorridorEW")]
    #endregion Header
    public bool isCorridorEW;
    #region Header
    [Header("One Type Shold Be An Entrance")]
    #endregion Header
    public bool isEntrance;
    #region Header
    [Header("One Type Shold Be A Boss Room")]
    #endregion Header
    public bool isBossRoom;
    #region Header
    [Header("One Type Shold Be A Unassigned")]
    #endregion Header
    public bool isNone;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
#endif
    #endregion
}