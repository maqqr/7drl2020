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

            var eff = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Effect/Push"));
            eff.transform.position = Utils.ConvertToUnityCoord(caster.Position) + new Vector3(0f, 0.6f, 0f);
            eff.transform.rotation = Quaternion.LookRotation(creature.transform.position - caster.transform.position);

            // Get the direction where the creature is pushed to
            Vector2 dir = creature.Position-caster.Position;
            dir = dir.normalized;
            Vector2 targetTile = creature.Position + dir*dist;
            Debug.DrawRay(Utils.ConvertToUnityCoord(creature.Position),new Vector3(dir.x,0,dir.y)*dist,UnityEngine.Color.red,2f);
            RaycastHit hit;
            // Check if the pushed creature hit a wall
            bool collided = Physics.Raycast(Utils.ConvertToUnityCoord(creature.Position),new Vector3(dir.x,0,dir.y),out hit,dist);
            int hitdmg = Utils.RollDice(dmg,true);
            creature.Hp -= hitdmg;
            creature.Stun +=1;
            gameManager.MessageBuffer.AddMessage(Color.white,$"{creature.Data.Name} takes {hitdmg} damage from getting pushed.");
            // Creatures don't block raycasts. Check if a creature was hit.
            for(float i = 0;i<=dist;i++) {
                Creature hitCreature = gameManager.CurrentFloor.GetCreatureAt(Utils.ConvertToTileCoord(creature.Position+dir*i));
                if (hitCreature != null) {
                    hitCreature.Hp -= hitdmg;
                    hitCreature.Stun +=1;
                    gameManager.MessageBuffer.AddMessage(Color.white,$"{hitCreature.Data.Name} takes {hitdmg} damage from the collision.");
                    Vector2 newPos = creature.Position + dir * (i - 1);
                    creature.Move(Utils.ConvertToTileCoord(new Vector3(newPos.x, 0f, newPos.y)));
                    return;
                }
                if (collided && hit.distance<=i) {
                    Debug.Log("Push was obstructed " + i);
                    Vector2 newPos = creature.Position + dir * (i - 1);
                    creature.Move(Utils.ConvertToTileCoord(new Vector3(newPos.x, 0f, newPos.y)));
                    return;
                }
            }
            creature.Move(Utils.ConvertToTileCoord(new Vector3(targetTile.x, 0f, targetTile.y)));
        }

        public void Firesquare (Creature target, string dmg, int aoedmg = 3) {
            int hitdmg = Utils.RollDice(dmg,true);
            target.Hp -= hitdmg;
            gameManager.MessageBuffer.AddMessage(Color.white,$"{target.Data.Name} takes {hitdmg} damage from the fire.");
            for (int i = -1;i<=1;i++) {
                for (int j =-1;j<=1;j++) {
                    try {
                         Creature hitCreature = gameManager.CurrentFloor.GetCreatureAt(new Vector2Int(target.Position.x+i,target.Position.y+j));
                         hitCreature.Hp -= aoedmg;
                         gameManager.MessageBuffer.AddMessage(Color.white,$"{hitCreature.Data.Name} takes {aoedmg} damage from the fire.");

                         var eff = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Effect/Firesquare"));
                         eff.transform.position = Utils.ConvertToUnityCoord(target.Position + new Vector2Int(i, j));
                    }
                    catch {
                        continue;
                    }
                }
            }
        }

        public void LeechLife (Creature target, Creature caster, string dmg) {
            int hitdmg = Utils.RollDice(dmg,true);
            int heal = Mathf.Min(target.Hp,hitdmg / 2);
            target.Hp -= hitdmg;
            caster.Hp += heal;
        }

        public void PoisonBeam (Creature target, Creature caster, string dmg) {
            int roll = Utils.RollDice(dmg);
            List<Vector2Int> path = Utils.Line(caster.Position,target.Position);
            foreach (Vector2Int tile in path) {
                try {
                    Creature hitCreature = gameManager.CurrentFloor.GetCreatureAt(tile);
                    if (hitCreature != null) {
                        hitCreature.Poison += roll;
                        gameManager.MessageBuffer.AddMessage(Color.white,$"{hitCreature.Data.Name} was hit by poison.");
                    }
                }
                catch {
                    continue;
                }
            }
        }

        public bool Cast(string effect, Creature caster, Creature target, string dmg) {
            switch(effect) {
                case "Push": {
                    Debug.Log("Casting Push");
                    Push(caster,target,dmg);
                    return true;
                }
                case "Firesquare" : {
                    Firesquare(target,dmg);
                    return true;
                }
                case "Leechlife": {
                    LeechLife(target,caster,dmg);
                    return true;
                }
                case "Poisonbeam" :{
                    PoisonBeam(target,caster,dmg);
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