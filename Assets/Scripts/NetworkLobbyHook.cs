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

        localPlayer.playerName = lobby.playerName;

        localPlayer.playerTeam = lobby.playerTeam;

        localPlayer.playerSkin = lobby.playerSkin;

        localPlayer.playerId = lobby.playerId;

        localPlayer.conn = lobby.connectionToClient;
    }
}
