using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    /// <summary>Sends a packet to a client via TCP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendTCPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    /// <summary>Sends a packet to a client via UDP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendUDPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    /// <summary>Sends a packet to all clients via TCP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(_packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via TCP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
    }

    /// <summary>Sends a packet to all clients via UDP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendUDPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(_packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via UDP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
    }

    #region Packets
    /// <summary>Sends a welcome message to the given client.</summary>
    /// <param name="_toClient">The client to send the packet to.</param>
    /// <param name="_msg">The message to send.</param>
    public static void Welcome(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.welcome))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>Tells a client to spawn a player.</summary>
    /// <param name="_toClient">The client that should spawn the player.</param>
    /// <param name="_player">The player to spawn.</param>
    public static void SpawnPlayer(int _toClient, Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>Sends a player's updated position to all clients.</summary>
    /// <param name="_player">The player whose position to update.</param>
    public static void PlayerPosition(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.position);

            SendUDPDataToAll(_packet);
        }
    }

    /// <summary>Sends a player's updated rotation to all clients except to himself (to avoid overwriting the local player's rotation).</summary>
    /// <param name="_player">The player whose rotation to update.</param>
    public static void PlayerRotation(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.rotation);

            SendUDPDataToAll(_player.id, _packet);
        }
    }

    public static void PlayerDisconnected(int _playerId)
    {
        for(int i = 0; i < Server.clients.Count; i++)
        {
            if(_playerId == Server.clients[i].player.id)
            {
                if(Server.clients[i].player.teamNum == 0)
                {
                    TeamDeathmatch.numOfTeam1--;
                }
                else
                {
                    TeamDeathmatch.numOfTeam2--;
                }
            }
        }
        using (Packet _packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            _packet.Write(_playerId);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerHealth(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerHealth))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.health);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerRespawned(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRespawned))
        {
            _packet.Write(_player.id);
            SendTCPDataToAll(_packet);
        }
    }

    public static void GameInformation(int team1, int team2)
    {
        using (Packet _packet = new Packet((int)ServerPackets.gameInformation))
        {
            _packet.Write(team1);
            _packet.Write(team2);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerActiveWeapon(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerActiveWeapon))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.selectedWeapon);

            SendTCPDataToAll(_packet);
        }
    }

    public static void CurrentAmmo(int _toClient, Player _player)
    {
        using(Packet _packet = new Packet((int)ServerPackets.playerAmmoCount))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.primary.GetComponent<Gun>().ammoCount);
            _packet.Write(_player.primary.GetComponent<Gun>().maxAmmo);
            _packet.Write(_player.secondary.GetComponent<Gun>().ammoCount);
            _packet.Write(_player.secondary.GetComponent<Gun>().maxAmmo);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void MuzzleFlash(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playMuzzleFlash))
        {
            if (_player.selectedWeapon == 0) {
                _packet.Write(_player.primaryMuzzleFlash.transform.position);
                _packet.Write(_player.primaryMuzzleFlash.transform.rotation);
            }
            else
            {
                _packet.Write(_player.secondaryMuzzleFlash.transform.position);
                _packet.Write(_player.secondaryMuzzleFlash.transform.rotation);
            }
            SendUDPDataToAll(_packet);
        }
    }

    public static void PlayBlood(Vector3 _pos, Quaternion _rot)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playBlood))
        {
            _packet.Write(_pos);
            _packet.Write(_rot);
            SendUDPDataToAll(_packet);
        }
    }

    public static void Reload(int _id, string _gun)
    {
        using (Packet _packet = new Packet((int)ServerPackets.reload))
        {
            _packet.Write(_id);
            _packet.Write(_gun);

            SendTCPDataToAll(_packet);
        }
    }
    #endregion
}