using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator
{
    public class GameManager : MonoBehaviour
    {
        public LevelGenerator.Scripts.LevelGenerator[] levelGeneratorPrefabs;
        public GameObject debugSphere;
        public GameObject itemLineUIPrefab;
        public GameObject inventoryCanvas;
        public GameObject pickupItemLinePrefab;
        public GameObject pickupList;

        public GameObject EnemyStatsWindow;
        public GameObject EnemyStatsName;
        public GameObject EnemyStatsDesc;

        public TMPro.TextMeshProUGUI HpText;
        public TMPro.TextMeshProUGUI MpText;
        public TMPro.TextMeshProUGUI SanityText;
        public TMPro.TextMeshProUGUI LampOilText;

        private float lanternOriginalIntensity;
        private Light lanternLight;

        public UnityEngine.UI.Image LanternImage;
        public Sprite LanternOnSprite;
        public Sprite LanternOffSprite;

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

        public int lastUsedSlot = 0;

        public MessageBuffer MessageBuffer;

        public int MaxLampOil = 100;
        public int CurrentLampOil = 25;
        public int MaxSanity = 100;
        public int CurrentSanity = 100;
        public int PlayerLevel = 1;
        public int PointsToSpend = 0;
        public bool LanternOn = true;

        public float GameWinTimer = 0f; // Small delay after defeating the queen

        public Action InventoryPressed;

        public void GainLampOil(int amount)
        {
            CurrentLampOil = Mathf.Clamp(CurrentLampOil + amount, 0, MaxLampOil);
        }

        public void GainSanity(int amount)
        {
            CurrentSanity = Mathf.Clamp(CurrentSanity + amount, 0, MaxSanity);
        }

        public void InventoryButtonPressed()
        {
            Debug.Log("inventory pressed");
            InventoryPressed?.Invoke();
        }

        public void LanternButtonPressed()
        {
            Debug.Log("lantern pressed");
            //InventoryPressed?.Invoke();

            if (CurrentLampOil == 0)
            {
                MessageBuffer.AddMessage(Color.white, "You cannot turn on your lantern until you fill it with lamp oil.");
                return;
            }

            LanternOn = !LanternOn;

            UpdateLampGraphics();

            if (LanternOn)
            {
                MessageBuffer.AddMessage(Color.yellow, "You turn on your lantern.");
            }
            else
            {
                MessageBuffer.AddMessage(Color.gray, "You turn off your lantern.");
            }
        }

        private static bool oilTutorial = true;

        public void UpdateLampGraphics()
        {
            LampOilText.text = Utils.FixFont($"{CurrentLampOil} turns");
            LanternImage.sprite = LanternOn ? LanternOnSprite : LanternOffSprite;

            lanternLight.intensity = LanternOn ? lanternOriginalIntensity : 0f;
        }

        public void SpendLampOil()
        {
            if (!LanternOn)
            {
                return;
            }

            if (CurrentLampOil > 0)
            {
                CurrentLampOil--;

                if (CurrentLampOil == 0)
                {
                    LanternOn = false;
                    LanternImage.sprite = LanternOffSprite;
                    MessageBuffer.AddMessage(Color.gray, "Your lantern ran out of lamp oil.");

                    if (oilTutorial)
                    {
                        MessageBuffer.AddMessage(Color.cyan, "You feel the darkness slowly taking away your sanity...");
                        oilTutorial = false;
                    }
                }
            }

            UpdateLampGraphics();
        }

        public void SanityCheck()
        {
            if (!LanternOn && CurrentSanity > 0)
            {
                CurrentSanity--;
            }
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

            MessageBuffer = FindObjectOfType<MessageBuffer>();
            MessageBuffer.AddMessage(Color.green, "You enter the tavern's cellar.");

            NextDungeonFloor();
            AddNewView(new GameViews.InGameView());
            Spell.setGameManager(this);

            // Spawn player to the current floor
            PlayerCreature = CurrentFloor.GetComponent<DungeonFloor>().SpawnCreature("player", Vector2Int.zero);
            PlayerCreature.gameObject.transform.parent = null;

            PlayerCreature.AddItem(Data.GameData.Instance.ItemData["shortsword"]);
            PlayerCreature.AddItem(Data.GameData.Instance.ItemData["oilflask"]);
            PlayerCreature.AddItem(Data.GameData.Instance.ItemData["bread"]);
            PlayerCreature.AddItem(Data.GameData.Instance.ItemData["studdedleatherarmour"]);
            PlayerCreature.AddItem(Data.GameData.Instance.ItemData["ratscalp"]);
            PlayerCreature.AddItem(Data.GameData.Instance.ItemData["flaskofspirits"]);
            PlayerCreature.AddItem(Data.GameData.Instance.ItemData["bow"]);
            PlayerCreature.AddItem(Data.GameData.Instance.ItemData["arrow"]);
            PlayerCreature.AddItem(Data.GameData.Instance.ItemData["arrow"]);
            PlayerCreature.AddItem(Data.GameData.Instance.ItemData["arrow"]);
            PlayerCreature.AddItem(Data.GameData.Instance.ItemData["arrow"]);
            PlayerCreature.AddItem(Data.GameData.Instance.ItemData["firescroll"]);
            PlayerCreature.EquipSlots[0] = PlayerCreature.Inventory[0];

            PlayerCreature.OnMovementAnimationEnd = OnPlayerMoveAnimationEnd;

            lanternLight = PlayerCreature.gameObject.GetComponentInChildren<Light>();
            lanternOriginalIntensity = lanternLight.intensity;

            Camera.main.GetComponent<CameraController>().FollowTransform = PlayerCreature.gameObject.transform;

            UpdateEquipSlotGraphics();
            UpdatePlayerStatsUI();
            UpdateLampGraphics();

            for (int i = 0; i < UIEquipSlots.transform.childCount; i++)
            {
                UIEquipSlots.transform.GetChild(i).GetComponent<EquipSlotButton>().OnClick += EquipSlotClicked;
            }
        }

        private void OnPlayerMoveAnimationEnd()
        {
            //Debug.Log("Anim end");
            if (CurrentFloor.IsDownstairsTile(PlayerCreature.Position))
            {
                MessageBuffer.AddMessage(Color.white, "You go down the stairs deeper into the cellar.");
                NextDungeonFloor();
            }
        }

        public void EquipSlotClicked(int index)
        {
            lastUsedSlot = index - 1;
            for (int i = 0; i < UIEquipSlots.transform.childCount; i++)
            {
                var slot = UIEquipSlots.transform.GetChild(i).GetComponent<EquipSlotButton>();
                if (i == lastUsedSlot)
                {
                    slot.SetColor(UnityEngine.Color.red);
                }
                else
                {
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

            // Go up one level (player character levels up)
            if (currentFloorIndex > 0)
            {
                PlayerLevel++;
                PointsToSpend++;
                MessageBuffer.AddMessage(Color.green, "Level up! Open your character sheet to assign one point to your attributes.");
            }

            // Generate dungeon if it does not exist
            if (currentFloorIndex >= dungeonFloors.Count)
            {
                if (currentFloorIndex >= levelGeneratorPrefabs.Length)
                {
                    Debug.LogError($"levelGeneratorPrefab for level index {currentFloorIndex} is missing!");
                    return;
                }

                DungeonFloor createdDungeonFloor;

                int attempt = 0;
                while (attempt < 10)
                {
                    attempt++;
                    Debug.Log("Generation attempt " + attempt);

                    //GameObject generatorInstance = Instantiate(generatorPrefab);
                    LevelGenerator.Scripts.LevelGenerator generator = Instantiate(levelGeneratorPrefabs[currentFloorIndex]);

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



        public bool Fight(Creature attacker, Creature defender)
        {
            if (attacker == null || defender == null)
            {
                string msg = attacker == null ? "Attacker cannot be null. " : "";
                msg += defender == null ? "Defender cannot be null." : "";
                Debug.LogError(nameof(Fight) + ": " + msg);
                return false;
            }
            // Check if an unblocked path exists.
            List<Vector2Int> path = Utils.Line(attacker.Position,defender.Position);
            foreach (Vector2Int tile in path) {
                try {
                    if (!CurrentFloor.Tiles[tile].IsWalkable) return false;
                }
                catch {
                    return false;
                }
                
            }

            int usedSlot = attacker == PlayerCreature ? lastUsedSlot : 0;
            Data.ItemData weapon;
            try
            {
                weapon = attacker.EquipSlots[usedSlot].ItemData;
            }
            catch
            {
                MessageBuffer.AddMessage(Color.white, $"{attacker.Data.Name} has no weapon equiped at slot {usedSlot + 1}");
                return false;
            }
            int dist = (int)Vector2.Distance(attacker.Position, defender.Position);
            if (dist < weapon.MinRange || dist > weapon.MaxRange)
            {
                MessageBuffer.AddMessage(Color.white, $"{attacker.Data.Name} can't attack at this distance");
                return false;
            }

            MessageBuffer.AddMessage(Color.white, $"{attacker.Data.Name} attacks {defender.Data.Name}");

            // TODO: Check for spell usage
            if (weapon.DamageTypeStr=="magic") {
                return Spell.Cast(weapon.Effect,attacker,defender,weapon.Damage);
            }


            bool hit;
            if (weapon.Ammo != null && weapon.Ammo != "")
            {
                InventoryItem ammo = attacker.GetItemByName(weapon.Ammo);
                if (ammo != null)
                {
                    hit = UnityEngine.Random.Range(0, 20) + 1 <= attacker.RangedSkill;
                    attacker.RemoveItem(ammo, 1);
                    UpdateEquipSlotGraphics();
                }
                else
                {
                    MessageBuffer.AddMessage(Color.white, $"{attacker.Data.Name} has no ammo {weapon.Ammo}");
                    return false;
                }

            }
            else
            {
                hit = UnityEngine.Random.Range(0, 20) + 1 <= attacker.MeleeSkill;
            }
            if (hit)
            {
                DamageType dmgType = weapon.DamageType;
                string dmgdice = weapon.Damage;
                if (attacker.Data.Name.Contains("swarm")) {
                    int missingHp = 10-(attacker.MaxHp - attacker.Hp)/2;
                    dmgdice = missingHp.ToString() + 'd'+weapon.Damage.Split('d')[1];
                    Debug.Log(weapon.Damage.Split('d')[1]);
                    Debug.Log("Swarm rolls with "+dmgdice);
                }
                int dmg = Utils.RollDice(dmgdice, true) + attacker.Strength;
                dmg = (int)(dmg * (1 - defender.GetResistance(dmgType) / 100.0f));
                defender.Hp -= dmg;
                MessageBuffer.AddMessage(Color.white, $"{defender.Data.Name} takes {dmg} {dmgType.ToString().ToLower()} damage!");
            }
            else
            {
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
                if (i == lastUsedSlot)
                {
                    slot.SetColor(UnityEngine.Color.red);
                }
                else
                {
                    slot.SetColor(UnityEngine.Color.gray);
                }

                if (i < PlayerCreature.EquipSlots.Length)
                {
                    slot.UpdateGraphic(PlayerCreature.EquipSlots[i]);
                }
            }
        }

        public void UpdatePlayerStatsUI()
        {
            HpText.text = Utils.FixFont($"HP: {PlayerCreature.Hp}/{PlayerCreature.MaxHp}");
            MpText.text = Utils.FixFont($"MP: {PlayerCreature.Mp}/{PlayerCreature.MaxMp}");
            SanityText.text = Utils.FixFont($"Sanity: {CurrentSanity}/{MaxSanity}");
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
                if (creature.Hp <= 0)
                {
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
            foreach (var creature in dyingCritters)
            {
                MessageBuffer.AddMessage(Color.white,$"{creature.Data.Name} dies!");
                CurrentFloor.DestroyCreature(creature);

                if (creature.Data.Id == "queen")
                {
                    GameWinTimer = 3f;
                }

                if (creature.Data.Id == "player")
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(3);
                }
            }

            SpendLampOil();
            SanityCheck();

            UpdatePlayerStatsUI();

            //AdjustNutrition(-1);
            //UpdateHunger();
            //UpdateHearts(playerCreature, PlayerHearts);
            //Debug.Log("Player nutrition: " + playerObject.GetComponent<Player>().Nutrition);
        }

        private void Update()
        {
            if (!CurrentFloor.IsInitialized)
            {
                CurrentFloor.Initialize(this, currentFloorIndex);
            }

            if (GameWinTimer > 0f)
            {
                GameWinTimer -= Time.deltaTime;
                if (GameWinTimer <= 0f)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(2);
                }
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