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

            return false;
        }

        private void HandlePlayerInput()
        {
            Vector2Int? playerMoveTo = null;
            var player = gameManager.PlayerCreature;

            // This is just for testing: move player to where the mouse was clicked
            if (Input.GetMouseButtonDown(0))
            {
                playerMoveTo = gameManager.TileCoordinateUnderMouse();
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
                if (gameManager.CurrentFloor.IsWalkableFrom(player.Position, playerMoveTo.Value))
                {
                    Creature creatureBlocking = gameManager.CurrentFloor.GetCreatureAt(playerMoveTo.Value);
                    if (creatureBlocking == null)
                    {
                        player.Position = playerMoveTo.Value;
                        //gameManager.AdvanceTime(player.Speed);
                        //gameManager.UpdatePlayerVisibility();
                        gameManager.AdvanceGameWorld(player.Speed);
                    }
                    else if (creatureBlocking != player)
                    {
                        //gameManager.playerAnim.StartAttackAnimation();
                        gameManager.Fight(player, creatureBlocking);
                        //gameManager.AdvanceTime(player.Speed);
                        gameManager.AdvanceGameWorld(player.Speed);
                        forcedCooldown = 1.0f; // Add a small delay to prevent too fast attack spam
                    }
                }
            }
        }
    }
}