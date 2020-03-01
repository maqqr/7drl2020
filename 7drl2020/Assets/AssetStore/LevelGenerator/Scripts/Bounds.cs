using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LevelGenerator.Scripts
{
    public class Bounds : MonoBehaviour
    {
        [SerializeField]
        private Collider[] extraColliders;

        public IEnumerable<Collider> Colliders => GetComponentsInChildren<Collider>().Concat(extraColliders.Where(c => c != null));
    }
}
