using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Data
{
    public static string status;
    public static string ipAddress;

    public static string player1;
    public static string player2;
    public static string localName;
    public static string joinCode;

    public static Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();

    public static void AddPlayerName(ulong clientId, string name)
    {
        if (!playerNames.ContainsKey(clientId))
        {
            playerNames.Add(clientId, name);
        }
        else
        {
            playerNames[clientId] = name;
        }
    }

    public static void AddPlayerNames(Dictionary<ulong, string> names)
    {
        foreach (var name in names)
        {
            if (!playerNames.ContainsKey(name.Key))
            {
                playerNames.Add(name.Key, name.Value);
            }
            else
            {
                playerNames[name.Key] = name.Value;
            }
        }
    }
}
