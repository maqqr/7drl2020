using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public LevelGenerator.Scripts.LevelGenerator levelGeneratorPrefab;
    public GameObject debugSphere;

    Vector2Int TileCoordinateUnderMouse()
    {
        // TODO: raycast from camera
        // RaycastHit hit;
         Vector3 objectHit = new Vector3(0,0,0);
        // Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // if (Physics.Raycast(ray, out hit)) {
        //     objectHit = hit.point;
        //     
        //     // Do something with the object that was hit by the raycast.
        // }

        Plane plane = new Plane(Vector3.up,0f);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float dToPlane;
        if (plane.Raycast(ray, out dToPlane)) {
            objectHit = ray.GetPoint(dToPlane);
        }
        return new Vector2Int(castCoord(objectHit.x), castCoord(objectHit.z));
    }

    private int castCoord(float coord) {
        return coord< 0 ? ((int)coord) - 1 : (int)coord;
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

        debugSphere.transform.position = unityCoordinate;

        Gizmos.DrawWireSphere(unityCoordinate, 0.5f);
    }
}
