using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteShadersUltimate
{
    [AddComponentMenu("Sprite Shaders Ultimate/Utility/Unscaled Time SSU")]
    [ExecuteAlways]
    public class UnscaledTimeSSU : MonoBehaviour
    {
        public bool dontDestroyOnLoad;

        void Awake()
        {
            if(dontDestroyOnLoad && Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        void Update()
        {
            Shader.SetGlobalFloat("UnscaledTime", Time.unscaledTime);
        }
    }
}
