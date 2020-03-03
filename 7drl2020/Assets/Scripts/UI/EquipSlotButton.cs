using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator
{
    public class EquipSlotButton : MonoBehaviour
    {
        public GameObject GraphicsContainer;

        public TMPro.TextMeshProUGUI SlotNumberText;

        private void OnMouseDown()
        {
            Debug.Log("OnMouseDown " + name);
        }

        public void SetNumber(int i)
        {
            SlotNumberText.text = i.ToString();
        }

        public void UpdateGraphic(InventoryItem item)
        {
            // Clean up previous models
            for (int i = 0; i < GraphicsContainer.transform.childCount; i++)
            {
                Destroy(GraphicsContainer.transform.GetChild(i));
            }

            // Create model
            var obj = Instantiate(item.ItemData.ItemPrefab, GraphicsContainer.transform);

            foreach (var renderer in obj.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.gameObject.layer = LayerMask.NameToLayer("UI");
            }
        }
    }
}