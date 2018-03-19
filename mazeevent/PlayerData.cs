using Rocket.Unturned.Player;
using Steamworks;
using System;
using UnityEngine;

namespace datathegenius.mazeevent
{
    public class PlayerData
    {
        string SteamID;
        Vector3 playerPosition;
        public Boolean isPromoted;

        public PlayerData()
        {
            SteamID = "-1";
            playerPosition = new Vector3(0, 0, 0);
            isPromoted = false;
        }
        public PlayerData(string playerID, Vector3 position)
        {
            SteamID = playerID;
            playerPosition = position;
        }

        public string getSteamID()
        {
            return SteamID;
        }

        public string getName()
        {
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(new CSteamID(Convert.ToUInt64(SteamID)));

            return player.CharacterName;
        }
        public Vector3 getPlayerPosition()
        {
            return playerPosition;
        }
    }
}
