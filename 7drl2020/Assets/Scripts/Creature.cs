using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator
{
    public enum DamageType
    {
        Slashing,
        Blunt,
        Piercing,
        Magic
    }

    public class Creature : MonoBehaviour
    {
        [HideInInspector] public Data.CreatureData Data; // Static creature data shared by all creature of this type
        public bool InSync => true;
        public int TimeElapsed; // Creature is updated once TimeElapsed >= Data.Speed

        // Creature specific attributes:
        public Vector2Int Position; // Creature's position in tile coordinates
        // public int Hp;
        //List<Item> Inventory;

        // Jump variables:
        Vector2Int jumpFrom;
        Vector2Int jumpTo;
        float jumpAnimationSpeed = 6342f;
        float steps = 9999;
        float nSteps = 2000;
        float paraHeight = 0.387f;


        public string Name => "Nameless";

        public int Hp { get; set; }
        public int Mp { get; set; }
        public int MaxHp => Data.BaseMaxHp;
        public int MaxMp => Data.BaseMaxMp;
        public int Strength => Data.BaseStr;
        public int Intelligence => Data.BaseInt;
        public int MeleeSkill => Data.BaseMeleeSkill;
        public int RangedSkill => Data.BaseRangedSkill;
        public int AggroRange => Data.BaseAggroRange;

        public int Speed => 1000;

        public int GetResistance(DamageType damageType)
        {
            return 0;
        }

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

        public bool Move(Vector2Int to)
        {
            if (Position != to)
            {
                RaycastHit hit;
                // If moving diagonally, check if there exists a manhattan-like path
                if (Vector2.Distance(Position,to)>1){
                    bool success = false;
                    if (!Physics.Raycast(Utils.ConvertToUnityCoord(Position), Utils.ConvertToUnityCoord(new Vector2Int(to.x,Position.y)) - Utils.ConvertToUnityCoord(Position), out hit, 1f)) {
                        if (!Physics.Raycast(Utils.ConvertToUnityCoord(new Vector2Int(to.x,Position.y)), Utils.ConvertToUnityCoord(to) - Utils.ConvertToUnityCoord(new Vector2Int(to.x,Position.y)), out hit, 1f)) {
                            success = true;
                        }
                    }
                    if (!Physics.Raycast(Utils.ConvertToUnityCoord(Position), Utils.ConvertToUnityCoord(new Vector2Int(Position.x,to.y)) - Utils.ConvertToUnityCoord(Position), out hit, 1f)) {
                        if (!Physics.Raycast(Utils.ConvertToUnityCoord(new Vector2Int(Position.x,to.y)), Utils.ConvertToUnityCoord(to) - Utils.ConvertToUnityCoord(new Vector2Int(Position.x,to.y)), out hit, 1f)) {
                            success = true;
                        }
                    }
                    if (!success) return false;
                }
                
                
                jumpFrom = Position;
                jumpTo = to;
                steps = 0;

                Position = to;
                return true;
            }
            return false;
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

        public bool AddItem(Item item) {
            // TODO: check if inventory is full or if the creature can hold the item
            //Inventory.Add(item);
            return true;
        }
    }
}