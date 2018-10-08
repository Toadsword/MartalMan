using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using System.Collections;

namespace NetworkLobby
{
    public class LobbyServerEntry : MonoBehaviour 
    {
        public Text serverInfoText;
        public Text slotInfo;
        public Button joinButton;
        public LanConnectionInfo lanMatch;
        public LobbyManager lobbyManager;

        public void Populate(MatchInfoSnapshot match, LobbyManager lobbyManager, Color c)
		{
            serverInfoText.text = match.name;

            slotInfo.text = match.currentSize.ToString() + "/" + match.maxSize.ToString(); ;

            NetworkID networkID = match.networkId;

            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(() => { JoinMatch(networkID, lobbyManager); });

            GetComponent<Image>().color = c;
        }

        public void Populate(LanConnectionInfo match, LobbyManager lobbyManager, Color c)
        {
            serverInfoText.text = match.name;

            slotInfo.text = match.currentSize.ToString() + "/" + match.maxSize.ToString(); ;

            lanMatch = match;
            this.lobbyManager = lobbyManager;

            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(JoinMatch);

            GetComponent<Image>().color = c;
        }


        void JoinMatch(NetworkID networkID, LobbyManager lobbyManager)
        {
			lobbyManager.matchMaker.JoinMatch(networkID, "", "", "", 0, 0, lobbyManager.OnMatchJoined);
			lobbyManager.backDelegate = lobbyManager.StopClientClbk;
            lobbyManager._isMatchmaking = true;
            lobbyManager.DisplayIsConnecting();
        }

        void JoinMatch()
        {
            lobbyManager.ChangeTo(lobbyManager.lobbyPanel);

            lobbyManager.networkAddress = lanMatch.ipAddress;
            lobbyManager.StartClient();

            lobbyManager.backDelegate = lobbyManager.StopClientClbk;
            lobbyManager.DisplayIsConnecting();

            lobbyManager.SetServerInfo("Connecting...", lobbyManager.networkAddress);
            lobbyManager.GetComponent<LobbyNetworkDiscovery>().StopBroadcast();
        }
    }
}