using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Verminator
{
    public class Intro : MonoBehaviour
    {
        private float delay = 2f;

        [SerializeField] private int nextSceneNumber;

        void Start()
        {

        }

        void Update()
        {
            if (delay > 0f)
            {
                delay -= Time.deltaTime;
            }
            else if (Input.anyKeyDown)
            {
                SceneManager.LoadScene(nextSceneNumber);
            }
        }
    }
}