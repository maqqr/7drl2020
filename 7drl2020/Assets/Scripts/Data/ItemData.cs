using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator.Data
{
    [System.Serializable]
    public class ItemData
    {
        public string Id;
        public string Name;
        public string Description;
        public int Weight;

        public int MinRange;
        public int MaxRange;

        public string Damage;
        public string DamageTypeStr;
        public DamageType DamageType;

        public bool IsEdible;
        public int GainHealth;
        public int GainMana;
        public int GainSanity;

        public string Ammo;

        public string ArmorSlot;
        public int SlashingRes;
        public int BluntRes;
        public int PiercingRes;
        public int MagicRes;

        public int ManaCost;
        public List<string> Effects;

        public string AssetPath;
        public GameObject ItemPrefab;

        public bool IsArmor => ArmorSlot != "";

        public ArmorSlot GetArmorSlot()
        {
            return (ArmorSlot)System.Enum.Parse(typeof(ArmorSlot), ArmorSlot, true);
        }

        public int GetResistance(DamageType type)
        {
            switch (type)
            {
                case DamageType.Slashing: return SlashingRes;
                case DamageType.Blunt: return BluntRes;
                case DamageType.Piercing: return PiercingRes;
                case DamageType.Magic: return MagicRes;
                default:
                    break;
            }
            return 0;
        }
    }
}