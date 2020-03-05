using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator.Data
{
    public class GameData
    {
        public static GameData Instance;

        public readonly Dictionary<string, ItemData> ItemData = new Dictionary<string, ItemData>();
        public readonly Dictionary<string, CreatureData> CreatureData = new Dictionary<string, CreatureData>();
        public readonly Dictionary<string, TraitData> TraitData = new Dictionary<string, TraitData>();
        public readonly Dictionary<int, string[]> SpawnList = new Dictionary<int, string[]>();
        public readonly Dictionary<int, string[]> ItemSpawnList = new Dictionary<int, string[]>();

        public static void LoadData()
        {
            int GetDefault(int defaultValue, string key, KeyValuePair<string, SimpleJSON.JSONNode> collection)
            {
                if (collection.Value[key] != null)
                {
                    return collection.Value[key].AsInt;
                }
                return defaultValue;
            }

            TextAsset textAsset = Resources.Load<TextAsset>("gamedata");
            string rawJsonData = textAsset.text;

            var parsedData = SimpleJSON.JSON.Parse(rawJsonData);
            GameData gameData = new GameData();

            foreach (var item in parsedData["items"])
            {
                var itemData = new ItemData()
                {
                    Id = item.Key,
                    Name = item.Value["name"],
                    Description = item.Value["desc"],
                    AssetPath = item.Value["assetpath"],
                    Weight = item.Value["weight"].AsInt,
                    Damage = item.Value["dmg"] != null ? (string)item.Value["dmg"] : "1d1",
                    DamageTypeStr = item.Value["dmgtype"] != null ? (string)item.Value["dmgtype"] : "blunt",
                    Ammo = item.Value["ammo"] != null ? (string)item.Value["ammo"] : "",
                    IsEdible = item.Value["edible"] != null ? item.Value["edible"].AsBool : false,
                    MinRange = GetDefault(1, "minrange", item),
                    MaxRange = GetDefault(2, "maxrange", item),
                    GainHealth = GetDefault(0, "gainhp", item),
                    GainMana = GetDefault(0, "gainmp", item),
                    GainSanity = GetDefault(0, "gainsanity", item),
                    SlashingRes = GetDefault(0, "slashingres", item),
                    BluntRes = GetDefault(0, "bluntres", item),
                    PiercingRes = GetDefault(0, "piercingres", item),
                    MagicRes = GetDefault(0, "magicres", item),
                    ArmorSlot = item.Value["armorslot"] != null ? (string)item.Value["armorslot"] : "",
                    ItemPrefab = Resources.Load<GameObject>(item.Value["assetpath"])
                };
                itemData.DamageType = (DamageType)System.Enum.Parse(typeof(DamageType), itemData.DamageTypeStr, true);
                gameData.ItemData.Add(item.Key, itemData);
            }

            foreach (var cre in parsedData["creatures"])
            {
                var creData = new CreatureData()
                {
                    Name = cre.Value["name"],
                    AssetPath = cre.Value["assetpath"],
                    BaseMaxHp = cre.Value["maxhp"].AsInt,
                    BaseMaxMp = cre.Value["maxmp"] != null ? cre.Value["maxmp"].AsInt : 0,
                    BaseSpeed = cre.Value["speed"].AsInt,
                    BaseMeleeSkill = cre.Value["melee"].AsInt,
                    BaseRangedSkill = cre.Value["ranged"].AsInt,
                    BaseStr = cre.Value["str"].AsInt,
                    BaseInt = cre.Value["int"] != null ? cre.Value["int"].AsInt : 0,
                    BaseAggroRange = cre.Value["aggrorange"] != null ? cre.Value["aggrorange"].AsInt : 0,
                    CreaturePrefab = Resources.Load<GameObject>(cre.Value["assetpath"])
                };
                List<string> allowedTraits = new List<string>();
                if (cre.Value["allowedtraits"])
                {
                    foreach (var key in cre.Value["allowedtraits"])
                    {
                        allowedTraits.Add(key.Value);
                    }
                }
                creData.AllowedTraits = allowedTraits.ToArray();
                List<string> weaponList = new List<string>();
                if (cre.Value["weapons"])
                {
                    foreach (var key in cre.Value["weapons"])
                    {
                        weaponList.Add(key.Value);
                    }
                }
                creData.WeaponList = weaponList.ToArray();
                gameData.CreatureData.Add(cre.Key, creData);
            }

            foreach (var trait in parsedData["traits"])
            {
                var traitData = new TraitData()
                {
                    Id = trait.Key,
                    Name = trait.Value["name"],
                    Description = trait.Value["desc"] != null ? (string)trait.Value["desc"] : "",
                    ModelAssetPath = trait.Value["modelassetpath"],
                    MaxHpBonus = GetDefault(0, "hpbonus", trait),
                    StrBonus = GetDefault(0, "strbonus", trait),
                    MeleeBonus = GetDefault(0, "meleebonus", trait),
                    RangedBonus = GetDefault(0, "rangedbonus", trait),
                    ModelScaleMultiplier = trait.Value["scalemultiplier"].AsFloat,
                    AggroRangeModifier = GetDefault(0, "aggrorangemodifier", trait),
                    SlashingResBonus = GetDefault(0, "slashingresbonus", trait),
                    BluntResBonus = GetDefault(0, "bluntresbonus", trait),
                    PiercingResBonus = GetDefault(0, "piercingresbonus", trait),
                    MagicResBonus = GetDefault(0, "magicresbonus", trait),
                    //ModelPrefab = Resources.Load<GameObject>(trait.Value["ModelAssetPath"])
                };
                gameData.TraitData.Add(trait.Key, traitData);
            }

            foreach (var spawnlist in parsedData["spawnlist"])
            {
                List<string> keys = new List<string>();
                foreach (var key in spawnlist.Value.AsArray)
                {
                    keys.Add(key.Value);
                }
                gameData.SpawnList.Add(int.Parse(spawnlist.Key), keys.ToArray());
            }

            foreach (var spawnlist in parsedData["itemspawnlist"])
            {
                List<string> keys = new List<string>();
                foreach (var key in spawnlist.Value.AsArray)
                {
                    keys.Add(key.Value);
                }
                gameData.ItemSpawnList.Add(int.Parse(spawnlist.Key), keys.ToArray());
            }

            Instance = gameData;
        }
    }
}