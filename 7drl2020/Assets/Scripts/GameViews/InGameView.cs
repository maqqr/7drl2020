﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator.GameViews
{
    public class InGameView : IGameView
    {
        private GameManager gameManager;
        private GameObject highlightedObject = null;

        private float forcedCooldown = 0f;

        private float playerTransitionSpeed = 0.3f;
        private float originalTransitionSpeed = 0.3f;

        private int debugMode = 5;

        private List<Vector2Int> pathToPos;

        public List<GameObject> pickupList = new List<GameObject>();

        public void Initialize(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        public void Destroy()
        {
        }

        public void OpenView()
        {
            gameManager.InventoryPressed = OpenInventory;
        }

        public void CloseView()
        {
        }

        public bool UpdateView()
        {
            if (gameManager.IsPlayerDead())
            {
                return true;
            }

            if (forcedCooldown >= 0f)
            {
                forcedCooldown -= Time.deltaTime;
            }

            bool playerCanAct = gameManager.PlayerCreature.InSync && forcedCooldown < 0f;
            if (playerCanAct)
            {
                //gameManager.UpdateGameWorld();

                if (gameManager.IsPlayerDead())
                {
                    return true;
                }
                HandlePlayerInput();
            }
            DrawPath();


            return false;
        }

        private void HideItemPickupList()
        {
            foreach (var item in pickupList)
            {
                GameObject.Destroy(item);
            }
            pickupList.Clear();
        }

        private void UpdateItemPickupList()
        {
            HideItemPickupList();

            int i = 0;
            PickupItemLineUI CreateLine(float x, string text)
            {
                var obj = GameObject.Instantiate(gameManager.pickupItemLinePrefab);
                obj.transform.SetParent(gameManager.pickupList.transform);
                obj.transform.localScale = Vector3.one;
                obj.transform.localRotation = Quaternion.identity;
                obj.GetComponent<RectTransform>().localPosition = new Vector3(x, -40 * i, 0);
                var ui = obj.GetComponentInChildren<PickupItemLineUI>();
                ui.ItemText.text = text;
                i++;
                pickupList.Add(obj);
                return ui;
            }

            var items = gameManager.CurrentFloor.GetItemsAt(gameManager.PlayerCreature.Position);
            if (items.Count > 0)
            {
                var title = CreateLine(0f, "Items here:");
                GameObject.Destroy(title.GetComponent<UnityEngine.UI.Button>());
                foreach (var item in items)
                {
                    var line = CreateLine(20f, item.Data.Name);
                    Item pickup = item;
                    line.OnClick += delegate
                    {
                        Debug.Log("Pickup by mouse");
                        PickupItem(pickup);
                    };
                }
            }
        }

        private void OpenInventory()
        {
            forcedCooldown = 0.2f;
            HideItemPickupList();
            gameManager.EnemyStatsWindow.SetActive(false);
            gameManager.AddNewView(new CharacterSheetView());
        }

        private void DrawPath()
        {
            Vector2Int? playerMoveTo = gameManager.TileCoordinateUnderMouse();
            var player = gameManager.PlayerCreature;
            if (gameManager.mouseTileChanged)
            {
                UpdateEnemyStatWindow();

                try
                {
                    pathToPos = gameManager.CurrentFloor.FindPath(player.Position, playerMoveTo.Value);
                }
                catch { }
            }
            if (pathToPos != null)
            {
                for (int ind = 0; ind < pathToPos.Count; ind++)
                {
                    try
                    {
                        Quaternion angle = new Quaternion();
                        angle.SetLookRotation(new Vector3(pathToPos[ind].x, 0, pathToPos[ind].y) - new Vector3(pathToPos[ind - 1].x, 0, pathToPos[ind - 1].y));
                        gameManager.DrawShoe(new Vector3(pathToPos[ind - 1].x + 0.5f, 0, pathToPos[ind - 1].y + 0.5f), angle);
                    }
                    catch { }
                }
            }
        }

        private void UpdateEnemyStatWindow()
        {
            var enemy = gameManager.CurrentFloor.GetCreatureAt(gameManager.TileCoordinateUnderMouse(), includePlayer: false);
            gameManager.EnemyStatsWindow.SetActive(enemy != null && enemy.IsVisible);
            if (enemy != null && enemy.IsVisible)
            {
                var cre = enemy.GetComponent<Creature>();
                gameManager.EnemyStatsName.GetComponent<TMPro.TextMeshProUGUI>().text = cre.Name;
                gameManager.EnemyStatsDesc.GetComponent<TMPro.TextMeshProUGUI>().text = cre.Desc;

                var rect = gameManager.EnemyStatsWindow.GetComponent<RectTransform>();

                var mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                bool onRightSide = mousePos.x < 0.5f;

                Debug.Log("right side: " + onRightSide + " mousepos: " + mousePos);

                rect.anchorMin = onRightSide ? new Vector2(1, 1) : new Vector2(0, 1);
                rect.anchorMax = onRightSide ? new Vector2(1, 1) : new Vector2(0, 1);
                rect.pivot = onRightSide ? new Vector2(1, 1) : new Vector2(0, 1);
            }
        }

        private void PickupItem(Item item)
        {
            if (gameManager.PlayerCreature.AddItem(item.Data))
            {
                gameManager.CurrentFloor.DestroyItem(item);
                gameManager.MessageBuffer.AddMessage(Color.white, "Picked up " + item.Data.Name);

                // Equip it instantly. This is only for debugging
                //player.EquipSlots[0] = player.Inventory[0];
                //gameManager.UpdateEquipSlotGraphics();

                gameManager.AdvanceGameWorld(gameManager.PlayerCreature.Speed);
            }
            else
            {
                gameManager.MessageBuffer.AddMessage(Color.white, "Cannot pick up " + item.Data.Name + ". You are carrying too much.");
            }
        }

        private void WaitOneTurn()
        {
            gameManager.MessageBuffer.AddMessage(Color.gray, "You stand still for a moment.");
            gameManager.AdvanceGameWorld(gameManager.PlayerCreature.Speed);
            forcedCooldown = 0.5f;
        }

        private void HandlePlayerInput()
        {
            Vector2Int? playerMoveTo = null;
            var player = gameManager.PlayerCreature;

            // Pick up item using the item list in left top corner
            if (Input.GetMouseButtonDown(0))
            {
                // This is a dirty hack, but Unity UI refuses to cooperate
                foreach (var itemLine in pickupList)
                {
                    var lineUi = itemLine.GetComponent<PickupItemLineUI>();
                    if (lineUi.IsMouseOver)
                    {
                        lineUi.SimulateClick();
                        forcedCooldown = 0.2f;
                        UpdateItemPickupList();
                        return;
                    }
                }
            }

            // Move the player on mouse click
            if (Input.GetMouseButton(0) && !player.OnMove)
            {
                var pointer = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                pointer.position = Input.mousePosition;
                var raycastResults = new List<UnityEngine.EventSystems.RaycastResult>();
                UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer, raycastResults);
                bool cursorOverUIStuff = raycastResults.Count > 0;

                if (!cursorOverUIStuff)
                {
                    playerMoveTo = gameManager.TileCoordinateUnderMouse();
                }

            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                var item = gameManager.CurrentFloor.GetItemAt(player.Position);
                if (item != null)
                {
                    PickupItem(item);
                    return;
                }
            }


            if (Input.GetKeyDown(KeyCode.I))
            {
                OpenInventory();
                return;
            }


            if (Input.GetKeyDown(KeyCode.A))
            {
                gameManager.EquipSlotClicked(1);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                gameManager.EquipSlotClicked(2);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                gameManager.EquipSlotClicked(3);
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                gameManager.EquipSlotClicked(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                gameManager.EquipSlotClicked(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                gameManager.EquipSlotClicked(3);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                WaitOneTurn();
                return;
            }


            if (playerMoveTo != null)
            {
                Creature creatureBlocking = gameManager.CurrentFloor.GetCreatureAt(playerMoveTo.Value);
                if (creatureBlocking == null)
                {
                    // Get the path to the desired location
                    pathToPos = gameManager.CurrentFloor.FindPath(player.Position, playerMoveTo.Value);
                    if (pathToPos != null && pathToPos.Count > 0)
                    {
                        //player.Position = pathToPos[0];
                        // Stop if the pathing goes through a creature
                        // TODO: Invisible creatures don't block the way
                        creatureBlocking = gameManager.CurrentFloor.GetCreatureAt(pathToPos[0]);
                        if (creatureBlocking != null)
                        {
                            gameManager.MessageBuffer.AddMessage(Color.white, creatureBlocking.Data.Name + " is blocking the way.");
                            forcedCooldown = 1f;
                            return;
                        }
                        if (player.Move(pathToPos[0]))
                        {
                            pathToPos.RemoveAt(0);
                            gameManager.AdvanceGameWorld(player.Speed);

                            // This is just for debugging:
                            //var item = gameManager.CurrentFloor.GetItemAt(player.Position);
                            //if (item != null)
                            //{
                            //    gameManager.MessageBuffer.AddMessage(Color.white, "You see " + Utils.GetIndefiniteArticle(item.Data.Name) + " " + item.Data.Name + " here.");
                            //}
                        }

                    }

                }
                else if (creatureBlocking != player)
                {
                    bool losBlocked = false;

                    var tiles = Utils.Line(player.Position, creatureBlocking.Position);
                    for (int i = 1; i < tiles.Count; i++)
                    {
                        if (!gameManager.CurrentFloor.IsWalkableFrom(tiles[i - 1], tiles[i]))
                        {
                            losBlocked = true;
                            break;
                        }
                    }

                    if (losBlocked)
                    {
                        gameManager.MessageBuffer.AddMessage(Color.gray, "Cannot attack through a wall.");
                        forcedCooldown = 0.5f;
                    }
                    else
                    {
                        if (gameManager.Fight(player, creatureBlocking))
                        {
                            player.Attacking = true;
                            if (pathToPos.Count > 0)
                            {
                                player.Move(pathToPos[0], true);
                            }
                            else
                            {
                                forcedCooldown = 0.5f;
                            }
                            gameManager.AdvanceGameWorld(player.Speed);
                            UpdateEnemyStatWindow();
                        }
                        else
                        {
                            // Failed the combat for some reason
                            // Have a cooldown to block the message spam
                            forcedCooldown = 0.5f;
                        }
                        //gameManager.AdvanceTime(player.Speed);
                    }
                }
                else
                {
                    // Player is clicked: Wait one turn
                    WaitOneTurn();
                }

                UpdateItemPickupList();
            }
        }
    }
}