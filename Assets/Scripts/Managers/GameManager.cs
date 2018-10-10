using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NetworkLobby;

public class GameManager : MonoBehaviour {

    public LocalPlayerController localPlayer;

    [Header("UI")]
    [SerializeField] Text infoText;
    [SerializeField] Text respawnText;
    [SerializeField] float timeDisplayInfoText = 3.0f;
    float timerDisplayInfoText;
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
    }

    private void Update()
    {
        if(Utility.IsOver(timerDisplayInfoText))
        {
            infoText.text = "";
        }
    }

    public void UpdateInfoText(string newText)
    {
        infoText.text = newText;
        timerDisplayInfoText = Utility.StartTimer(timeDisplayInfoText);
    }

    public void UpdateRespawnText(string newText)
    {
        respawnText.text = newText;
    }

    public void GameFinished(LobbyPlayer.PlayerTeam teamLost)
    {
        if (localPlayer.playerTeam == teamLost)
        {
            SoundManager._instance.PlaySound(SoundManager.SoundList.LOSE_SOUND);
            UpdateInfoText("Your team win the game !");
        }
        else
        {
            SoundManager._instance.PlaySound(SoundManager.SoundList.WIN_SOUND);
            UpdateInfoText("Your team lost the game...");
        }
    }

}

