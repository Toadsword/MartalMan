using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace NetworkLobby
{
    public class LobbyNetworkDiscovery : NetworkDiscovery
    {
        private float timeout = 5.0f;

        private Dictionary<LanConnectionInfo, float> lanAdresses = new Dictionary<LanConnectionInfo, float>();

        public void SearchServers()
        {
            base.Initialize();
            base.StartAsClient();
            StartCoroutine(CleanupExpiredEntries());
        }

        public void StartBroadcast()
        {
            if(base.isClient)
                StopBroadcast();
            base.Initialize();
            base.StartAsServer();
            UpdateMatchInfos();
        }

        public void StopGlobalBroadcast()
        {
            if(base.isClient || base.isServer)
                base.StopBroadcast();
            lanAdresses.Clear();
        }

        private IEnumerator CleanupExpiredEntries()
        {
            while (true)
            {
                bool changed = false;

                List<LanConnectionInfo> keys = lanAdresses.Keys.ToList();

                foreach (LanConnectionInfo key in keys)
                {
                    if (Utility.IsOver(lanAdresses[key]))
                    {
                        lanAdresses.Remove(key);
                        changed = true;
                    }
                }
                if (changed)
                    UpdateMatchInfos();

                yield return new WaitForSeconds(timeout);
            }
        }

        public override void OnReceivedBroadcast(string fromAddress, string data)
        {
            base.OnReceivedBroadcast(fromAddress, data);

            LanConnectionInfo info = new LanConnectionInfo(fromAddress, data);

            if (!lanAdresses.ContainsKey(info))
            {
                lanAdresses.Add(info, Utility.StartTimer(timeout));
                UpdateMatchInfos();
            }
            else
                lanAdresses[info] = Utility.StartTimer(timeout);
        }

        private void UpdateMatchInfos()
        {
            //Mettre à jour les entrées LAN ici.
            LobbyUpdateServerList servList = FindObjectOfType<LobbyUpdateServerList>();
            if(servList)
                servList.UpdateServerList(lanAdresses.Keys.ToList());
        }
    }
}
