using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator.Data
{
    [System.Serializable]
    public class TraitData
    {
        public string Name;
        public int StrBonus;
        public int IntBonus;
        public float ModelScaleMultiplier;

        public string ModelAssetPath;
        public GameObject ModelPrefab;
    }

    [System.Serializable]
    public class CreatureData
    {
        public string Name;
        public int BaseMaxHp;
        public int BaseMaxMp;
        public int BaseSpeed;
        public int BaseMeleeSkill;
        public int BaseRangedSkill;
        public int BaseStr;
        public int BaseInt;
        public int BaseAggroRange;

        public string[] AllowedTraits;

        public string AssetPath;
        public GameObject CreaturePrefab;
    }
}