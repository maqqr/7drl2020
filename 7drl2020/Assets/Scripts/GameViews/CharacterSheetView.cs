using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Verminator.GameViews
{
    public class CharacterSheetView : IGameView
    {
        private GameManager gameManager;

        private List<GameObject> guiItems = new List<GameObject>();

        private CharacterSheetUI sheetUI;

        private int selectedIndex = 0;

        public void Initialize(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        public void Destroy()
        {
        }

        public void OpenView()
        {
            gameManager.inventoryCanvas.SetActive(true);
            sheetUI = gameManager.inventoryCanvas.GetComponent<CharacterSheetUI>();
            sheetUI.InteractionWindow.SetActive(false);

            var itemListTransform = sheetUI.ItemList.transform;
            for (int i = 0; i < itemListTransform.childCount; i++)
            {
                GameObject.Destroy(itemListTransform.GetChild(i).gameObject);
            }

            sheetUI.Equip.OnClick = OnEquipClicked;
            sheetUI.Drop.OnClick = OnDropClicked;
            sheetUI.Eat.OnClick = OnEatClicked;

            HideItemDesc();
            RefreshView();
        }

        public void CloseView()
        {
            gameManager.inventoryCanvas.SetActive(false);
        }

        public bool UpdateView()
        {
            //if (highlightedItem != null && Utils.IsPressed(gameManager.keybindings.DropItem))
            //{
            //    DropItem(highlightedItem);
            //    highlightedItem = null;
            //    RefreshView();
            //}
            //else if (highlightedItem != null && Utils.IsPressed(gameManager.keybindings.ConsumeItem))
            //{
            //    ConsumeItem(highlightedItem);
            //    highlightedItem = null;
            //    RefreshView();
            //}

            //return Utils.IsPressed(gameManager.keybindings.OpenInventory) || gameManager.playerCreature.Hp < 1;
            return Input.GetKeyDown(KeyCode.I);
        }

        private void RefreshView()
        {
            for (int i = 0; i < guiItems.Count; i++)
            {
                GameObject.Destroy(guiItems[i]);
            }
            //Debug.Log($"Destroyed {guiItems.Count} gui lines");
            guiItems.Clear();


            var player = gameManager.PlayerCreature;
            var sheetUi = gameManager.inventoryCanvas.GetComponent<CharacterSheetUI>();

            bool IsEquipped(InventoryItem item)
            {
                if (item.ItemData.IsArmor)
                {
                    foreach (var kv in player.ArmorSlots)
                    {
                        if (kv.Value == item)
                        {
                            return true;
                        }
                    }
                }

                for (int i = 0; i < gameManager.PlayerCreature.EquipSlots.Length; i++)
                {
                    if (player.EquipSlots[i] == item)
                    {
                        return true;
                    }
                }
                return false;
            }

            string ArmorName(ArmorSlot slot)
            {
                if (player.ArmorSlots[slot] != null)
                {
                    return player.ArmorSlots[slot].ItemData.Name;
                }
                return "Nothing";
            }

            // Update equipment text
            string equipmentDesc = "";
            equipmentDesc += "Head:\n - " + ArmorName(ArmorSlot.Head) + "\n";
            equipmentDesc += "Body:\n - " + ArmorName(ArmorSlot.Body) + "\n";
            equipmentDesc += "Feet:\n - " + ArmorName(ArmorSlot.Legs) + "\n";
            sheetUi.EquipmentText.text = equipmentDesc;

            // Update player stats
            sheetUI.StrengthText.text = Utils.FixFont(player.Strength.ToString());
            sheetUI.IntelligenceText.text = Utils.FixFont(player.Intelligence.ToString());
            sheetUI.MeleeText.text = Utils.FixFont(player.MeleeSkill.ToString());
            sheetUI.RangedText.text = Utils.FixFont(player.RangedSkill.ToString());
            sheetUI.SlashingText.text = Utils.FixFont(player.GetResistance(DamageType.Slashing).ToString() + "%");
            sheetUI.BluntText.text = Utils.FixFont(player.GetResistance(DamageType.Blunt).ToString() + "%");
            sheetUI.PiercingText.text = Utils.FixFont(player.GetResistance(DamageType.Piercing).ToString() + "%");
            sheetUI.MagicText.text = Utils.FixFont(player.GetResistance(DamageType.Magic).ToString() + "%");

            // Instantiate items in inventory
            for (int i = 0; i < player.Inventory.Count; i++)
            {
                var obj = GameObject.Instantiate(gameManager.itemLineUIPrefab);

                obj.transform.SetParent(sheetUi.ItemList.transform);
                obj.transform.localScale = Vector3.one;
                obj.GetComponent<RectTransform>().localPosition = new Vector3(0, -30f * i, 0);
                guiItems.Add(obj);

                InventoryItem invItem = player.Inventory[i];
                Data.ItemData item = invItem.ItemData;

                var itemLineUi = obj.GetComponent<ItemLineUI>();
                itemLineUi.ItemText.text = "- " + (invItem.Count > 1 ? "" + invItem.Count + "x " : "") + item.Name.ToLower();

                if (IsEquipped(invItem))
                {
                    itemLineUi.ItemText.text += " [e]";
                }

                int index = i;
                itemLineUi.OnClick += delegate (PointerEventData eventData)
                {
                    OnItemClicked(itemLineUi, index);
                };
                itemLineUi.OnEnter += delegate (PointerEventData eventData)
                {
                    ShowItemDesc(index);
                };
                itemLineUi.OnExit += delegate (PointerEventData eventData)
                {
                    HideItemDesc();
                };
            }
        }

        private void ShowItemDesc(int index)
        {
            sheetUI.DescWindow.SetActive(true);

            var invItem = gameManager.PlayerCreature.Inventory[index];
            sheetUI.DescWindowTitle.text = invItem.ItemData.Name;

            string msg = "";
            if (!invItem.ItemData.IsArmor)
            {
                msg += Utils.FixFont("Damage: " + invItem.ItemData.Damage + " " + invItem.ItemData.DamageType.ToString().ToLower() + "\n");
                msg += Utils.FixFont("Range: " + invItem.ItemData.MinRange + " - " + invItem.ItemData.MaxRange + " tiles\n\n");
            }
            else
            {
                msg += "Resistances:\n";
                msg += Utils.FixFont("Slashing: " + invItem.ItemData.SlashingRes + "  Blunt: " + invItem.ItemData.BluntRes + "\n");
                msg += Utils.FixFont("Piercing: " + invItem.ItemData.PiercingRes + "  Magic: " + invItem.ItemData.MagicRes + "\n\n");
            }
            msg += invItem.ItemData.Description;
            sheetUI.DescWindowText.text = msg;
        }

        private void HideItemDesc()
        {
            sheetUI.DescWindow.SetActive(false);
        }

        private void UnselectAll()
        {
            for (int i = 0; i < guiItems.Count; i++)
            {
                guiItems[i].GetComponent<ItemLineUI>().SetSelected(false);
            }
        }

        private void OnItemClicked(ItemLineUI itemLineUi, int index)
        {
            UnselectAll();
            itemLineUi.SetSelected(true);
            sheetUI.InteractionWindow.SetActive(true);
            selectedIndex = index;

            var invItem = gameManager.PlayerCreature.Inventory[selectedIndex];
            sheetUI.Equip.Text.text = invItem.ItemData.Id == "oilflask" ? "Use" : "Equip";
        }

        private void RemoveArmor(InventoryItem invItem)
        {
            var makeNull = new List<ArmorSlot>();
            foreach (var kv in gameManager.PlayerCreature.ArmorSlots)
            {
                if (kv.Value == invItem)
                {
                    makeNull.Add(kv.Key);
                }
            }
            makeNull.ForEach(s => gameManager.PlayerCreature.ArmorSlots[s] = null);
        }

        private void OnEquipClicked(PointerEventData eventData)
        {
            var invItem = gameManager.PlayerCreature.Inventory[selectedIndex];

            if (invItem.ItemData.Id == "oilflask")
            {
                // Consume oil instead of equipping
                int gainOil = 10;
                gameManager.CurrentLampOil = Mathf.Min(gameManager.MaxLampOil, gameManager.CurrentLampOil + gainOil);
                int percent = (int)(100.0f * (gameManager.CurrentLampOil / (float)gameManager.MaxLampOil));
                gameManager.MessageBuffer.AddMessage(Color.white, Utils.FixFont($"You put the oil into your lantern. Your lantern is now {percent}% filled."));
                gameManager.PlayerCreature.RemoveItem(invItem, 1);
                UnselectAll();
                RefreshView();
                sheetUI.InteractionWindow.SetActive(false);
                return;
            }

            if (invItem.ItemData.IsArmor)
            {
                RemoveArmor(invItem); // Make sure same item is not equipped to another armor slot
                gameManager.PlayerCreature.ArmorSlots[invItem.ItemData.GetArmorSlot()] = invItem;
            }
            else
            {
                // Make sure same item is not equipped to multiple slots by unequipping first
                for (int i = 0; i < gameManager.PlayerCreature.EquipSlots.Length; i++)
                {
                    if (gameManager.PlayerCreature.EquipSlots[i] == invItem)
                    {
                        gameManager.PlayerCreature.EquipSlots[i] = null;
                    }
                }
                gameManager.PlayerCreature.EquipSlots[gameManager.lastUsedSlot] = invItem;
                gameManager.UpdateEquipSlotGraphics();
            }

            UnselectAll();
            RefreshView();
            sheetUI.InteractionWindow.SetActive(false);

            gameManager.MessageBuffer.AddMessage(Color.white, "You equip the " + invItem.ItemData.Name + ".");
        }

        private void OnDropClicked(PointerEventData eventData)
        {
            var invItem = gameManager.PlayerCreature.Inventory[selectedIndex];
            gameManager.PlayerCreature.RemoveItem(invItem, 1);
            gameManager.CurrentFloor.SpawnItem(invItem.ItemData.Id, gameManager.PlayerCreature.Position);

            // Unequip dropped item
            for (int i = 0; i < gameManager.PlayerCreature.EquipSlots.Length; i++)
            {
                if (gameManager.PlayerCreature.EquipSlots[i] == invItem)
                {
                    gameManager.PlayerCreature.EquipSlots[i] = null;
                    gameManager.UpdateEquipSlotGraphics();
                }
            }

            // Unequip dropped armor
            RemoveArmor(invItem);

            sheetUI.InteractionWindow.SetActive(false);
            UnselectAll();
            RefreshView();

            gameManager.MessageBuffer.AddMessage(Color.white, "You drop the " + invItem.ItemData.Name + ".");
        }

        private void OnEatClicked(PointerEventData eventData)
        {
            var invItem = gameManager.PlayerCreature.Inventory[selectedIndex];

            if (!invItem.ItemData.IsEdible)
            {
                gameManager.MessageBuffer.AddMessage(Color.white, "You lick the " + invItem.ItemData.Name + ". Nothing happens.");
                sheetUI.InteractionWindow.SetActive(false);
                UnselectAll();
                RefreshView();
                return;
            }

            // Unequip eaten item
            for (int i = 0; i < gameManager.PlayerCreature.EquipSlots.Length; i++)
            {
                if (gameManager.PlayerCreature.EquipSlots[i] == invItem)
                {
                    gameManager.PlayerCreature.EquipSlots[i] = null;
                    gameManager.UpdateEquipSlotGraphics();
                }
            }
            RemoveArmor(invItem);

            gameManager.PlayerCreature.RemoveItem(invItem, 1);

            sheetUI.InteractionWindow.SetActive(false);
            UnselectAll();
            RefreshView();

            string eff = "";
            if (invItem.ItemData.GainHealth > 0) eff += "You gain some health. ";
            if (invItem.ItemData.GainHealth < 0) eff += "You lose some health. ";
            if (invItem.ItemData.GainMana > 0) eff += "You gain some mana. ";
            if (invItem.ItemData.GainMana < 0) eff += "You lose some mana. ";
            if (invItem.ItemData.GainSanity > 0) eff += "You regain some sanity. ";
            if (invItem.ItemData.GainSanity < 0) eff += "You lose some sanity. ";

            gameManager.PlayerCreature.Hp += invItem.ItemData.GainHealth;
            gameManager.PlayerCreature.Mp += invItem.ItemData.GainMana;
            gameManager.GainSanity(invItem.ItemData.GainSanity);

            gameManager.MessageBuffer.AddMessage(Color.white, "You consume the " + invItem.ItemData.Name + ". " + eff);
        }
    }
}