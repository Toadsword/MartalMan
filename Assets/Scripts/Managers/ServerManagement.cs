using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerManagement : NetworkBehaviour {


    /*** OLD VERSION, NOW USING LOBBYPLAY/LOBBYPLAYERLIST, ETC ***/
    /*
    public enum PLAYER_TEAM
    {
        NO_TEAM,
        RED, 
        BLUE
    }

    private short lastId = 0;

    [Header("Public vars")]
    public List<LocalPlayerController> players;

    public static ServerManagement _instance;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        if(isServer)
        {
            lastId = 0;
        }
        players = new List<LocalPlayerController>();
    }

    public void SetupNewPlayerNetwork(short playerId, LocalPlayerController playerObject)
    {
        PLAYER_TEAM team = GetTeamWithLessPlayer();
        if (team == PLAYER_TEAM.NO_TEAM)
        {
            float rand = Random.Range(0.0f, 1.0f);
            if (rand < 0.5) team = PLAYER_TEAM.BLUE;
            else team = PLAYER_TEAM.RED;
        }

        players.Add(playerObject);
        playerObject.playerId = lastId;
        lastId++;
        playerObject.RpcSetTeam(team);
        Debug.LogError("ADDING NEW PLAYER; IS SERVER : " + isServer);
    }

    private PLAYER_TEAM GetTeamWithLessPlayer()
    {
        short blue = 0, red = 0;
        foreach (LocalPlayerController player in players)
        {
            if (player.team== PLAYER_TEAM.RED) red++;
            if (player.team == PLAYER_TEAM.BLUE) blue++;
        }

        if (red == blue)
            return PLAYER_TEAM.NO_TEAM;
        else if (red < blue)
            return PLAYER_TEAM.RED;
        else
            return PLAYER_TEAM.BLUE;
    }

    public void TeamWin(PLAYER_TEAM team)
    {
        // :)
        Debug.LogError(team + " TEAM WIN");
    }

    public LocalPlayerController GetPlayerObjFromId(short playerId)
    {
        foreach (LocalPlayerController player in players)
        {
            if(player.playerId == playerId)
            {
                return player;
            }
        }
        return null;
    }

    public void UpdateCurrentPlayers()
    {
        LocalPlayerController[] foundPlayers = FindObjectsOfType<LocalPlayerController>();
        players.Clear();
        foreach(LocalPlayerController player in foundPlayers)
        {
            players.Add(player);
        }
    }

    [ClientRpc]
    public void RpcRemovePlayer(short playerId)
    {
        LocalPlayerController playerToRemove = null;
        foreach (LocalPlayerController player in players)
        {
            if (player.playerId == playerId)
            {
                playerToRemove = player;
                break;
            }
        }

        if (playerToRemove != null)
            players.Remove(playerToRemove);
    }
    */
}
