using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator
{
    public class Item : MonoBehaviour
    {
        [HideInInspector] public Data.ItemData Data; // Static item data shared by all creature of this type

        public Vector2Int Position; // Item's position in tile coordinates

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}