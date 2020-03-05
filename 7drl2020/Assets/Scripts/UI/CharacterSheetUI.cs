using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator
{
    public class CharacterSheetUI : MonoBehaviour
    {
        public GameObject ItemList;

        public GameObject InteractionWindow;

        public InteractionButton Equip;
        public InteractionButton Drop;
        public InteractionButton Eat;

        public TMPro.TextMeshProUGUI EquipmentText;
    }
}