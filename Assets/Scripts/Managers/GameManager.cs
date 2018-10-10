using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NetworkLobby;

public class GameManager : MonoBehaviour {

    public LocalPlayerController localPlayer;

    [Header("Public Vars")]    
    public bool gamePaused = false;

    // Menu gestion
    public Selectable[] menuBtns;
    public int selectedIndex = 0;
    public bool isInMenu = false;
    
    public GameObject pauseCanvas;

    public static GameManager _instance;

    private void Awake()
    {
        if (_instance)
            Destroy(gameObject);
        else
            _instance = this;
    }

    // Use this for initialization
    void Start ()
    {
        DontDestroyOnLoad(gameObject);
        
        gamePaused = false;
        isInMenu = false;

        GameObject tempPauseCanvas = GameObject.Find("PauseCanvas");
        if (tempPauseCanvas)
            pauseCanvas = tempPauseCanvas;
    }
	
    public void SetupScene(SceneManagement.Scenes scene)
    {
        GameObject tempPauseCanvas;
        switch (scene)
        {
            case SceneManagement.Scenes.MENU:
               Transform panelMenu = GameObject.Find("MainMenu").transform;
                if (panelMenu)
                    SetupMenuBtns(panelMenu, true);
                
                break;
            case SceneManagement.Scenes.GAME:
                isInMenu = false;
                gamePaused = false;

                tempPauseCanvas = GameObject.Find("PauseCanvas");
                if (tempPauseCanvas)
                    pauseCanvas = tempPauseCanvas;
                break;
        } // End switch
    }
    

    public void PauseGame(bool doPause = true)
    {
        gamePaused = doPause;
        SetupMenuBtns(pauseCanvas.transform.GetChild(0), doPause);
    }

    private void Update()
    {
        if (GameInput.GetInputDown(GameInput.InputType.PAUSE) && pauseCanvas)
            PauseGame();

        if (isInMenu || gamePaused)
        {
            ManageNavigation();
            return;
        }
    }

    private void ManageNavigation()
    {
        if (GameInput.GetInputDown(GameInput.InputType.DOWN) || GameInput.GetInputDown(GameInput.InputType.LEFT))
            NavigateMenu(false);
        if (GameInput.GetInputDown(GameInput.InputType.UP) || GameInput.GetInputDown(GameInput.InputType.RIGHT))
            NavigateMenu(true);
        if (GameInput.GetInputDown(GameInput.InputType.JUMP) || GameInput.GetInputDown(GameInput.InputType.ATTACK))
            SubmitButtonAction();
    }

    public void NavigateMenu(bool onLeft)
    {
        if (isInMenu)
        {
            if (onLeft)
            {
                selectedIndex--;
                if (selectedIndex == -1)
                    selectedIndex = 0;
                //else
                //    SoundManager._instance.PlaySound(SoundManager.SoundList.MENU_SELECTION);
            }
            else
            {
                selectedIndex++;
                if (selectedIndex == menuBtns.Length)
                    selectedIndex = menuBtns.Length - 1;
                //else
                //    SoundManager._instance.PlaySound(SoundManager.SoundList.MENU_SELECTION);
            }
            menuBtns[selectedIndex].Select();
        }
    }

    public void SelectButton(string name)
    {
        int newIndex = 0;
        foreach(Selectable btn in menuBtns)
        {
            if (btn.name == name)
            {
                selectedIndex = newIndex;
                break;
            }
            newIndex++;
        }
        menuBtns[selectedIndex].Select();
    }

    public void SubmitButtonAction()
    {
        switch (menuBtns[selectedIndex].name)
        {
            case "ButtonPlay":
                SceneManagement._instance.StartBtn();
                break;
            case "ButtonCredits":
                GameObject creditPanelObject = GameObject.Find("Canvas").transform.Find("CreditPanel").gameObject;
                creditPanelObject.SetActive(!creditPanelObject.activeSelf);
                break;
            case "ButtonQuit":
                SceneManagement._instance.QuitBtn();
                break;
            case "BtnContinue":
                PauseGame(false);
                break;
            case "BtnMainMenu":
                SetupMenuBtns(pauseCanvas.transform.GetChild(1), true);
                break;
            case "BtnConfirmMainMenu":
                SceneManagement._instance.ChangeScene(SceneManagement.Scenes.MENU);
                break;
            case "BtnReturn":
                pauseCanvas.transform.GetChild(1).gameObject.SetActive(false);
                SetupMenuBtns(pauseCanvas.transform.GetChild(0), true);
                break;
        }
        //SoundManager._instance.PlaySound(SoundManager.SoundList.MENU_VALIDATION);
    }

    public void SetupMenuBtns(Transform panelMenu, bool activate)
    {
        panelMenu.gameObject.SetActive(activate);

        isInMenu = activate;

        selectedIndex = 0;

        if (activate)
        {
            menuBtns = panelMenu.GetComponent<MenuActions>().menuBtns;
            menuBtns[selectedIndex].Select();
        }
        else
            menuBtns = null;
    }

    public void GameFinished(LobbyPlayer.PlayerTeam teamLost)
    {
        if (localPlayer.playerTeam == teamLost)
            SoundManager._instance.PlaySound(SoundManager.SoundList.LOSE_SOUND);
        else
            SoundManager._instance.PlaySound(SoundManager.SoundList.WIN_SOUND);
    }

}

