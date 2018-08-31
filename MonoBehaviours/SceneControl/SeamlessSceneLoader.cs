using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KopliSoft.SceneControl
{
    public class SeamlessSceneLoader : MonoBehaviour
    {
        public string sceneName;
        private bool loaded;

        private void OnTriggerEnter(Collider other)
        {
            if (!loaded)
            {
                loaded = true;
                if (!SceneManager.GetSceneByName(sceneName).isLoaded)
                {
                    StartCoroutine(DoLoadScene());
                }
            }
        }

        IEnumerator DoLoadScene()
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }

        public void Unload()
        {
            if (loaded)
            {
                loaded = false;
                if (SceneManager.GetSceneByName(sceneName).isLoaded)
                {
                    StartCoroutine(DoUnloadScene());
                }
            }
        }

        IEnumerator DoUnloadScene()
        {
            yield return SceneManager.UnloadSceneAsync(sceneName);
        }
    }
}
