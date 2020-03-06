using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator
{
    public class Spells {

        private GameManager gameManager;

        public void setGameManager(GameManager gameManager) {
            this.gameManager = gameManager;
        }
        public void Push(Creature caster ,Creature creature,string dmg, float dist = 3) {
            // Get the direction where the creature is pushed to
            Vector2 dir = creature.Position-caster.Position;
            dir = dir.normalized;
            Vector2 targetTile = creature.Position + dir*dist;
            Debug.DrawRay(Utils.ConvertToUnityCoord(creature.Position),new Vector3(dir.x,0,dir.y),UnityEngine.Color.red,2f);
            RaycastHit hit;
            // Check if the pushed creature hit a wall
            bool collided = Physics.Raycast(Utils.ConvertToUnityCoord(creature.Position),new Vector3(dir.x,0,dir.y),out hit,dist);
            int hitdmg = Utils.RollDice(dmg,true);
            creature.Hp -= hitdmg;
            Debug.Log($"{creature.Data.Name} takes {hitdmg} damage from getting pushed.");
            // Creatures don't block raycasts. Check if a creature was hit.
            for(float i = 0;i<=dist;i++) {
                Creature hitCreature = gameManager.CurrentFloor.GetCreatureAt(Utils.ConvertToTileCoord(creature.Position+dir*i));
                if (hitCreature != null) {
                    hitCreature.Hp -= hitdmg;
                    Debug.Log($"{hitCreature.Data.Name} takes {hitdmg} damage from the collision.");
                    Debug.Log($"{creature.Position+dir*(i-1)}");
                    Vector2 newPos = creature.Position + dir * (i - 1);
                    creature.Move(Utils.ConvertToTileCoord(new Vector3(newPos.x, 0f, newPos.y)));
                    return;
                }
                if (collided && hit.distance<=i) {
                    Debug.Log("Push was obstructed " + i);
                    Vector2 newPos = creature.Position + dir * (i - 1);
                    creature.Move(Utils.ConvertToTileCoord(new Vector3(newPos.x, 0f, newPos.y)));
                    Debug.Log($"{creature.Position+dir*(i-1)}");
                    return;
                }
            }
            creature.Move(Utils.ConvertToTileCoord(new Vector3(targetTile.x, 0f, targetTile.y)));
        }

        public bool Cast(string effect, Creature caster, Creature target, string dmg) {
            switch(effect) {
                case "Push": {
                    Debug.Log("Casting Push");
                    Push(caster,target,dmg);
                    return true;
                }
                default: {
                    Debug.Log("No casting");
                    return false;
                }
            }
        }
    }
}