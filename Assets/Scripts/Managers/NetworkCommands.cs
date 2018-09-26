using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkCommands : NetworkBehaviour {

    public static NetworkCommands _instance;

    private void Awake()
    {
        if (!_instance)
            _instance = this;
        else
            Destroy(gameObject);
    }
    /*
    public void PropellPlayers(LocalPlayerController[] players)
    {
        foreach (LocalPlayerController player in players)
        {
            PropellPlayers(player);
        }
    }

    public void PropellPlayers(LocalPlayerController player)
    {
        player.GetHit(new Vector2(1.0f, 1.0f), 600);
    }
    */
}
