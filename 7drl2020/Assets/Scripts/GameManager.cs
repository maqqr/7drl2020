using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public LevelGenerator.Scripts.LevelGenerator levelGeneratorPrefab;

    Vector2Int TileCoordinateUnderMouse()
    {
        // TODO: raycast from camera

        return new Vector2Int(0, 0);
    }

    void Start()
    {
        // This does not work yet.
        //levelGeneratorPrefab.GenerateLevel();
    }

    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        Vector2Int tile = TileCoordinateUnderMouse();

        Vector3 unityCoordinate = new Vector3(tile.x + 0.5f, 0.0f, tile.y + 0.5f);

        Gizmos.DrawWireSphere(unityCoordinate, 0.5f);
    }
}
