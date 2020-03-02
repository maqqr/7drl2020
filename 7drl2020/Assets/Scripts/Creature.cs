using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator
{
    public class Creature : MonoBehaviour
    {
        [HideInInspector] public Data.CreatureData Data; // Static creature data shared by all creature of this type
        public bool InSync => true;
        public int TimeElapsed; // Creature is updated once TimeElapsed >= Data.Speed

        public int Speed => 1000;

        // Creature specific attributes:
        public Vector2Int Position; // Creature's position in tile coordinates
        // public int Hp;
        // List<InventoryItem> Inventory;

        private void Start()
        {

        }

        private void Update()
        {
            Vector3 target = Utils.ConvertToUnityCoord(Position);
            transform.position += (target - transform.position) * 10f * Time.deltaTime;
        }

        public void AIUpdate(GameManager gameManager)
        {
            // TODO: make proper AI

            var newPosition = Position + new Vector2Int(Random.Range(-1, 2), Random.Range(-1, 2));

            if (gameManager.CurrentFloor.IsWalkableFrom(Position, newPosition))
            {
                Position = newPosition;
            }
        }
    }
}