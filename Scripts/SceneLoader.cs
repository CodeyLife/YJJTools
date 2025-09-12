using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : YjjSingleton<SceneLoader>
{
    public Image fadeImage;
    [ShowInInspector]
    private string currentScene;
    public Queue<Action> AfterSceneLoad = new();
    public Queue<Action> OnLoadingAction = new();
    public bool isLoading = false;



    public void LoadScene(string name)
    {
        if (string.IsNullOrEmpty(name) || (name == currentScene && isLoading)) return;
        RemoveCurrentScene();
        currentScene = name;
        StartCoroutine(LoadAnimation(name));
    }
    [Button]
    public void RemoveCurrentScene()
    {
        if (string.IsNullOrEmpty(currentScene)) return;
        SceneManager.UnloadSceneAsync(currentScene);
        currentScene = null;
    }


    IEnumerator LoadAnimation(string name)
    {
        isLoading = true;
        float fadeTime = 0.25f;
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = Color.black.SetAlpha(0);
        yield return this.FadeIn(fadeTime, (t) =>
        {
            fadeImage.color = Color.black.SetAlpha(t);
        });
        while (OnLoadingAction.Count > 0)
        {
            OnLoadingAction.Dequeue()?.Invoke();
        }
        LoadSceneParameters config = new LoadSceneParameters(LoadSceneMode.Single, LocalPhysicsMode.None);
        AsyncOperation loader = SceneManager.LoadSceneAsync(name, config);
        //loader.allowSceneActivation = false;
        //loader.allowSceneActivation = true;
        while (!loader.isDone)
        {
            yield return null;
        }
        yield return this.FadeIn(fadeTime, (t) =>
        {
            fadeImage.color = Color.black.SetAlpha(1 - t);
        });
        while (AfterSceneLoad.Count > 0)
        {
            AfterSceneLoad.Dequeue().Invoke();
        }
        isLoading = false;
        fadeImage.gameObject.SetActive(false);
    }

    IEnumerator LoadAnimation(AsyncOperation loader)
    {
        float fadeTime = 0.25f;
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = Color.black.SetAlpha(0);
        yield return this.FadeIn(fadeTime, (t) =>
        {
            fadeImage.color = Color.black.SetAlpha(t);
        });
        loader.allowSceneActivation = true;
        while (!loader.isDone)
        {
            yield return null;
        }
        yield return this.FadeIn(fadeTime, (t) =>
        {
            fadeImage.color = Color.black.SetAlpha(1 - t);
        });
        fadeImage.gameObject.SetActive(false);
    }
}
