using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamDeathmatch
{
    public static int team1Score, team2Score, numOfTeam1, numOfTeam2;
    public static int maxScore = 25;

    public static void StartMatch()
    {
        team1Score = 0;
        team2Score = 0;
        ServerSend.GameInformation(team1Score, team2Score);
    }

    public static void AddScore(Player _player)
    {
        if (_player.teamNum == 0)
        {
            team1Score++;
            if(team1Score >= maxScore)
            {
                RestartMatch();
            }
        }
        else
        {
            team2Score++;
            if (team2Score >= maxScore)
            {
                RestartMatch();
            }
        }
        ServerSend.GameInformation(team1Score, team2Score);
    }

    public static void RestartMatch()
    {
        Debug.Log("Game Done\nRestarting...");

        foreach (int i in Server.clients.Keys)
        {
            Player _player = Server.clients[i].player;
            if (_player != null)
            {
                _player.controller.enabled = false;
                if (_player.teamNum == 0)
                    _player.transform.position = Constants.team1Spawnpoints[Random.Range(0, 4)];
                else
                    _player.transform.position = Constants.team2Spawnpoints[Random.Range(0, 4)];
                ServerSend.PlayerPosition(_player);
                _player.StartCoroutine(_player.Respawn());
            }
        }
        StartMatch();
    }
}