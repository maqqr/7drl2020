using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator
{
    public class FloatingDamageValue : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI Text;

        private Vector3 speed;

        public void SetText(string text, Color color)
        {
            Text.text = text;
            Text.color = color;
            Text.alpha = 1f;

            speed = new Vector3(0f, 2f, 0f) + Random.onUnitSphere * 1f;
        }

        private void FixedUpdate()
        {
            speed *= 0.95f;
        }

        void Update()
        {
            transform.Translate(speed * Time.deltaTime);

            Text.alpha = Mathf.Max(0f, Text.alpha - Time.deltaTime);
            if (Text.alpha <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}