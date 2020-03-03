using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator
{
    public class CameraController : MonoBehaviour
    {
        public Transform FollowTransform;

        public Vector3 Offset = new Vector3(11.82f, 8.05f, 12.44f);

        void Update()
        {
            if (FollowTransform == null)
            {
                return;
            }

            transform.position = new Vector3(FollowTransform.position.x, 0f, FollowTransform.position.z) + Offset;
        }
    }
}