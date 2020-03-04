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

            // Instantiate items in inventory
            for (int i = 0; i < player.Inventory.Count; i++)
            {
                var obj = GameObject.Instantiate(gameManager.itemLineUIPrefab);

                obj.transform.SetParent(sheetUi.ItemList.transform);
                obj.transform.localScale = Vector3.one;
                obj.GetComponent<RectTransform>().localPosition = new Vector3(0, 30f * i, 0);
                guiItems.Add(obj);

                InventoryItem invItem = player.Inventory[i];
                Data.ItemData item = invItem.ItemData;

                var itemLineUi = obj.GetComponent<ItemLineUI>();
                itemLineUi.ItemText.text = "- " + (invItem.Count > 1 ? "" + invItem.Count + "x " : "") + item.Name.ToLower();

                int index = i;
                itemLineUi.OnClick += delegate (PointerEventData eventData)
                {
                    OnItemClick(itemLineUi, index);
                };
            }
        }

        private void OnItemClick(ItemLineUI itemLineUi, int index)
        {
            for (int i = 0; i < guiItems.Count; i++)
            {
                guiItems[i].GetComponent<ItemLineUI>().SetSelected(false);
            }
            itemLineUi.SetSelected(true);
            sheetUI.InteractionWindow.SetActive(true);
        }
    }
}