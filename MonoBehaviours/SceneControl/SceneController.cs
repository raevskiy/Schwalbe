using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace KopliSoft.SceneControl
{
    public class SceneController : MonoBehaviour
    {
        public event Action BeforeSceneUnload;          // Event delegate that is called just before a scene is unloaded.
        public event Action AfterSceneLoad;             // Event delegate that is called just after a scene is loaded.

        public CanvasGroup faderCanvasGroup;            // The CanvasGroup that controls the Image used for fading to black.
        public float fadeDuration = 1f;                 // How long it should take to fade to and from black.
        public string startingSceneName = "MainScene";

        private bool isFading;

        private IEnumerator Start()
        {
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            faderCanvasGroup.alpha = 1f;
            yield return StartCoroutine(LoadScenes(new string[] { startingSceneName }));
            StartCoroutine(Fade(0f));
        }

        public void FadeAndLoadScene(string[] sceneNamesToLoad, string[] sceneNamesToUnload)
        {
            if (!isFading)
            {
                StartCoroutine(FadeAndSwitchScenes(sceneNamesToLoad, sceneNamesToUnload));
            }
        }

        private IEnumerator FadeAndSwitchScenes(string[] sceneNamesToLoad, string[] sceneNamesToUnload)
        {
            yield return StartCoroutine(Fade(1f));
            Time.timeScale = 0;

            if (BeforeSceneUnload != null)
            {
                BeforeSceneUnload();
            }

            yield return StartCoroutine(UnloadScenes(sceneNamesToUnload));
            yield return StartCoroutine(LoadScenes(sceneNamesToLoad));

            if (AfterSceneLoad != null)
            {
                AfterSceneLoad();
            }

            Time.timeScale = 1;
            yield return StartCoroutine(Fade(0f));
        }

        private IEnumerator LoadScenes(string[] sceneNames)
        {
            foreach (string sceneName in sceneNames)
            {
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
        }

        private IEnumerator UnloadScenes(string[] sceneNames)
        {
            foreach (string sceneName in sceneNames)
            {
                yield return SceneManager.UnloadSceneAsync(sceneName);
            }
        }

        public IEnumerator Fade(float finalAlpha)
        {
            // FadeAndSwitchScenes coroutine won't be called again.
            isFading = true;
            // no more input can be accepted.
            faderCanvasGroup.blocksRaycasts = true;
            float fadeSpeed = Mathf.Abs(faderCanvasGroup.alpha - finalAlpha) / fadeDuration;
            while (!Mathf.Approximately(faderCanvasGroup.alpha, finalAlpha))
            {
                faderCanvasGroup.alpha = Mathf.MoveTowards(
                    faderCanvasGroup.alpha,
                    finalAlpha,
                    fadeSpeed * Time.deltaTime);
                yield return null;
            }

            isFading = false;
            // input is no longer ignored.
            faderCanvasGroup.blocksRaycasts = false;
        }
    }
}
