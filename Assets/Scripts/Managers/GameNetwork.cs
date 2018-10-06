using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityEngine.Networking
{
    public class GameNetwork : NetworkManager
    {
        public NetworkDiscovery discovery;

        public static GameNetwork _instance;

        private void Awake()
        {
            if (!_instance)
                _instance = this;
            else
                Destroy(gameObject);
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
            //base.OnServerAddPlayer(conn, playerControllerId);
            GameObject newPlayer = Instantiate(playerPrefab, playerPrefab.transform.position, playerPrefab.transform.rotation);
            NetworkServer.AddPlayerForConnection(conn, newPlayer, playerControllerId);

            //ServerManagement._instance.SetupNewPlayerNetwork(playerControllerId, newPlayer.GetComponent<LocalPlayerController>());

            Debug.Log("AddPlayerControllerId : " + playerControllerId);
        }

        public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
        {
            base.OnServerRemovePlayer(conn, player);
            //ServerManagement._instance.RpcRemovePlayer(player.playerControllerId);
            Debug.Log("RemovePlayer : " + player.playerControllerId);
        }

        /* BROADCASTING */
        /*
        public override void OnStartHost()
        {
            discovery.Initialize();
            discovery.StartAsServer();
        }

        public override void OnStartClient(NetworkClient client)
        {
            discovery.showGUI = false;
        }

        public override void OnStopClient()
        {
            discovery.StopBroadcast();
            discovery.showGUI = true;
        }
        */
    }
}
