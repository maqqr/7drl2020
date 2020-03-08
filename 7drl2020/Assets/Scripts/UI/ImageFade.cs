using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageFade : MonoBehaviour
{
    Image img;
    public float FadeSpeed = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        img = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (img.color.a > 0f)
        {
            img.color = new Color(0, 0, 0, img.color.a - FadeSpeed * Time.deltaTime);
        }
    }
}
