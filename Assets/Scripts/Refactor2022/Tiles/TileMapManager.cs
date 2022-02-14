using BattleDelts.Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class TileMapWrap
{
    public Tilemap Tilemap;
    public TileBase this[int x, int y]
    {
        get
        {
            return Tilemap.GetTile(new Vector3Int(x, y, 0));
        }
        set
        {
            Tilemap.SetTile(new Vector3Int(x, y, 0), value);
        }
    }

    public bool TryGetTile(int x, int y, out TileBase tile)
    {
        tile = this[x, y];
        return !(tile is null);
    }
}

[Serializable]
public class SceneInteractables
{
    public SceneId SceneId;

    public List<InteractAction> Items;
    public List<InteractAction> Messages;
    public List<InteractAction> Quests;
    public List<InteractAction> Characters;
    public List<DoorAction> Doors;
}


public class TileMapManager : MonoBehaviour
{
    private enum GrassAnimationState
    {
        Untouched,
        SteppedOn,
        ComingUp
    }

    private enum GrassType
    {
        Green,
        Desert
    }

    public SceneInteractables CurrentSceneInteractables => SceneInteractables[CurrentScene];

    [SerializeField]
    private TileMapWrap ForegroundTiles;
    
    [SerializeField]
    private TileMapWrap Characters;
    
    [SerializeField]
    private TileMapWrap PlayerBlockingTiles;
    
    [SerializeField]
    private TileMapWrap TallGrass;
    
    [SerializeField]
    private TileMapWrap OverBackgroundTiles;
    
    [SerializeField]
    private TileMapWrap BackgroundTiles;

    [SerializeField]
    private List<SceneInteractables> Interactables;
    private readonly Dictionary<SceneId, SceneInteractables> SceneInteractables = new Dictionary<SceneId, SceneInteractables>();
    private Transform PlayerTransform;

    // TODO: Subscribe to scene ID changed event
    private SceneId CurrentScene = SceneId.DAGraveyard;

    private void Start()
    {
        foreach(var interactable in Interactables)
        {
            SceneInteractables.Add(interactable.SceneId, interactable);
        }

        PlayerTransform = PlayerMovement.PlayMov.playerSprite.transform;
    }

    public void OnTallGrassEnter()
    {
        var wildDeltSpawnId = GetCurrentWildDeltSpawn();
        if (!wildDeltSpawnId.HasValue)
        {
            Debug.LogError("Failed to find wild delt spawn ID for tile at " + PlayerTransform.position);
        }

        var grassType = GetGrassType(wildDeltSpawnId.Value);

        Debug.Log($"Stepped onto {wildDeltSpawnId} grass: {grassType}");
    }

    private WildDeltSpawnId? GetCurrentWildDeltSpawn()
    {
        var playerPos = PlayerTransform.position;
        foreach (var spawn in GameManager.GameMan.Data.DeltSpawns.Values)
        {
            foreach (var section in spawn.Bounds)
            {
                if (section.xMin <= playerPos.x && section.xMax >= playerPos.x &&
                    section.yMin <= playerPos.y && section.yMax >= playerPos.y)
                {
                    return spawn.WildDeltSpawnId;
                }
            }
        }

        return null;
    }

    private GrassType GetGrassType(WildDeltSpawnId id)
    {
        // TODO: Add desert types when they are introduced
        return GrassType.Green;
    }
}
