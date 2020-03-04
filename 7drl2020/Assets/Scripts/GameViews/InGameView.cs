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

        public void Initialize(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        public void Destroy()
        {
        }

        public void OpenView()
        {
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

        private void DrawPath()
        {
            Vector2Int? playerMoveTo = gameManager.TileCoordinateUnderMouse();
            var player = gameManager.PlayerCreature;
            if (gameManager.mouseTileChanged)
            {
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

        private void HandlePlayerInput()
        {
            Vector2Int? playerMoveTo = null;
            var player = gameManager.PlayerCreature;

            // This is just for testing: move player to where the mouse was clicked
            if (Input.GetMouseButton(0) && !player.OnMove)
            {
                playerMoveTo = gameManager.TileCoordinateUnderMouse();
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                var item = gameManager.CurrentFloor.GetItemAt(player.Position);
                if (item != null)
                {
                    if (player.AddItem(item.Data))
                    {
                        gameManager.CurrentFloor.DestroyItem(item);
                        gameManager.MessageBuffer.AddMessage(Color.white, "Picked up " + item.Data.Name);

                        // Equip it instantly. This is only for debugging
                        //player.EquipSlots[0] = player.Inventory[0];
                        //gameManager.UpdateEquipSlotGraphics();
                    }
                    else
                    {
                        gameManager.MessageBuffer.AddMessage(Color.white, "Cannot pick up " + item.Data.Name + ". You are carrying too much.");
                    }
                }
            }


            if (Input.GetKeyDown(KeyCode.I))
            {
                gameManager.AddNewView(new CharacterSheetView());
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


            //if (Utils.IsDown(gameManager.keybindings.PeekLeft))
            //{
            //    gameManager.playerAnim.Peek = -1;
            //}
            //else if (Utils.IsDown(gameManager.keybindings.PeekRight))
            //{
            //    gameManager.playerAnim.Peek = 1;
            //}
            //else
            //{
            //    gameManager.playerAnim.Peek = 0;
            //}

            //if (Utils.IsDown(gameManager.keybindings.MoveForward))
            //{
            //    playerMoveTo = Utils.ConvertToGameCoord(playerObj.transform.localPosition + playerObj.transform.forward);
            //}
            //else if (Utils.IsDown(gameManager.keybindings.MoveBackward))
            //{
            //    playerMoveTo = Utils.ConvertToGameCoord(playerObj.transform.localPosition - playerObj.transform.forward);
            //}
            //else if (Utils.IsDown(gameManager.keybindings.MoveRight))
            //{
            //    playerMoveTo = Utils.ConvertToGameCoord(playerObj.transform.localPosition + playerObj.transform.right);
            //}
            //else if (Utils.IsDown(gameManager.keybindings.MoveLeft))
            //{
            //    playerMoveTo = Utils.ConvertToGameCoord(playerObj.transform.localPosition - playerObj.transform.right);
            //}

            //if (Utils.IsPressed(gameManager.keybindings.PickUp))
            //{
            //    if (highlightedObject != null)
            //    {
            //        Unhighlight(highlightedObject);
            //        var interactable = highlightedObject.GetComponent<Interaction.Interactable>();
            //        bool advanceTime = false;
            //        for (int i = 0; i < interactable.Interactions.Length; i++)
            //        {
            //            if (interactable.Interactions[i].Interact(gameManager))
            //            {
            //                advanceTime = true;
            //            }
            //        }

            //        if (advanceTime)
            //        {
            //            gameManager.AdvanceTime(player.Speed);
            //        }
            //    }
            //}

            //if (Utils.IsPressed(gameManager.keybindings.Help))
            //{
            //    gameManager.HelpWindow.SetActive(!gameManager.HelpWindow.activeSelf);
            //}

            //if (Utils.IsPressed(gameManager.keybindings.Wait))
            //{
            //    gameManager.AdvanceTime(player.Speed);
            //}

            //if (Utils.IsPressed(gameManager.keybindings.Push) && gameManager.throwable != null)
            //{
            //    if (Vector3.Distance(gameManager.throwable.transform.position, gameManager.playerObject.transform.position) < 1.5f)
            //    {
            //        gameManager.throwable.isKinematic = false;
            //        gameManager.throwable.AddForce(new Vector3(gameManager.Camera.transform.forward.x, 0f, gameManager.Camera.transform.forward.z) * 10f, ForceMode.Impulse);
            //        gameManager.AdvanceTime(player.Speed);
            //    }
            //}

            //if (Utils.IsPressed(gameManager.keybindings.ThrowLeftHand) || Utils.IsPressed(gameManager.keybindings.ThrowRightHand))
            //{
            //    EquipSlot slot = Utils.IsPressed(gameManager.keybindings.ThrowLeftHand) ? EquipSlot.LeftHand : EquipSlot.RightHand;
            //    if (player.Equipment.ContainsKey(slot))
            //    {
            //        gameManager.PlayerThrowItem(slot);
            //    }
            //    else if (highlightedObject != null)
            //    {
            //        // Unhighlight(highlightedObject);
            //        var pick = highlightedObject.GetComponent<Interaction.PickupItem>();
            //        if (pick)
            //        {
            //            pick.Interact(gameManager);
            //            foreach (var invitem in player.Inventory)
            //            {
            //                if (invitem.ItemKey == pick.itemKey)
            //                {
            //                    gameManager.PlayerEquip(invitem, slot);
            //                    break;
            //                }
            //            }
            //            gameManager.AdvanceTime(player.Speed);
            //        }


            //    }
            //}

            //if (Utils.IsPressed(gameManager.keybindings.OpenPerkTree))
            //{
            //    gameManager.AddNewView(new PerkTreeView());
            //}

            //if (Utils.IsPressed(gameManager.keybindings.OpenInventory))
            //{
            //    gameManager.AddNewView(new InventoryView());
            //}

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
                        if (player.Move(pathToPos[0]))
                        {
                            pathToPos.RemoveAt(0);
                            gameManager.AdvanceGameWorld(player.Speed);

                            // This is just for debugging:
                            var item = gameManager.CurrentFloor.GetItemAt(player.Position);
                            if (item != null)
                            {
                                gameManager.MessageBuffer.AddMessage(Color.white, "You see " +Utils.GetIndefiniteArticle(item.Data.Name)+" "+ item.Data.Name+" here.");
                            }
                        }

                    }

                }
                else if (creatureBlocking != player)
                {
                    //gameManager.playerAnim.StartAttackAnimation();
                    if (gameManager.Fight(player, creatureBlocking))
                    { // TODO: Check for bugs related to pathToPos list
                        player.Attacking = true;
                        player.Move(pathToPos[0], true);
                        gameManager.AdvanceGameWorld(player.Speed);
                    }
                    else {
                        // Failed the combat for some reason
                        // Have a cooldown to block the message spam
                        forcedCooldown = 0.5f;
                    }
                    //gameManager.AdvanceTime(player.Speed);
                }

            }
        }
    }
}