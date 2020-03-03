using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator
{
    public class Item : MonoBehaviour
    {
        [HideInInspector] public Data.ItemData Data; // Static item data shared by all items of this type

        public Vector2Int Position; // Item's position in tile coordinates
    }
}