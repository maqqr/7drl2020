﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator
{
    public class DungeonFloor : MonoBehaviour
    {
        private GameManager gameManager;
        public List<Creature> Creatures = new List<Creature>();
        public List<Item> Items = new List<Item>();
        public Dictionary<Vector2Int, Tile> Tiles = new Dictionary<Vector2Int, Tile>();

        public Bounds Bounds;
        private Pathfinding.DungeonGrid pathfindingGrid;

        public void Initialize(GameManager gameManager)
        {
            this.gameManager = gameManager;

            foreach (var tile in GetComponentsInChildren<Tile>())
            {
                var position = Utils.ConvertToTileCoord(tile.gameObject.transform.position);
                if (Tiles.ContainsKey(position))
                {
                    Debug.LogError("Two tiles share the same position!");
                }
                Tiles.Add(position, tile);
            }

            UpdatePathfindingGrid();
        }

        public Creature SpawnCreature(string id, Vector2Int spawnCoordinate) // creature id is defined in json
        {
            if (!Data.GameData.Instance.CreatureData.ContainsKey(id))
            {
                Debug.LogError($"{nameof(SpawnCreature)}: Tried to spawn creature with invalid id '{id}'");
                return null;
            }

            Data.CreatureData data = Data.GameData.Instance.CreatureData[id];

            if (data.CreaturePrefab == null)
            {
                Debug.LogError($"{nameof(SpawnCreature)}: Creature prefab missing for id '{id}'");
                return null;
            }

            GameObject creatureObject = Instantiate(data.CreaturePrefab, Utils.ConvertToUnityCoord(spawnCoordinate), Quaternion.identity);
            creatureObject.transform.position = Utils.ConvertToUnityCoord(spawnCoordinate);
            creatureObject.transform.parent = transform;

            Creature creature = creatureObject.GetComponent<Creature>();
            creature.Data = data;
            creature.Hp = creature.MaxHp;
            creature.Position = spawnCoordinate;

            Creatures.Add(creature);
            return creature;
        }

        public void DestroyCreature(Creature creature)
        {
            if (!Creatures.Contains(creature))
            {
                Debug.LogError($"{nameof(DestroyCreature)}: Failed to destroy creature that does not exists on this dungeon floor");
                return;
            }
            Creatures.Remove(creature);
            Destroy(creature.gameObject);
        }

        public Item SpawnItem(string id, Vector2Int spawnCoordinate) // item id is defined in json
        {
            if (!Data.GameData.Instance.ItemData.ContainsKey(id))
            {
                Debug.LogError($"{nameof(SpawnItem)}: Tried to spawn item with invalid id '{id}'");
                return null;
            }

            Data.ItemData data = Data.GameData.Instance.ItemData[id];

            if (data.ItemPrefab == null)
            {
                Debug.LogError($"{nameof(SpawnItem)}: Item prefab missing for id '{id}'");
                return null;
            }

            GameObject itemObject = Instantiate(data.ItemPrefab, Utils.ConvertToUnityCoord(spawnCoordinate), Quaternion.identity);
            itemObject.transform.position = Utils.ConvertToUnityCoord(spawnCoordinate);
            itemObject.transform.parent = transform;

            Item item = itemObject.GetComponent<Item>();
            item.Data = data;
            item.Position = spawnCoordinate;

            Items.Add(item);
            return item;
        }

        public void DestroyItem(Item item)
        {
            if (!Items.Contains(item))
            {
                Debug.LogError($"{nameof(DestroyItem)}: Failed to destroy item that does not exists on this dungeon floor");
                return;
            }
            Items.Remove(item);
            Destroy(item.gameObject);
        }

        public Creature GetCreatureAt(Vector2Int position, bool includePlayer = true)
        {
            foreach (var creature in Creatures)
            {
                if (creature.Position == position)
                {
                    return creature;
                }
            }

            if (includePlayer && gameManager.PlayerCreature.Position == position)
            {
                return gameManager.PlayerCreature;
            }

            return null;
        }

        public Item GetItemAt(Vector2Int position)
        {
            foreach (var item in Items)
            {
                if (item.Position == position)
                {
                    return item;
                }
            }
            return null;
        }

        public List<Item> GetItemsAt(Vector2Int position)
        {
            List<Item> result = new List<Item>();
            foreach (var item in Items)
            {
                if (item.Position == position)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public Tile GetTileAt(Vector2Int position)
        {
            if (Tiles.ContainsKey(position))
            {
                return Tiles[position];
            }
            return null;
        }

        public void UpdatePathfindingGrid()
        {
            CalculateBounds();
            int minX = (int)Mathf.Round(Bounds.min.x);
            int maxX = (int)Mathf.Round(Bounds.max.x);
            int minY = (int)Mathf.Round(Bounds.min.z);
            int maxY = (int)Mathf.Round(Bounds.max.z);

            pathfindingGrid = new Pathfinding.DungeonGrid();
            pathfindingGrid.CreateGrid(minX, maxX, minY, maxY, delegate (Vector2Int pos)
            {
                return IsWalkable(pos);
            });
        }

        public bool IsWalkable(Vector2Int pos)
        {
            return true;
        }

        private void CalculateBounds()
        {
            foreach (var key in Tiles.Keys)
            {
                Bounds.Encapsulate(Utils.ConvertToUnityCoord(key));
            }
            Bounds.Expand(1f);
        }

        public List<Vector2Int> FindPath(Vector2Int from, Vector2Int to)
        {
            System.Func<Vector2Int, Vector2Int, bool> isWalkableFrom = delegate (Vector2Int start, Vector2Int end)
            {
                return IsWalkableFrom(start, end);
            };

            return Pathfinding.Pathfinding.FindPath(pathfindingGrid, from, to, isWalkableFrom);
        }

        public bool IsWalkableFrom(Vector2Int from, Vector2Int to) // 'from' and 'to' are assumed to be next to each other
        {
            // TODO: check that manhattan distance between 'from' and 'to' is not greater than 1?

            //Vector3 fromWorld = Utils.ConvertToUnityCoord(from) + new Vector3(0f, 0.5f, 0f);
            //Vector3 toWorld = Utils.ConvertToWorldCoord(to) + new Vector3(0f, 0.5f, 0f);
            //Vector3 raycastDir = toWorld - fromWorld;
            //bool wayBlocked = Physics.Raycast(fromWorld, raycastDir, 1f, ignoreMask);

            //bool targetSpaceFree = IsWalkable(to, ignoreMask);
            //return targetSpaceFree && !wayBlocked;

            if (!Tiles.ContainsKey(to))
            {
                return false;
            }


            var targetTile = Tiles[to];
            return targetTile.IsWalkable;
        }
    }
}