using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator
{
    public class EquipSlotButton : MonoBehaviour
    {
        public GameObject GraphicsContainer;

        public TMPro.TextMeshProUGUI SlotNumberText;

        public GameObject SlotBorder;

        public event System.Action<int> OnClick;

        int slotIndex;

        private void OnMouseDown()
        {
            Debug.Log("OnMouseDown " + name);

            OnClick?.Invoke(slotIndex);
        }

        public void SetNumber(int i)
        {
            slotIndex = i;
            SlotNumberText.text = i.ToString();
        }

        public void UpdateGraphic(InventoryItem item)
        {
            // Clean up previous models
            for (int i = 0; i < GraphicsContainer.transform.childCount; i++)
            {
                Destroy(GraphicsContainer.transform.GetChild(i).gameObject);
            }

            // Create model
            var obj = Instantiate(item.ItemData.ItemPrefab, GraphicsContainer.transform);

            foreach (var renderer in obj.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.gameObject.layer = LayerMask.NameToLayer("UI");
            }
        }

        public void SetColor(Color color)
        {
            SlotBorder.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", color);
        }
    }
}