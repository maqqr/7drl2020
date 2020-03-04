using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator
{
    /// <summary>
    /// Selects one child object and deletes the rest.
    /// </summary>
    public class RandomSpawn : MonoBehaviour
    {
        public int SpawnChance = 50;

        void Awake()
        {
            if (Random.Range(0, 100) >= SpawnChance)
            {
                GameObject.Destroy(gameObject);
            }
        }
    }
}