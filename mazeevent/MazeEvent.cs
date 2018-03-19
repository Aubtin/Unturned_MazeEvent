using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace datathegenius.mazeevent
{
    public class MazeEvent : RocketPlugin<MazeEventConfiguration>
    {
        public static MazeEvent Instance;

        public static List<UnturnedPlayer> deadPlayerForTransport = new List<UnturnedPlayer>() { };

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    {"mazeevent_storage_given", "You've been given 4 lockers." },
                    {"mazeevent_storage_used", "You've already used your storage request." },
                    {"mazeevent_invalid_parameter", "Sorry, it looks like you didn't use the command correctly." },
                    {"mazeevent_not_joined", "You aren't in an event." },
                    {"mazeevent_already_joined_game", "{0} has already joined the game!" },
                    {"mazeevent_left", "You've left the event, thanks for playing!" },
                    {"mazeevent_not_active", "There are no active maze events!" },
                    {"mazeevent_position_set", "{0} set at {1}" },
                    {"mazeevent_player_list", "Joined Players ({0}): {1}" },
                    {"mazeevent_join_announcement", "Join the maze event! Do '/maze storage' to get lockers, and '/maze join' to join." },
                    {"mazeevent_ended_announcement", "The maze event is over, thanks for playing!" },
                    {"mazeevent_off", "Maze event shutdown." },
                    {"mazeevent_no_permission", "Sorry, you don't have the required permissions to use this." }
                };
            }
            
        }
      
        //On player revive, check if active, event active, then move them. 
        public void OnPlayerRevive(UnturnedPlayer player, Vector3 position, byte angle)
        {
            if (CommandMazeEvent.eventActive)
            {
                var tempCharacterInfoDuplicate = CommandMazeEvent.allJoinedPlayers.FirstOrDefault(item => item.getSteamID() == player.CSteamID.ToString());

                if (tempCharacterInfoDuplicate != null)
                {
                    deadPlayerForTransport.Add(player);
                    CommandMazeEvent.maxSkills(player);
                    return;
                }
            }
        }

        public void OnPlayerDisconnected(UnturnedPlayer player)
        {
            var tempCharacterInfoDuplicate = CommandMazeEvent.allJoinedPlayers.FirstOrDefault(item => item.getSteamID() == player.CSteamID.ToString());
            if (tempCharacterInfoDuplicate != null)
            {
                Rocket.Core.R.Permissions.AddPlayerToGroup("Guest", player);
                Rocket.Core.R.Permissions.RemovePlayerFromGroup("EventGroup", player);
                player.Teleport(tempCharacterInfoDuplicate.getPlayerPosition(), 0);
                CommandMazeEvent.allJoinedPlayers.Remove(tempCharacterInfoDuplicate);
                UnturnedChat.Say(player, MazeEvent.Instance.Translate("mazeevent_left"), MazeEvent.Instance.Configuration.Instance.SuccessColor);
                return;
            }
        }


    //Keep Track of Time... Make announcement.
    DateTime lastCalled = DateTime.Now;
        private void announcementManager()
        {
            if (CommandMazeEvent.eventActive && ((DateTime.Now - this.lastCalled).TotalSeconds > MazeEvent.Instance.Configuration.Instance.announcementSeconds))
            {
                lastCalled = DateTime.Now;

                UnturnedChat.Say(MazeEvent.Instance.Translate("mazeevent_join_announcement"), MazeEvent.Instance.Configuration.Instance.AnnouncementColor);

                //Heal barricades.
                repairBarricades();
            }
        }

        private void dealWithDead()
        {
            if (CommandMazeEvent.eventActive && deadPlayerForTransport.Count >= 1 && deadPlayerForTransport[0] != null)
            {
                var tempCharacterInfoDuplicate = CommandMazeEvent.allJoinedPlayers.FirstOrDefault(item => item.getSteamID() == deadPlayerForTransport[0].CSteamID.ToString());

                if (tempCharacterInfoDuplicate.isPromoted)
                    deadPlayerForTransport[0].Teleport(CommandMazeEvent.position2, CommandMazeEvent.position2Rotation);
                else
                    deadPlayerForTransport[0].Teleport(CommandMazeEvent.position1, CommandMazeEvent.position1Rotation);

                deadPlayerForTransport.Remove(deadPlayerForTransport[0]);
            }
        }

        void FixedUpdate()
        {
            announcementManager();

            dealWithDead();
        }

        protected override void Load()
        {
            UnturnedPlayerEvents.OnPlayerRevive += OnPlayerRevive;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            Instance = this;
        }

        protected override void Unload()
        {
            UnturnedPlayerEvents.OnPlayerRevive -= OnPlayerRevive;
        }

        public void repairBarricades()
        {
            Transform transform;
            int transformCount = 0;

            BarricadeRegion barricadeRegion;

            for (int k = 0; k < BarricadeManager.BarricadeRegions.GetLength(0); k++)
            {
                for (int l = 0; l < BarricadeManager.BarricadeRegions.GetLength(1); l++)
                {
                    barricadeRegion = BarricadeManager.BarricadeRegions[k, l];
                    transformCount = barricadeRegion.drops.Count;
                    for (int i = 0; i < transformCount; i++)
                    {
                        transform = barricadeRegion.drops[i].model;
                        BarricadeManager.repair(transform, 100, 1);
                    }

                }
            }
        }
    }
}
