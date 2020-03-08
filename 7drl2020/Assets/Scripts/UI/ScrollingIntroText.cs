using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingIntroText : MonoBehaviour
{
    RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        rect.localPosition += new Vector3(0f, 30f * Time.deltaTime, 0f);
    }
}
