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

        // Jump variables:
        Vector2Int jumpFrom;
        Vector2Int jumpTo;
        float jumpAnimationSpeed = 6342f;
        float steps = 9999;
        float nSteps = 2000;
        float paraHeight = 0.387f;

        private void Start()
        {

        }

        private void Update()
        {
            if (steps < nSteps)
            {
                float xzDist = Vector2.Distance(new Vector2(jumpFrom.x, jumpFrom.y), new Vector2(jumpTo.x, jumpTo.y));
                Vector2 curXZ = jumpFrom;
                float curY = 0f;

                curXZ = Vector2.Lerp(curXZ, jumpTo, steps / (float)nSteps);
                curY = -paraHeight * (Vector2.Distance(curXZ,jumpFrom)) * (Vector2.Distance(curXZ,jumpTo)) / (-0.25f * xzDist * xzDist);

                transform.position = new Vector3(curXZ.x + 0.5f, curY, curXZ.y + 0.5f);

                steps += Time.deltaTime * jumpAnimationSpeed;
            }
        }

        public void Move(Vector2Int to)
        {
            if (Position != to)
            {
                jumpFrom = Position;
                jumpTo = to;
                steps = 0;

                Position = to;
            }
        }

        public void AIUpdate(GameManager gameManager)
        {
            // TODO: make proper AI

            var newPosition = Position + new Vector2Int(Random.Range(-1, 2), Random.Range(-1, 2));

            if (gameManager.CurrentFloor.IsWalkableFrom(Position, newPosition))
            {
                Move(newPosition);
                //Position = newPosition;
            }
        }
    }
}