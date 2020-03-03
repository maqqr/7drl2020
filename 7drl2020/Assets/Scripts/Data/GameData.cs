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
                gameData.ItemData.Add(item.Key, new ItemData()
                {
                    Name = item.Value["name"],
                    AssetPath = item.Value["assetpath"],
                    Weight = item.Value["weight"].AsInt,
                    MeleeDamage = item.Value["damM"].AsInt,
                    ThrowingDamage = item.Value["damT"].AsInt,
                    Defence = item.Value["def"].AsInt,
                    Description = item.Value["desc"],
                    ItemPrefab = Resources.Load<GameObject>(item.Value["assetpath"]),
                    Sanity = item.Value["sanity"] != null ? item.Value["sanity"].AsInt : 0,
                    Healing = item.Value["healing"] != null ? item.Value["healing"].AsInt : 0,
                    Poisoning = item.Value["poisoning"] != null ? item.Value["poisoning"].AsInt : 0,
                    Breakable = item.Value["breakable"] != null ? item.Value["breakable"].AsBool : false,
                    Experience = item.Value["experience"] != null ? item.Value["experience"].AsInt : 0
                });
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
                gameData.CreatureData.Add(cre.Key, creData);
            }

            foreach (var trait in parsedData["traits"])
            {
                var traitData = new TraitData()
                {
                    Name = trait.Value["name"],
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