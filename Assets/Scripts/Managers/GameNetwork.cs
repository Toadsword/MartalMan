using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityEngine.Networking
{
    public class GameNetwork : NetworkManager
    {
        PlayerController clientPlayer;

        public static GameNetwork _instance;

        private void Awake()
        {
            if (!_instance)
                _instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            clientPlayer = null;
        }

        public override void OnServerConnect(NetworkConnection conn)
        {
            base.OnServerConnect(conn);
            Debug.Log("ServerConnect");
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);
            Debug.Log("ServerDisconnect");
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
            Debug.Log("ClientConnect");
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            Debug.Log("ClientConnect");
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            base.OnServerAddPlayer(conn, playerControllerId);
            Debug.Log("AddPlayerControllerId : " + playerControllerId);
        }

        public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
        {
            base.OnServerRemovePlayer(conn, player);
            Debug.Log("RemovePlayer : " + player.playerControllerId);
        }
        
    }
}
