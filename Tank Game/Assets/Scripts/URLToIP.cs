using System.Net;
using UnityEngine.Networking;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class URLToIP : MonoBehaviour
{
    public string URL;
    [SerializeField] UnityTransport transport;

    private void Start()
    {
        if (URL == "") { return; }
        if (transport == null) { return; }

        IPAddress ip = System.Net.Dns.GetHostEntry(URL).AddressList[0];
        Debug.Log($"{URL} => {ip}");
        transport.SetConnectionData(ip.ToString(), 7777); 
    }
}
