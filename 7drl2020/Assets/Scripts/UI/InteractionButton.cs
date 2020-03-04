using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Verminator
{
    public class InteractionButton : MonoBehaviour, IPointerClickHandler
    {
        public System.Action<PointerEventData> OnClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log(name + " clicked!");
            OnClick?.Invoke(eventData);
        }
    }
}