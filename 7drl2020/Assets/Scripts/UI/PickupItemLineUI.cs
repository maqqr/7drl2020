using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Verminator
{
    public class PickupItemLineUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Image BackgroundImage;

        public TMPro.TextMeshProUGUI ItemText;

        public event System.Action<PointerEventData> OnClick;
        public event System.Action<PointerEventData> OnEnter;
        public event System.Action<PointerEventData> OnExit;

        public bool IsMouseOver = false;

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("Mouse enter");
            OnEnter?.Invoke(eventData);

            eventData.eligibleForClick = true;
            IsMouseOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("Mouse exit");
            OnExit?.Invoke(eventData);
            IsMouseOver = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //Debug.Log("Mouse click");
            //OnClick?.Invoke(eventData);
        }

        public void Click()
        {
            //Debug.Log("Mouse click");
            //OnClick?.Invoke(null);
        }

        public void SimulateClick()
        {
            Debug.Log("Mouse click");
            OnClick?.Invoke(null);
        }
    }
}