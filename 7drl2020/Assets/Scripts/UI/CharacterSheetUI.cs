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

        public TMPro.TextMeshProUGUI HpText;
        public TMPro.TextMeshProUGUI MpText;

        public TMPro.TextMeshProUGUI StrengthText;
        public TMPro.TextMeshProUGUI IntelligenceText;
        public TMPro.TextMeshProUGUI MeleeText;
        public TMPro.TextMeshProUGUI RangedText;

        public TMPro.TextMeshProUGUI LevelText;

        public TMPro.TextMeshProUGUI SlashingText;
        public TMPro.TextMeshProUGUI BluntText;
        public TMPro.TextMeshProUGUI PiercingText;
        public TMPro.TextMeshProUGUI MagicText;

        public TMPro.TextMeshProUGUI DescWindowTitle;
        public TMPro.TextMeshProUGUI DescWindowText;
        public GameObject DescWindow;

        public GameObject LevelUpButtons;


        public System.Action<int> StatIncreased;

        public void StatIncreaseClicked(int index)
        {
            Debug.Log("Stat button " + index + " clicked");
            StatIncreased?.Invoke(index);
        }
    }
}