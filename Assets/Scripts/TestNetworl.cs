using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TestNetworl : NetworkBehaviour {

    public int index = 0;

	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.H) && isLocalPlayer)
        {
            SendTest();
        }
	}

    private void SendTest()
    {
        IncrementIndex(1);
        CmdCommandTest();
    }

    [Command]
    private void CmdCommandTest()
    {
        RpcTest();
    }

    [ClientRpc]
    private void RpcTest()
    {
        if(!isLocalPlayer)
            IncrementIndex(100);
    }

    private void IncrementIndex(int unit)
    {
        index += unit;
        Debug.LogError(index);
    }
}
