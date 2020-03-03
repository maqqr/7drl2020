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

        public string AssetPath;
        public GameObject ItemPrefab;
    }
}