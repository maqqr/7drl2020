﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Verminator
{
    public enum DamageType
    {
        Slashing,
        Blunt,
        Piercing,
        Magic
    }

    public enum ArmorSlot
    {
        Head,
        Body,
        Legs
    }

    public class Creature : MonoBehaviour
    {
        [HideInInspector] public Data.CreatureData Data; // Static creature data shared by all creature of this type
        public bool InSync => true;
        public int TimeElapsed; // Creature is updated once TimeElapsed >= Speed

        public Vector2Int Position; // Creature's position in tile coordinates
        public List<InventoryItem> Inventory = new List<InventoryItem>();

        public Data.TraitData CurrentTrait = null;
        public List<Data.TraitData> Mutations = new List<Data.TraitData>();

        public InventoryItem[] EquipSlots = new InventoryItem[3]; // These are refereces to items in inventory

        public Dictionary<ArmorSlot, InventoryItem> ArmorSlots = new Dictionary<ArmorSlot, InventoryItem>() { { ArmorSlot.Head, null }, { ArmorSlot.Body, null }, { ArmorSlot.Legs, null } };

        public Quaternion targetRotation;

        public GameObject ModelGameObject;

        // Jump variables:
        Vector2Int jumpFrom;
        Vector2Int jumpTo;
        float jumpAnimationSpeed = 6342f;
        float steps = 9999;
        float nSteps = 2000;
        float paraHeight = 0.387f;

        public bool OnMove = false;
        public bool Attacking = false;
        private int hp;
        public int BaseHpBonus = 0;
        private int mp;

        public System.Action OnMovementAnimationEnd;

        public string Name => (CurrentTrait != null ? CurrentTrait.Name + " " : "") + Data.Name;

        public int Hp { get => hp; set => hp = Mathf.Clamp(value, 0, MaxHp); }
        public int Mp { get => mp; set => mp = Mathf.Clamp(value, 0, MaxMp); }
        public int MaxHp => BaseHpBonus + Data.BaseMaxHp + (CurrentTrait != null ? CurrentTrait.MaxHpBonus : 0) + Mutations.Select(m => m.MaxHpBonus).Sum();
        public int MaxMp => Data.BaseMaxMp;
        public int Strength => Data.BaseStr + (CurrentTrait != null ? CurrentTrait.StrBonus : 0) + Mutations.Select(m => m.StrBonus).Sum();
        public int Intelligence => Data.BaseInt;
        public int MeleeSkill => Data.BaseMeleeSkill + (CurrentTrait != null ? CurrentTrait.MeleeBonus : 0) + Mutations.Select(m => m.MeleeBonus).Sum();
        public int RangedSkill => Data.BaseRangedSkill + (CurrentTrait != null ? CurrentTrait.RangedBonus : 0) + Mutations.Select(m => m.RangedBonus).Sum();
        public int AggroRange => Data.BaseAggroRange + (CurrentTrait != null ? CurrentTrait.AggroRangeModifier : 0) + Mutations.Select(m => m.AggroRangeModifier).Sum();

        public int Speed => Data.BaseSpeed;

        public int Stun = 0;

        public int Poison = 0;

        public bool IsVisible { get; private set; }

        // Get back 1 mp every 3 turns
        public int ManaCounter = 0;
        public int RecoverTime = 2;

        public int weaponOfChoice = 0;

        public string Desc
        {
            get
            {
                string msg = "";
                int playerSpeed = 100; // Player speed is assumed to be 100 always

                msg += Utils.FixFont($"HP: {Hp} / {MaxHp}  Str: {Strength} Melee: {MeleeSkill}\n");
                if (Speed > playerSpeed) msg += "It is faster than you are.\n\n";
                if (Speed < playerSpeed) msg += "It is slower than you are.\n\n";
                if (Speed == playerSpeed) msg += "Its speed matches yours.\n\n";
                msg += Utils.FixFont($"Resistances:\n");
                msg += Utils.FixFont($"Slashing: {GetResistance(DamageType.Slashing)}%, Blunt: {GetResistance(DamageType.Blunt)}%\n");
                msg += Utils.FixFont($"Piercing: {GetResistance(DamageType.Piercing)}%, Magic: {GetResistance(DamageType.Magic)}%\n");

                msg += "\n";

                if (CurrentTrait != null)
                {
                    msg += "Trait: " + CurrentTrait.Name + "\n - " + CurrentTrait.Description + "\n\n";
                }
                foreach (var mutation in Mutations)
                {
                    msg += mutation.Name + "\n - " + mutation.Description + "\n";
                }
                return msg;
            }
        }

        public int GetResistance(DamageType damageType)
        {
            int armorBonus = 0;
            foreach (var slot in ArmorSlots)
            {
                if (slot.Value != null)
                {
                    armorBonus += slot.Value.ItemData.GetResistance(damageType);
                }
            }

            int traitBonus = 0;
            if (CurrentTrait != null) traitBonus += CurrentTrait.GetResistanceBonus(damageType);
            foreach (var mut in Mutations)
            {
                traitBonus += mut.GetResistanceBonus(damageType);
            }
            return Data.GetBaseResistance(damageType) + traitBonus + armorBonus;
        }

        public void SetVisibility(bool visible)
        {
            IsVisible = visible;
            ModelGameObject.SetActive(visible);
        }

        private void Start()
        {

        }

        private void Update()
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 8f * Time.deltaTime);

            if (steps < nSteps)
            {
                if (steps / nSteps >= 0.5 && Attacking)
                {
                    jumpFrom = jumpTo;
                    jumpTo = Position;
                    Attacking = false;
                }
                float xzDist = Vector2.Distance(new Vector2(jumpFrom.x, jumpFrom.y), new Vector2(jumpTo.x, jumpTo.y));
                Vector2 curXZ = jumpFrom;
                float curY = 0f;

                curXZ = Vector2.Lerp(curXZ, jumpTo, steps / (float)nSteps);
                if (xzDist != 0)
                {
                    curY = -paraHeight * (Vector2.Distance(curXZ, jumpFrom)) * (Vector2.Distance(curXZ, jumpTo)) / (-0.25f * xzDist * xzDist);
                }
                else
                {
                    curY = 0.0f;
                }

                transform.position = new Vector3(curXZ.x + 0.5f, curY, curXZ.y + 0.5f);

                steps += Time.deltaTime * jumpAnimationSpeed;

                if (steps >= nSteps)
                {
                    OnMovementAnimationEnd?.Invoke();
                }
            }
            else
            {
                OnMove = false;
            }
        }

        public bool Move(Vector2Int to, bool attackMove = false)
        {
            if (Position != to)
            {
                RaycastHit hit;
                // If moving diagonally, check if there exists a manhattan-like path
                if (Vector2.Distance(Position, to) > 1)
                {
                    bool success = false;
                    if (!Physics.Raycast(Utils.ConvertToUnityCoord(Position), Utils.ConvertToUnityCoord(new Vector2Int(to.x, Position.y)) - Utils.ConvertToUnityCoord(Position), out hit, 1f))
                    {
                        if (!Physics.Raycast(Utils.ConvertToUnityCoord(new Vector2Int(to.x, Position.y)), Utils.ConvertToUnityCoord(to) - Utils.ConvertToUnityCoord(new Vector2Int(to.x, Position.y)), out hit, 1f))
                        {
                            success = true;
                        }
                    }
                    if (!Physics.Raycast(Utils.ConvertToUnityCoord(Position), Utils.ConvertToUnityCoord(new Vector2Int(Position.x, to.y)) - Utils.ConvertToUnityCoord(Position), out hit, 1f))
                    {
                        if (!Physics.Raycast(Utils.ConvertToUnityCoord(new Vector2Int(Position.x, to.y)), Utils.ConvertToUnityCoord(to) - Utils.ConvertToUnityCoord(new Vector2Int(Position.x, to.y)), out hit, 1f))
                        {
                            success = true;
                        }
                    }
                    if (!success) return false;
                }

                targetRotation = Quaternion.LookRotation(Utils.ConvertToUnityCoord(to) - Utils.ConvertToUnityCoord(Position), Vector3.up);

                jumpFrom = Position;
                jumpTo = to;
                steps = 0;

                if (!attackMove) Position = to;
                OnMove = true;

                return true;
            }
            return false;
        }

        public void AIUpdate(GameManager gameManager)
        {
            // TODO: make proper AI

            if (this.Stun > 0)
            {
                gameManager.MessageBuffer.AddMessage(UnityEngine.Color.yellow, $"{this.Data.Name} is stunned for {this.Stun} turns.");
                this.Stun -= 1;
                return;
            }
            if (this.Poison > 0)
            {
                this.hp -= this.Poison;
                gameManager.MessageBuffer.AddMessage(UnityEngine.Color.green, $"{this.Data.Name} takes {this.Poison} damage poison.");
                this.Poison--;
            }
            if (this.mp < this.MaxMp)
            {
                if (this.ManaCounter == this.RecoverTime)
                {
                    this.mp += 1;
                    this.ManaCounter = 0;
                }
                else
                {
                    this.ManaCounter++;
                }
            }
            Vector2Int newPosition;
            var rayStart = Utils.ConvertToUnityCoord(this.Position) + new Vector3(0f, 0.5f, 0f);
            var rayEnd = Utils.ConvertToUnityCoord(gameManager.PlayerCreature.Position) + new Vector3(0f, 0.5f, 0f);
            var rayDir = rayEnd - rayStart;
            var rayDist = rayDir.magnitude;
            rayDir = rayDir.normalized;

            if (Vector3.Distance(rayStart, rayEnd) <= AggroRange)
                Debug.DrawRay(rayStart, rayDir * rayDist, Color.magenta, 1f);

            if (Vector3.Distance(rayStart, rayEnd) <= AggroRange && !Physics.Raycast(rayStart, rayDir, rayDist))
            {
                // Use ranged attacks if at disposal
                weaponOfChoice = Random.Range(0, 3);
                int dist = Mathf.FloorToInt(Vector2Int.Distance(this.Position, gameManager.PlayerCreature.Position));
                if (dist <= this.EquipSlots[weaponOfChoice].ItemData.MaxRange && dist >= this.EquipSlots[weaponOfChoice].ItemData.MinRange)
                {
                    FightPlayer(gameManager);
                    return;
                }
                else
                {
                    newPosition = gameManager.CurrentFloor.FindPath(this.Position, gameManager.PlayerCreature.Position)[0];
                }
            }
            else
            {
                newPosition = Position + new Vector2Int(Random.Range(-1, 2), Random.Range(-1, 2));
            }


            if (gameManager.CurrentFloor.IsWalkableFrom(Position, newPosition))
            {
                Creature creatureBlocking = gameManager.CurrentFloor.GetCreatureAt(newPosition);
                if (creatureBlocking == null)
                {
                    Move(newPosition);
                }
                else if (creatureBlocking == gameManager.PlayerCreature)
                {
                    Attacking = true;
                    FightPlayer(gameManager);
                    Move(newPosition, true);
                }

                //Position = newPosition;
            }
        }

        private void FightPlayer(GameManager gameManager)
        {
            Attacking = true;
            Move(gameManager.PlayerCreature.Position, true);

            gameManager.Fight(this, gameManager.PlayerCreature);

            if (Vector2.Distance(Position, gameManager.PlayerCreature.Position) < 2f)
            {
                var check = new List<Verminator.Data.TraitData>();
                if (CurrentTrait != null) check.Add(CurrentTrait);
                foreach (var mut in Mutations) check.Add(mut);

                foreach (var trait in check)
                {
                    foreach (var weaponId in trait.ExtraWeapons)
                    {
                        Verminator.Data.ItemData traitWeapon = Verminator.Data.GameData.Instance.ItemData[weaponId];
                        gameManager.Fight(this, gameManager.PlayerCreature, traitWeapon);
                    }
                }
            }

        }

        public bool AddItem(Data.ItemData newItem)
        {

            // TODO: check if inventory is full or if the creature can hold the item

            for (int i = 0; i < Inventory.Count; i++)
            {
                if (Inventory[i].ItemData.Id == newItem.Id)
                {
                    Inventory[i].Count++;
                    return true;
                }
            }

            Inventory.Add(new InventoryItem() { ItemData = newItem, Count = 1 });

            return true;
        }

        public void RemoveItem(InventoryItem item, int removeCount)
        {
            item.Count--;

            if (item.Count == 0)
            {
                Inventory.Remove(item);

                for (int i = 0; i < EquipSlots.Length; i++)
                {
                    if (EquipSlots[i] == item)
                    {
                        EquipSlots[i] = null;
                    }
                }
            }
        }

        public InventoryItem GetItemByName(string itemName)
        {
            foreach (InventoryItem item in Inventory)
            {
                if (item.ItemData.Name == itemName) return item;
            }
            return null;
        }

        public void AddTrait(Data.TraitData traitData)
        {
            if (traitData == null)
            {
                Debug.LogWarning("Tried to add null trait");
                return;
            }

            // TODO: color changes?

            transform.localScale *= traitData.ModelScaleMultiplier;

            if (traitData.IsTrait)
            {
                CurrentTrait = traitData;
                return;
            }

            if (Mutations.Select(m => m.Id).Contains(traitData.Id))
            {
                Debug.LogWarning($"Tried to add mutation '{traitData.Id}' second time for '{name}'.");
                return;
            }

            Mutations.Add(traitData);

            // Enable 3D model
            if (traitData.ModelAssetPath.Length > 0 && traitData.ModelAssetPath != "Empty")
            {
                // TODO
                bool found = false;

                var searchingFor = Data.Id.Substring(0, 1).ToUpperInvariant() + Data.Id.Substring(1) + traitData.ModelAssetPath;

                //for (int i = 0; i < transform.childCount; i++)
                //{
                //    var obj = transform.GetChild(i);
                //    if (obj.gameObject.name == searchingFor)
                //    {
                //        obj.gameObject.SetActive(true);
                //        found = true;
                //        break;
                //    }
                //}

                Transform[] trs = GetComponentsInChildren<Transform>(true);
                foreach (Transform t in trs)
                {
                    if (t.gameObject.name == searchingFor)
                    {
                        t.gameObject.SetActive(true);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Debug.LogWarning($"Could not find trait model '{searchingFor}' for {gameObject.name}");
                }
            }
        }

        public Data.TraitData GetRandomTrait(bool isTrait = true)
        {
            // TODO: check allowed traits
            //var allowedKeys = Verminator.Data.GameData.Instance.TraitData.Values.Where(t => t.IsTrait == isTrait).Select(t => t.Id).ToArray();
            var allowedKeys = Data.AllowedTraits.Where(k => Verminator.Data.GameData.Instance.TraitData[k].IsTrait == isTrait).ToArray();

            if (allowedKeys.Length == 0)
            {
                return null;
            }

            int index = Random.Range(0, allowedKeys.Length);
            var key = allowedKeys[index];
            return Verminator.Data.GameData.Instance.TraitData[key];
        }

        public Data.TraitData GetRandomMutation()
        {
            return GetRandomTrait(false);
        }
    }
}