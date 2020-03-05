using System;
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
        private Graph graph = new Graph();
        Dictionary<Vector2Int, Node> cache = new Dictionary<Vector2Int, Node>();
        HashSet<Vector2Int> blockedPositions = new HashSet<Vector2Int>();

        private int currentFloorIndex = -1;

        public bool IsInitialized { get; private set; }

        public void Initialize(GameManager gameManager, int currentFloor)
        {
            this.gameManager = gameManager;
            currentFloorIndex = currentFloor;

            // Spawn creatures
            var enemySpawnPoints = gameObject.transform.GetComponentsInChildren<CreatureSpawnPoint>();
            Debug.Log("Creature spawn point count: " + enemySpawnPoints.Length);
            for (int i = 0; i < enemySpawnPoints.Length; i++)
            {
                var cre = SpawnCreatureAtSpawnPoint(enemySpawnPoints[i]);
                if (cre != null)
                {
                    cre.AddTrait(cre.GetRandomTrait());

                    int muts = UnityEngine.Random.Range(0, 3);
                    for (int m = 0; m < muts; m++)
                    {
                        cre.AddTrait(cre.GetRandomMutation());
                    }
                    cre.Hp = cre.MaxHp; // Mutations may change max hp, fix current hp
                }
            }

            // Spawn items
            var itemSpawnPoints = gameObject.transform.GetComponentsInChildren<ItemSpawnPoint>();
            Debug.Log("Item spawn point count: " + itemSpawnPoints.Length);
            for (int i = 0; i < itemSpawnPoints.Length; i++)
            {
                SpawnItemAtSpawnPoint(itemSpawnPoints[i]);
            }

            foreach (var tile in GetComponentsInChildren<Tile>())
            {
                var position = Utils.ConvertToTileCoord(tile.gameObject.transform.position);
                if (Tiles.ContainsKey(position))
                {
                    Debug.LogError("Two tiles share the same position!");
                }
                Tiles.Add(position, tile);
            }

            foreach (var obj in GetComponentsInChildren<MovementBlocker>())
            {
                blockedPositions.Add(Utils.ConvertToTileCoord(obj.transform.position));
            }

            UpdatePathfindingGrid();
            IsInitialized = true;
        }

        private Creature SpawnCreatureAtSpawnPoint(CreatureSpawnPoint creatureSpawnPoint)
        {
            if (!(UnityEngine.Random.Range(1, 101) <= creatureSpawnPoint.SpawnChance))
            {
                return null;
            }

            if (string.IsNullOrEmpty(creatureSpawnPoint.SpawnCreature))
            {
                if (!Data.GameData.Instance.SpawnList.ContainsKey(currentFloorIndex))
                {
                    Debug.LogError("Spawn list missing for floor " + currentFloorIndex);
                    return null;
                }

                var creatureKeyList = Data.GameData.Instance.SpawnList[currentFloorIndex];
                int index = UnityEngine.Random.Range(0, creatureKeyList.Length);
                return SpawnCreature(creatureKeyList[index], Utils.ConvertToTileCoord(creatureSpawnPoint.transform.position));
            }
            else
            {
                return SpawnCreature(creatureSpawnPoint.SpawnCreature, Utils.ConvertToTileCoord(creatureSpawnPoint.transform.position));
            }
        }

        private void SpawnItemAtSpawnPoint(ItemSpawnPoint itemSpawnPoint)
        {
            if (!(UnityEngine.Random.Range(1, 101) <= itemSpawnPoint.SpawnChance))
            {
                return;
            }

            if (string.IsNullOrEmpty(itemSpawnPoint.SpawnItem))
            {
                //Debug.LogError("Trying to spawn empty item. Item spawn point is missing item name.");
                //return;
                if (!Data.GameData.Instance.ItemSpawnList.ContainsKey(currentFloorIndex))
                {
                    Debug.LogError("Item spawn list missing for floor " + currentFloorIndex);
                    return;
                }

                var itemKeyList = Data.GameData.Instance.ItemSpawnList[currentFloorIndex];
                int index = UnityEngine.Random.Range(0, itemKeyList.Length);
                SpawnItem(itemKeyList[index], Utils.ConvertToTileCoord(itemSpawnPoint.transform.position), itemSpawnPoint.transform);
            }
            else
            {
                SpawnItem(itemSpawnPoint.SpawnItem, Utils.ConvertToTileCoord(itemSpawnPoint.transform.position), itemSpawnPoint.transform);
            }
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

        public Item SpawnItem(string id, Vector2Int spawnCoordinate, Transform freePlacement) // item id is defined in json
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

            if (freePlacement != null)
            {
                itemObject.transform.position = freePlacement.position;
                itemObject.transform.rotation = freePlacement.rotation;
            }

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
            if (includePlayer && gameManager.PlayerCreature.Position == position)
            {
                return gameManager.PlayerCreature;
            }

            if (!includePlayer && gameManager.PlayerCreature.Position == position)
            {
                return null;
            }

            foreach (var creature in Creatures)
            {
                if (creature.Position == position)
                {
                    return creature;
                }
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
            //CalculateBounds();
            //int minX = (int)Mathf.Round(Bounds.min.x);
            //int maxX = (int)Mathf.Round(Bounds.max.x);
            //int minY = (int)Mathf.Round(Bounds.min.z);
            //int maxY = (int)Mathf.Round(Bounds.max.z);

            //pathfindingGrid = new Pathfinding.DungeonGrid();
            //pathfindingGrid.CreateGrid(minX, maxX, minY, maxY, delegate (Vector2Int pos)
            //{
            //    return IsWalkable(pos);
            //});

            var deltas = new List<Vector2Int>();
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    if (!(x == 0 && y == 0))
                    {
                        deltas.Add(new Vector2Int(x, y));
                    }
                }
            }

            foreach (var kv in Tiles)
            {
                var node = new Node() { Position = kv.Key };
                graph.nodes.Add(node);
                cache.Add(kv.Key, node);
            }

            foreach (var node in graph.nodes)
            {
                foreach (var delta in deltas)
                {
                    var neighbourPos = node.Position + delta;
                    if (cache.ContainsKey(neighbourPos))
                    {
                        var neighbour = cache[neighbourPos];;

                        if (IsWalkableFrom(node.Position, neighbourPos))
                        {
                            node.connections.Add(neighbour);
                        }
                    }
                }
            }
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
            //System.Func<Vector2Int, Vector2Int, bool> isWalkableFrom = delegate (Vector2Int start, Vector2Int end)
            //{
            //    return IsWalkableFrom(start, end);
            //};

            //return Pathfinding.Pathfinding.FindPath(pathfindingGrid, from, to, isWalkableFrom);

            if (!cache.ContainsKey(from) || !cache.ContainsKey(to))
            {
                return new List<Vector2Int>();
            }

            List<Vector2Int> pathPoints = new List<Vector2Int>();
            var path = graph.GetShortestPath(cache[from], cache[to]);
            for (int i = 0; i < path.nodes.Count; i++)
            {
                pathPoints.Add(path.nodes[i].Position);
            }
            pathPoints.RemoveAt(0);
            return pathPoints;
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
            if (from == to) return true;
            if (Vector2.Distance(from, to) == 1)
            {
                if (Physics.Raycast(Utils.ConvertToUnityCoord(from), Utils.ConvertToUnityCoord(to) - Utils.ConvertToUnityCoord(from), 1f))
                {
                    return false;
                }
            }
            if (Vector2.Distance(from, to) > 1 && Vector2.Distance(from, to) < 2)
            {
                bool success = false;
                if (!Physics.Raycast(Utils.ConvertToUnityCoord(from), Utils.ConvertToUnityCoord(new Vector2Int(to.x, from.y)) - Utils.ConvertToUnityCoord(from), 1f))
                {
                    if (!Physics.Raycast(Utils.ConvertToUnityCoord(new Vector2Int(to.x, from.y)), Utils.ConvertToUnityCoord(to) - Utils.ConvertToUnityCoord(new Vector2Int(to.x, from.y)), 1f))
                    {
                        success = true;
                    }
                }
                if (!Physics.Raycast(Utils.ConvertToUnityCoord(from), Utils.ConvertToUnityCoord(new Vector2Int(from.x, to.y)) - Utils.ConvertToUnityCoord(from), 1f))
                {
                    if (!Physics.Raycast(Utils.ConvertToUnityCoord(new Vector2Int(from.x, to.y)), Utils.ConvertToUnityCoord(to) - Utils.ConvertToUnityCoord(new Vector2Int(from.x, to.y)), 1f))
                    {
                        success = true;
                    }
                }
                if (!success) return false;
            }



            if (!Tiles.ContainsKey(to))
            {
                return false;
            }

            if (blockedPositions.Contains(to))
            {
                return false;
            }

            var targetTile = Tiles[to];
            return targetTile.IsWalkable;
        }
    }
}