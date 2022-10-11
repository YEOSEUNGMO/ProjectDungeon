using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class InstantiatedRoom : MonoBehaviour
{
    [HideInInspector] public Room room;
    [HideInInspector] public Grid grid;
    [HideInInspector] public Tilemap groundTilemap;
    [HideInInspector] public Tilemap decoration1Tilemap;
    [HideInInspector] public Tilemap decoration2Tilemap;
    [HideInInspector] public Tilemap frontTilemap;
    [HideInInspector] public Tilemap collisionTilemap;
    [HideInInspector] public Tilemap minimapTilemap;
    [HideInInspector] public Bounds roomColliderBounds;

    private BoxCollider2D boxCollider2D;

    void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();

        //Save room collider bounds
        roomColliderBounds = boxCollider2D.bounds;
    }

    /// <summary>
    /// Initialise the Instantiated Room
    /// </summary>
    public void Initialise(GameObject roomGameojbect)
    {
        PopulateTilemapMemberVariables(roomGameojbect);

        DisableCollisionTilemapRenderer();
    }

    /// <summary>
    /// Populate the tilemap and grid member variables.
    /// </summary>
    private void PopulateTilemapMemberVariables(GameObject roomGameobject)
    {
        //Get the grid componet.
        grid = roomGameobject.GetComponentInChildren<Grid>();

        //Get tilemaps in children.
        Tilemap[] tilemaps = roomGameobject.GetComponentsInChildren<Tilemap>();

        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap.gameObject.CompareTag("goundTilemap"))
            {
                groundTilemap = tilemap;
            }
            else if (tilemap.gameObject.CompareTag("decoration1Tilemap"))
            {
                decoration1Tilemap = tilemap;
            }
            else if (tilemap.gameObject.CompareTag("decoration2Tilemap"))
            {
                decoration2Tilemap = tilemap;
            }
            else if (tilemap.gameObject.CompareTag("frontTilemap"))
            {
                frontTilemap = tilemap;
            }
            else if (tilemap.gameObject.CompareTag("collisionTilemap"))
            {
                collisionTilemap = tilemap;
            }
            else if (tilemap.gameObject.CompareTag("minimapTilemap"))
            {
                minimapTilemap = tilemap;
            }
        }
    }

    /// <summary>
    /// Disable collision tilemap renderer
    /// </summary>
    private void DisableCollisionTilemapRenderer()
    {
        collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled=false;
    }
}