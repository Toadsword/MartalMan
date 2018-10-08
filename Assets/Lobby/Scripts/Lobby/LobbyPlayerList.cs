using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace NetworkLobby
{
    //List of players in the lobby
    public class LobbyPlayerList : MonoBehaviour
    {

        public static LobbyPlayerList _instance = null;

        public RectTransform blueTeamListContentTransform;
        public RectTransform redTeamListContentTransform;
        public Transform changeTeamButtonRowRed;
        public Transform changeTeamButtonRowBlue;

        protected VerticalLayoutGroup _layoutRed, _layoutBlue;
        protected List<LobbyPlayer> _players = new List<LobbyPlayer>();

        public void OnEnable()
        {
            _instance = this;
            _layoutRed = redTeamListContentTransform.GetComponent<VerticalLayoutGroup>();
            _layoutBlue = blueTeamListContentTransform.GetComponent<VerticalLayoutGroup>();
        }

        void Update()
        {
            //this dirty the layout to force it to recompute evryframe (a sync problem between client/server
            //sometime to child being assigned before layout was enabled/init, leading to broken layouting)
            
            if(_layoutRed)
               _layoutRed.childAlignment = Time.frameCount % 2 == 0 ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
            if (_layoutBlue)
                _layoutBlue.childAlignment = Time.frameCount % 2 == 0 ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
        }

        public void AddPlayer(LobbyPlayer player)
        {
            if (_players.Contains(player))
                return;

            //Define base team when entering lobby
            int moreInRed = 0;
            foreach (LobbyPlayer p in _players)
            {
                if(p.playerTeam == LobbyPlayer.PlayerTeam.RED)
                {
                    moreInRed++;
                }
                else
                {
                    moreInRed--;
                }
            }

            Debug.Log(moreInRed);
            if(moreInRed >= 0)
                player.playerTeam = LobbyPlayer.PlayerTeam.BLUE;
            else
                player.playerTeam = LobbyPlayer.PlayerTeam.RED;

            if (player.playerTeam == LobbyPlayer.PlayerTeam.BLUE)
                player.transform.SetParent(blueTeamListContentTransform, false);
            else
                player.transform.SetParent(redTeamListContentTransform, false);

            _players.Add(player);

            changeTeamButtonRowRed.SetAsLastSibling();
            changeTeamButtonRowBlue.SetAsLastSibling();

            PlayerListModified();
        }
        
        public void PlayerChangeTeam(LobbyPlayer player)
        {
            if (player.playerTeam == LobbyPlayer.PlayerTeam.BLUE)
                player.transform.SetParent(blueTeamListContentTransform, false);
            else
                player.transform.SetParent(redTeamListContentTransform, false);

            changeTeamButtonRowRed.SetAsLastSibling();
            changeTeamButtonRowBlue.SetAsLastSibling();
        }

        public void RemovePlayer(LobbyPlayer player)
        {
            _players.Remove(player);
            PlayerListModified();
        }

        public void PlayerListModified()
        {
            int i = 0;
            foreach (LobbyPlayer p in _players)
            {
                p.OnPlayerListChanged(i);
                ++i;
            }
        }
    }
}
