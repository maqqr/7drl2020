﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator
{
    public class SelectOne : MonoBehaviour
    {
        void Awake()
        {
            if (transform.childCount > 0)
            {
                int randomIndex = Random.Range(0, transform.childCount);
                var selectedChild = transform.GetChild(randomIndex); // Save one from destruction
                selectedChild.SetParent(transform.parent);
                selectedChild.gameObject.SetActive(true);

                Transform[] remainingChildren = GetComponentsInChildren<Transform>(true);

                for (int i = 0; i < remainingChildren.Length; i++)
                {
                    Destroy(remainingChildren[i].gameObject);
                }
            }
        }
    }
}