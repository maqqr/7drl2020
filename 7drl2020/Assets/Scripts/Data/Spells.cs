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
        public void Push(Creature creature, Vector2 dir, float dist,string dmg) {
            Vector2 targetTile = creature.Position + dir*dist;
            RaycastHit hit;
            bool collided = Physics.Raycast(Utils.ConvertToUnityCoord(creature.Position),new Vector3(dir.x,0,dir.y),out hit,dist);
            int hitdmg = Utils.RollDice(dmg,true);
            creature.Hp -= hitdmg;
            Debug.Log($"{creature.Data.Name} takes {hitdmg} damage from getting pushed.");
            for(float i = 0;i<=dist;i++) {
                Creature hitCreature = gameManager.CurrentFloor.GetCreatureAt(Utils.ConvertToTileCoord(creature.Position+dir.normalized*i));
                if (hitCreature != null) {
                    hitCreature.Hp -= hitdmg;
                    Debug.Log($"{hitCreature.Data.Name} takes {hitdmg} damage from the collision.");
                    creature.Move(Utils.ConvertToTileCoord(creature.Position+dir.normalized*(i-1)));
                    break;
                }
                if (collided && hit.distance<=i) {
                    Debug.Log("Push was obstructed");
                    creature.Move(Utils.ConvertToTileCoord(creature.Position+dir.normalized*(i-1)));
                    break;
                }
            }
        }
    }
}