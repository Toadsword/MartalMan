using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using NetworkLobby;

public class NetworkLobbyHook : LobbyHook {

	public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager,
        GameObject lobbyPlayer, GameObject gamePlayer)
    {
        LobbyPlayer lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
        LocalPlayerController localPlayer = gamePlayer.GetComponent<LocalPlayerController>();

        localPlayer.SetupBeginGame();

        localPlayer.playerName = lobby.playerName;

        if (lobby.playerTeam == LobbyPlayer.PlayerTeam.BLUE)
            localPlayer.team = LobbyPlayer.PlayerTeam.BLUE;
        else
            localPlayer.team = LobbyPlayer.PlayerTeam.RED;

        localPlayer.skin = lobby.playerSkin;
    }
}
