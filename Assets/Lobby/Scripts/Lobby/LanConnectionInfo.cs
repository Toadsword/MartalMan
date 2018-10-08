using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LanConnectionInfo {

    public string ipAddress;
    public int port;
    public string name;
    public int currentSize;
    public int maxSize;

    public LanConnectionInfo(string fromAddress, string data)
    {
        ipAddress = fromAddress.Substring(fromAddress.LastIndexOf(":") + 1, fromAddress.Length - (fromAddress.LastIndexOf(":") + 1));
        string portText = data.Substring(data.LastIndexOf(":") + 1, data.Length - (data.LastIndexOf(":") + 1));
        port = 7777;
        int.TryParse(portText, out port);
        name = "local";
        currentSize = 0;
        maxSize = 8;
    }
}
