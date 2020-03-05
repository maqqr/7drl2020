using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Verminator
{
    public class ItemLineUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public GameObject Underline;
        public TMPro.TextMeshProUGUI ItemText;

        private bool isSelected = false;
        public event System.Action<PointerEventData> OnClick;

        public event System.Action<PointerEventData> OnEnter;
        public event System.Action<PointerEventData> OnExit;

        private void Start()
        {
            Underline.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            //Debug.Log("Mouse enter");
            Underline.SetActive(true);
            OnEnter?.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //Debug.Log("Mouse exit");
            if (!isSelected)
            {
                Underline.SetActive(false);
            }
            OnExit?.Invoke(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //Debug.Log("Mouse click");
            OnClick?.Invoke(eventData);
        }

        public void SetSelected(bool isSelected)
        {
            this.isSelected = isSelected;
            Underline.SetActive(isSelected);
        }
    }
}