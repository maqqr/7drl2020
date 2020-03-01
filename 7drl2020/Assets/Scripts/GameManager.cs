using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Camera cam;
    public LevelGenerator.Scripts.LevelGenerator levelGeneratorPrefab;
    public GameObject debugSphere;

    Vector2Int TileCoordinateUnderMouse()
    {
        // TODO: raycast from camera
        RaycastHit hit;
        Vector3 objectHit = new Vector3(0,0,0);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit)) {
            objectHit = hit.point;
            
            // Do something with the object that was hit by the raycast.
        }
        return new Vector2Int((int)objectHit.x, (int)objectHit.z);
    }

    void Start()
    {
        cam = Camera.main;
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
        Debug.Log("X: " + tile.x + ", Y: " + tile.y);

        debugSphere.transform.position = unityCoordinate;

        Gizmos.DrawWireSphere(unityCoordinate, 0.5f);
    }
}
