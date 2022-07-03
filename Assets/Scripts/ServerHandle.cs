using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }
        Server.clients[_fromClient].SendIntoGame(_username);
    }

    public static void PlayerMovement(int _fromClient, Packet _packet)
    {
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++)
        {
            _inputs[i] = _packet.ReadBool();
        }
        Quaternion _rotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].player.SetInput(_inputs, _rotation);
    }

    public static void PlayerShoot(int _fromClient, Packet _packet)
    {
        Vector3 _shootDirection = _packet.ReadVector3();

        if (Server.clients[_fromClient].player.selectedWeapon == 0)
        {
            if (Server.clients[_fromClient].player.primary.GetComponent<Gun>().ammoCount > 0)
            {
                Server.clients[_fromClient].player.Shoot(_shootDirection);
            }

            else
            {
                Server.clients[_fromClient].player.primary.GetComponent<Gun>().Reload(Server.clients[_fromClient].player.id);
            }
        }

        if (Server.clients[_fromClient].player.selectedWeapon == 1)
        {
            if (Server.clients[_fromClient].player.secondary.GetComponent<Gun>().ammoCount > 0)
            {
                Server.clients[_fromClient].player.Shoot(_shootDirection);
            }
            else
            {
                Server.clients[_fromClient].player.secondary.GetComponent<Gun>().Reload(Server.clients[_fromClient].player.id);
            }
        }
    }

    public static void StartReload(int _fromClient, Packet _packet)
    {
        int selectedWeapon = _packet.ReadInt();
        if(selectedWeapon == 0)
            Server.clients[_fromClient].player.primary.GetComponent<Gun>().Reload(Server.clients[_fromClient].player.id);
        else if(selectedWeapon == 1)
            Server.clients[_fromClient].player.secondary.GetComponent<Gun>().Reload(Server.clients[_fromClient].player.id);
    }
}