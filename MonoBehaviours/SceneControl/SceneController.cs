using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// This script exists in the Persistent scene and manages the content
// based scene's loading.  It works on a principle that the
// Persistent scene will be loaded first, then it loads the scenes that
// contain the player and other visual elements when they are needed.
// At the same time it will unload the scenes that are not needed when
// the player leaves them.
public class SceneController : MonoBehaviour
{
    public event Action BeforeSceneUnload;          // Event delegate that is called just before a scene is unloaded.
    public event Action AfterSceneLoad;             // Event delegate that is called just after a scene is loaded.

    public CanvasGroup faderCanvasGroup;            // The CanvasGroup that controls the Image used for fading to black.
    public float fadeDuration = 1f;                 // How long it should take to fade to and from black.
    public string startingSceneName = "MainScene";
    // The name of the scene that should be loaded first.

    private bool isFading;                          // Flag used to determine if the Image is currently fading to or from black.

    private IEnumerator Start()
    {
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        // Set the initial alpha to start off with a black screen.
        faderCanvasGroup.alpha = 1f;
        // Start the first scene loading and wait for it to finish.
        yield return StartCoroutine(LoadScenes(new string[] { startingSceneName }));
        // Once the scene is finished loading, start fading in.
        StartCoroutine(Fade(0f));
    }

    public void FadeAndLoadScene(string[] sceneNamesToLoad, string[] sceneNamesToUnload)
    {
        if (!isFading)
        {
            StartCoroutine(FadeAndSwitchScenes(sceneNamesToLoad, sceneNamesToUnload));
        }
    }

    private IEnumerator FadeAndSwitchScenes (string[] sceneNamesToLoad, string[] sceneNamesToUnload)
    {
        // Start fading to black and wait for it to finish before continuing.
        yield return StartCoroutine (Fade (1f));
        Time.timeScale = 0;

        if (BeforeSceneUnload != null)
            BeforeSceneUnload ();

        yield return StartCoroutine (UnloadScenes(sceneNamesToUnload));
        yield return StartCoroutine (LoadScenes(sceneNamesToLoad));

        if (AfterSceneLoad != null)
            AfterSceneLoad ();

        Time.timeScale = 1;
        // Start fading back in and wait for it to finish before exiting the function.
        yield return StartCoroutine (Fade (0f));

    }

    private IEnumerator LoadScenes (string[] sceneNames)
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

    public IEnumerator Fade (float finalAlpha)
    {
        // Set the fading flag to true so the FadeAndSwitchScenes coroutine won't be called again.
        isFading = true;
        // Make sure the CanvasGroup blocks raycasts into the scene so no more input can be accepted.
        faderCanvasGroup.blocksRaycasts = true;
        // Calculate how fast the CanvasGroup should fade based on it's current alpha, it's final alpha and how long it has to change between the two.
        float fadeSpeed = Mathf.Abs (faderCanvasGroup.alpha - finalAlpha) / fadeDuration;
        // While the CanvasGroup hasn't reached the final alpha yet...
        while (!Mathf.Approximately (faderCanvasGroup.alpha, finalAlpha))
        {
            faderCanvasGroup.alpha = Mathf.MoveTowards (faderCanvasGroup.alpha, finalAlpha,
                fadeSpeed * Time.deltaTime);
            // Wait for a frame then continue.
            yield return null;
        }

        isFading = false;
        // Stop the CanvasGroup from blocking raycasts so input is no longer ignored.
        faderCanvasGroup.blocksRaycasts = false;
    }
}
