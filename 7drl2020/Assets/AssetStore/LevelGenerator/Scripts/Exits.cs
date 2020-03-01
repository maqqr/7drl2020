using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LevelGenerator.Scripts
{
    public class Exits : MonoBehaviour
    {
        [SerializeField]
        private Transform[] extraExits;

        public IEnumerable<Transform> ExitSpots => GetComponentsInChildren<Transform>().Where(t => t != transform).Concat(extraExits.Where(e => e != null));
    }
}
