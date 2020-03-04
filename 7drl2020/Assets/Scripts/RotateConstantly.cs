using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Verminator
{
    public class RotateConstantly : MonoBehaviour
    {
        public Vector3 Axis;
        public float Speed;

        void Update()
        {
            transform.Rotate(Axis, Speed * Time.deltaTime);
        }
    }
}