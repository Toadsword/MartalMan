
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public static SceneManagement _instance;

    const float MAX_FADE_OPACITY = 1.0f;

    public enum Scenes
    {
        MENU,
        GAME
    }

    [Header("Fade")]
    [SerializeField] Image blackFadeObject;
    [SerializeField] float fadeDuration = 2.0f;
    private float fadeTimer;
    private float faceOpacity = 1.0f;
    
    [SerializeField] private string sceneNameMenu;
    [SerializeField] private string sceneNameGame;

    private bool isChangingScene = false;
    private bool isLoadingScene = false;
    private Scenes nextScene;
    private Scenes oldScene;
    AsyncOperation asyncScene;

    private void Awake()
    {
        _instance = this;
    }

    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (isChangingScene)
        {
            if (!Utility.IsOver(fadeTimer))
            {
                blackFadeObject.gameObject.SetActive(true);
                FadeAnimation(true);
            }
            else if (!isLoadingScene)
            {
                asyncScene = SceneManager.LoadSceneAsync(GetSceneName(nextScene));
                isLoadingScene = true;
            }
            else if (asyncScene.isDone)
            {
                Resources.UnloadUnusedAssets();
                Debug.Log("SetupScene : " + GetSceneName(nextScene));
                GameManager._instance.SetupScene(nextScene);
                isChangingScene = false;
                isLoadingScene = false;
                fadeTimer = Utility.StartTimer(fadeDuration);
            }
        }
        else if (!Utility.IsOver(fadeTimer))
        {
            FadeAnimation(false);
        }
        else
        {
            blackFadeObject.gameObject.SetActive(false);
        }
    }

    public void ChangeScene(Scenes scene)
    {
        if (!isChangingScene)
        {
            nextScene = scene;
            isChangingScene = true;
            fadeTimer = Utility.StartTimer(fadeDuration);
        }
    }

    private string GetSceneName(Scenes scene)
    {
        string sceneName = "";
        switch (scene)
        {
            case Scenes.MENU:
                sceneName = sceneNameMenu;
                break;
            case Scenes.GAME:
                sceneName = sceneNameGame;
                break;
        }
        return sceneName;
    }

    private void FadeAnimation(bool DoAddOpacity)
    {
        Color color = blackFadeObject.color;
        if (DoAddOpacity)
        {
            color.a = MAX_FADE_OPACITY - (MAX_FADE_OPACITY * Utility.GetTimerRemainingTime(fadeTimer) / fadeDuration);
        }
        else
        {
            color.a = MAX_FADE_OPACITY * Utility.GetTimerRemainingTime(fadeTimer) / fadeDuration;
        }
        blackFadeObject.color = color;
    }

    #region Buttons
    public void StartBtn()
    {
        ChangeScene(SceneManagement.Scenes.GAME);
    }

    public void MenuBtn()
    {
        ChangeScene(SceneManagement.Scenes.MENU);
    }

    public void QuitBtn()
    {
        Application.Quit();
    }
    #endregion
}
