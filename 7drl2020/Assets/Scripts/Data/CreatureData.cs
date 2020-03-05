﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator.Data
{
    [System.Serializable]
    public class TraitData
    {
        public string Id;
        public string Name;
        public string Description;
        public int MaxHpBonus;
        public int StrBonus;
        public int MeleeBonus;
        public int RangedBonus;
        public float ModelScaleMultiplier;
        public int AggroRangeModifier;

        public int SlashingResBonus;
        public int BluntResBonus;
        public int PiercingResBonus;
        public int MagicResBonus;

        public string ModelAssetPath;

        public bool IsTrait => ModelAssetPath == "";
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

        public int BaseSlashingRes;
        public int BaseBluntRes;
        public int BasePiercingRes;
        public int BaseMagicRes;

        public string[] AllowedTraits;
        public string[] WeaponList;

        public string AssetPath;
        public GameObject CreaturePrefab;
    }
}