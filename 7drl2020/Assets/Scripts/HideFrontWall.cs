using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator
{
    public class HideFrontWall : MonoBehaviour
    {
        private const float angleDegrees = 45f;

        [SerializeField] private GameObject meshObject;

        void Start()
        {
            HideCheck();
        }

        public void HideCheck()
        {
            float angle = Mathf.Deg2Rad * angleDegrees;

            Vector3 hideDirection = new Vector3(Mathf.Cos(angle), 0.0f, Mathf.Sin(angle));
            Vector3 forwardInWorldSpace = transform.forward;

            float angleBetween = Vector3.Angle(hideDirection, forwardInWorldSpace);

            meshObject.SetActive(angleBetween < 90f);
        }

        //public void OnDrawGizmos()
        //{
        //    void Arrow(Vector3 from, Vector3 to, Color color)
        //    {
        //        Gizmos.color = color;
        //        Gizmos.DrawLine(from, to);
        //        Gizmos.DrawWireSphere(to, 0.05f);
        //    }

        //    float angle = Mathf.Deg2Rad * angleDegrees;
        //    Vector3 hideDirection = new Vector3(Mathf.Cos(angle), 0.0f, Mathf.Sin(angle));
        //    Vector3 forwardInWorldSpace = transform.forward;

        //    var start = transform.position + new Vector3(0.0f, 0.5f, 0.0f);

        //    Arrow(start, start + 0.2f * transform.forward, Color.green);

        //    Arrow(start, start + 0.3f * hideDirection, Color.red);
        //}
    }
}