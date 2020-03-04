using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator
{
    public class GameManager : MonoBehaviour
    {
        public LevelGenerator.Scripts.LevelGenerator levelGeneratorPrefab;
        public GameObject debugSphere;
        public GameObject itemLineUIPrefab;
        public GameObject inventoryCanvas;

        public int currentFloorIndex = -1;
        private List<DungeonFloor> dungeonFloors = new List<DungeonFloor>();
        private Stack<GameViews.IGameView> gameViews = new Stack<GameViews.IGameView>();

        public Creature PlayerCreature;
        //private int advanceTime;

        public Mesh ShoeMesh;
        public Material ShoeMaterial;

        public DungeonFloor CurrentFloor => currentFloorIndex >= 0 && currentFloorIndex < dungeonFloors.Count ? dungeonFloors[currentFloorIndex] : null;

        public Spells Spell = new Spells();

        public Vector2Int mouseTile;
        public bool mouseTileChanged;

        public GameObject UIEquipSlots;

        public int lastUsedSlot=0;

        public MessageBuffer MessageBuffer;

        public int MaxLampOil = 100;
        public int CurrentLampOil = 25;
        public int MaxSanity = 100;
        public int CurrentSanity = 100;


        public void GainLampOil(int amount)
        {
            CurrentLampOil = Mathf.Clamp(CurrentLampOil + amount, 0, MaxLampOil);
        }

        public void GainSanity(int amount)
        {
            CurrentSanity = Mathf.Clamp(CurrentSanity + amount, 0, MaxSanity);
        }

        public void DrawShoe(Vector3 position, Quaternion rotation)
        {
            Graphics.DrawMesh(ShoeMesh, Matrix4x4.TRS(position, rotation, Vector3.one), ShoeMaterial, 0);
        }

        public Vector2Int TileCoordinateUnderMouse()
        {
            // TODO: raycast from camera
            // RaycastHit hit;
            Vector3 objectHit = new Vector3(0, 0, 0);
            // Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // if (Physics.Raycast(ray, out hit)) {
            //     objectHit = hit.point;
            //     
            //     // Do something with the object that was hit by the raycast.
            // }

            Plane plane = new Plane(Vector3.up, 0f);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float dToPlane;
            if (plane.Raycast(ray, out dToPlane))
            {
                objectHit = ray.GetPoint(dToPlane);
            }
            return Utils.ConvertToTileCoord(objectHit);
        }

        public bool IsPlayerDead()
        {
            // TODO: implement properly
            return false;
        }

        void Start()
        {
            Data.GameData.LoadData();

            NextDungeonFloor();
            AddNewView(new GameViews.InGameView());
            Spell.setGameManager(this);

            // Spawn player to the current floor
            PlayerCreature = CurrentFloor.GetComponent<DungeonFloor>().SpawnCreature("player", Vector2Int.zero);
            PlayerCreature.gameObject.transform.parent = null;

            PlayerCreature.AddItem(Data.GameData.Instance.ItemData["shortsword"]);
            PlayerCreature.AddItem(Data.GameData.Instance.ItemData["oilflask"]);
            PlayerCreature.AddItem(Data.GameData.Instance.ItemData["bread"]);

            Camera.main.GetComponent<CameraController>().FollowTransform = PlayerCreature.gameObject.transform;

            UpdateEquipSlotGraphics();

            for (int i = 0; i < UIEquipSlots.transform.childCount; i++)
            {
                UIEquipSlots.transform.GetChild(i).GetComponent<EquipSlotButton>().OnClick += EquipSlotClicked;
            }

            MessageBuffer = FindObjectOfType<MessageBuffer>();
            MessageBuffer.AddMessage(Color.green, "You enter the tavern's cellar.");
        }

        public void EquipSlotClicked(int index)
        {
            lastUsedSlot = index-1;
            for (int i = 0; i < UIEquipSlots.transform.childCount; i++)
            {
                var slot = UIEquipSlots.transform.GetChild(i).GetComponent<EquipSlotButton>();
                if (i==lastUsedSlot) {
                    slot.SetColor(UnityEngine.Color.red);
                }
                else {
                    slot.SetColor(UnityEngine.Color.gray);
                }
            }
        }

        public void AddNewView(GameViews.IGameView view)
        {
            if (gameViews.Count > 0)
            {
                gameViews.Peek().CloseView();
            }

            gameViews.Push(view);
            view.Initialize(this);
            view.OpenView();
        }

        public void NextDungeonFloor()
        {
            // Disable current level
            if (CurrentFloor != null)
            {
                CurrentFloor.gameObject.SetActive(false);
            }

            // Remove player from old level
            if (CurrentFloor != null && PlayerCreature != null)
            {
                //PlayerCreature.gameObject.transform.parent = null;
                CurrentFloor.Creatures.Remove(PlayerCreature);
            }

            // Go up one level
            currentFloorIndex++;

            // Generate dungeon if it does not exist
            if (currentFloorIndex >= dungeonFloors.Count)
            {
                if (levelGeneratorPrefab == null)
                {
                    Debug.LogError("levelGeneratorPrefab is missing!");
                    return;
                }

                DungeonFloor createdDungeonFloor;

                int attempt = 0;
                while (attempt < 10)
                {
                    attempt++;
                    Debug.Log("Generation attempt " + attempt);

                    //GameObject generatorInstance = Instantiate(generatorPrefab);
                    LevelGenerator.Scripts.LevelGenerator generator = Instantiate(levelGeneratorPrefab); //generatorInstance.GetComponent<LevelGen>();

                    // Add empty game object that will be filled with objects generated by the level generator
                    GameObject dungeonFloorObj = new GameObject("Floor" + currentFloorIndex);
                    createdDungeonFloor = dungeonFloorObj.AddComponent<DungeonFloor>();

                    //generator.Seed = 234121242;
                    generator.SectionContainer = dungeonFloorObj.transform;
                    generator.GenerateLevel();
                    Debug.Log("Last seed: " + generator.Seed);
                    Destroy(generator.gameObject);

                    if (IsDungeonValid(createdDungeonFloor))
                    {
                        dungeonFloors.Add(createdDungeonFloor);
                        //createdDungeonFloor.Initialize(this);
                        break;
                    }
                    else
                    {
                        // Something went wrong, clean up failed floor
                        Destroy(dungeonFloorObj);
                    }
                }

                // Dungeon was generated, spawn some monsters next
                var enemySpawnPoints = CurrentFloor.gameObject.transform.GetComponentsInChildren<CreatureSpawnPoint>();
                Debug.Log("Creature spawn point count: " + enemySpawnPoints.Length);
                for (int i = 0; i < enemySpawnPoints.Length; i++)
                {
                    SpawnCreatureAtSpawnPoint(enemySpawnPoints[i]);
                }

                // Spawn items in dungeon
                var itemSpawnPoints = CurrentFloor.gameObject.transform.GetComponentsInChildren<ItemSpawnPoint>();
                Debug.Log("Item spawn point count: " + itemSpawnPoints.Length);
                for (int i = 0; i < itemSpawnPoints.Length; i++)
                {
                    SpawnItemAtSpawnPoint(itemSpawnPoints[i]);
                }
            }
            else
            {
                CurrentFloor.gameObject.SetActive(true);
            }

            // Add player to the current floor
            if (PlayerCreature != null)
            {
                CurrentFloor.Creatures.Add(PlayerCreature);
                //PlayerCreature.gameObject.transform.parent = CurrentFloor.gameObject.transform;
            }

            //pathfindDirty = true;
            //CurrentFloorObject.GetComponent<DungeonLevel>().UpdateAllReferences();

            //var spawnPoint = CurrentFloorObject.transform.GetComponentInChildren<PlayerSpawnPoint>();
            //if (spawnPoint)
            //{
            //    Vector2Int point = Utils.ConvertToGameCoord(spawnPoint.transform.position);
            //    playerObject.transform.position = Utils.ConvertToWorldCoord(point);
            //    playerObject.transform.rotation = Quaternion.LookRotation(spawnPoint.transform.forward);

            //    Camera.transform.localRotation = Quaternion.identity;
            //    Camera.GetComponent<SmoothMouseLook>().targetDirection = transform.localRotation.eulerAngles;
            //    Camera.GetComponent<SmoothMouseLook>().targetCharacterDirection = playerObject.transform.localRotation.eulerAngles;
            //    Camera.GetComponent<SmoothMouseLook>().ResetCamera();
            //    playerCreature.Position = point;
            //}
            //else
            //{
            //    Debug.LogError("Spawn point not found!");
            //}
        }

        private void SpawnCreatureAtSpawnPoint(CreatureSpawnPoint creatureSpawnPoint)
        {
            if (!(UnityEngine.Random.Range(1, 101) <= creatureSpawnPoint.SpawnChance))
            {
                return;
            }

            if (string.IsNullOrEmpty(creatureSpawnPoint.SpawnCreature))
            {
                if (!Data.GameData.Instance.SpawnList.ContainsKey(currentFloorIndex))
                {
                    Debug.LogError("Spawn list missing for floor " + currentFloorIndex);
                    return;
                }

                var creatureKeyList = Data.GameData.Instance.SpawnList[currentFloorIndex];
                int index = UnityEngine.Random.Range(0, creatureKeyList.Length);
                CurrentFloor.SpawnCreature(creatureKeyList[index], Utils.ConvertToTileCoord(creatureSpawnPoint.transform.position));
            }
            else
            {
                CurrentFloor.SpawnCreature(creatureSpawnPoint.SpawnCreature, Utils.ConvertToTileCoord(creatureSpawnPoint.transform.position));
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
                CurrentFloor.SpawnItem(itemKeyList[index], Utils.ConvertToTileCoord(itemSpawnPoint.transform.position));
            }
            else
            {
                CurrentFloor.SpawnItem(itemSpawnPoint.SpawnItem, Utils.ConvertToTileCoord(itemSpawnPoint.transform.position));
            }
        }

        public bool Fight(Creature attacker, Creature defender)
        {
            if (attacker == null || defender == null)
            {
                string msg = attacker == null ? "Attacker cannot be null. " : "";
                msg += defender == null ? "Defender cannot be null." : "";
                Debug.LogError(nameof(Fight) + ": " + msg);
                return false;
            }


            int usedSlot = attacker == PlayerCreature ? lastUsedSlot : 0;
            Data.ItemData weapon;
            try {
                weapon = attacker.EquipSlots[usedSlot].ItemData;
            }
            catch {
                MessageBuffer.AddMessage(Color.white, $"{attacker.Data.Name} has no weapon equiped at slot {usedSlot}");
                return false;
            }
            int dist = (int)Vector2.Distance(attacker.Position,defender.Position);
            if(dist<weapon.MinRange || dist>weapon.MaxRange) {
                MessageBuffer.AddMessage(Color.white, $"{attacker.Data.Name} can't attack at this distance");
                return false;
            }

            MessageBuffer.AddMessage(Color.white, $"{attacker.Data.Name} attacks {defender.Data.Name}");

            // TODO: Check for spell usage
            bool hit;
            if(weapon.Ammo!=null && weapon.Ammo != "") {
                InventoryItem ammo = attacker.GetItemByName(weapon.Ammo);
                if (ammo != null) {
                    hit = UnityEngine.Random.Range(0,20)+1<=attacker.RangedSkill;
                    attacker.RemoveItem(ammo,1);
                }
                else {
                    MessageBuffer.AddMessage(Color.white, $"{attacker.Data.Name} has no ammo {weapon.Ammo}");
                    return false;
                }
                
            }
            else {
                hit = UnityEngine.Random.Range(0,20)+1<=attacker.MeleeSkill;
            }
            if (hit) {
                DamageType dmgType = weapon.DamageType;
                int dmg = Utils.RollDice(weapon.Damage,true) +attacker.Strength;
                dmg = dmg*(1-defender.GetResistance(dmgType));
                defender.Hp -= dmg;
                MessageBuffer.AddMessage(Color.white, $"{defender.Data.Name} takes {dmg} damage!");
            }
            else {
                MessageBuffer.AddMessage(Color.white, $"{attacker.Data.Name} misses!");
            }
            return true;
        }

        public void UpdateEquipSlotGraphics()
        {
            if (UIEquipSlots == null)
            {
                Debug.LogError("UIEquipSlots reference is missing");
                return;
            }

            for (int i = 0; i < UIEquipSlots.transform.childCount; i++)
            {
                var slot = UIEquipSlots.transform.GetChild(i).GetComponent<EquipSlotButton>();
                slot.SetNumber(i + 1);
                if (i==lastUsedSlot) {
                    slot.SetColor(UnityEngine.Color.red);
                }
                else {
                    slot.SetColor(UnityEngine.Color.gray);
                }

                if (i < PlayerCreature.EquipSlots.Length)
                {
                    slot.UpdateGraphic(PlayerCreature.EquipSlots[i]);
                }
            }
        }

        public void PreviousDungeonFloor()
        {
            if (currentFloorIndex == 0)
            {
                Debug.LogWarning("Attempted to go below level 0");
                return;
            }

            // Disable current level
            CurrentFloor.gameObject.SetActive(false);
            //PlayerCreature.gameObject.transform.parent = null;
            CurrentFloor.Creatures.Remove(PlayerCreature);

            // Activate previous level
            currentFloorIndex--;
            CurrentFloor.gameObject.SetActive(true);
            //PlayerCreature.gameObject.transform.parent = CurrentFloor.gameObject.transform;
            CurrentFloor.Creatures.Add(PlayerCreature);

            //var downstairsPoint = CurrentFloor.gameObject.transform.GetComponentInChildren<DownStairsReturnPoint>();
            //Vector2Int point = Utils.ConvertToGameCoord(downstairsPoint.transform.position);
            //playerObject.transform.position = Utils.ConvertToWorldCoord(point);
            //playerCreature.Position = point;

            //pathfindDirty = true;
            //CurrentFloorObject.GetComponent<DungeonLevel>().UpdateAllReferences();
        }

        private bool IsDungeonValid(DungeonFloor dungeonFloor)
        {
            return true;
        }

        //public void AdvanceTime(int deltaTime)
        //{
        //    advanceTime += deltaTime;
        //}

        //public void UpdateGameWorld()
        //{
        //    if (advanceTime > 0)
        //    {
        //        AdvanceGameWorld(advanceTime);
        //        advanceTime = 0;
        //    }
        //}

        public void AdvanceGameWorld(int deltaTime)
        {
            // Update all enemy creatures
            List<Creature> dyingCritters = new List<Creature>();
            foreach (var creature in CurrentFloor.Creatures)
            {
                if (creature == PlayerCreature)
                {
                    continue;
                }
                if (creature.Hp <=0) {
                    dyingCritters.Add(creature);
                    continue;
                }

                creature.TimeElapsed += deltaTime;

                while (creature.TimeElapsed >= creature.Speed)
                {
                    creature.TimeElapsed -= creature.Speed;
                    creature.AIUpdate(this);
                }
            }
            foreach (var creature in dyingCritters) {
                Debug.Log($"{creature.Data.Name} dies!");
                CurrentFloor.DestroyCreature(creature);
            }

            //AdjustNutrition(-1);
            //UpdateHunger();
            //UpdateHearts(playerCreature, PlayerHearts);
            //Debug.Log("Player nutrition: " + playerObject.GetComponent<Player>().Nutrition);
        }

        private void Update()
        {
            if (!CurrentFloor.IsInitialized)
            {
                CurrentFloor.Initialize(this);
            }

            Vector2Int tile = TileCoordinateUnderMouse();
            mouseTileChanged = tile != mouseTile;
            mouseTile = tile;

            Vector3 unityCoordinate = new Vector3(tile.x + 0.5f, 0.0f, tile.y + 0.5f);
            debugSphere.transform.position = unityCoordinate;

            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                NextDungeonFloor();
            }
            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                PreviousDungeonFloor();
            }

            if (gameViews.Count == 0)
            {
                //gameoverTimer -= Time.deltaTime;
                //if (gameoverTimer < 0f)
                //{
                //    if (BackgroundMusic.Instance)
                //    {
                //        BackgroundMusic.Instance.SetMusic(BackgroundMusic.Music.Menu, 1f, true);
                //    }
                //    UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
                //}
                return;
            }

            //if (pathfindDirty)
            //{
            //    pathfindDirty = false;
            //    UpdatePathfindingGrid();
            //}

            bool closeView = gameViews.Peek().UpdateView();

            if (closeView)
            {
                var view = gameViews.Pop();
                view.CloseView();
                view.Destroy();

                if (gameViews.Count > 0)
                {
                    // Re-enable previous view
                    gameViews.Peek().OpenView();
                }

                //if (gameViews.Count == 0)
                //{
                //    MessageBuffer.AddMessage(Color.red, "GAME OVER");
                //    gameoverTimer = 3f;
                //}
            }
        }
    }
}