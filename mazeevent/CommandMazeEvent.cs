using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace datathegenius.mazeevent
{
    public class CommandMazeEvent : IRocketCommand
    {
        public static Boolean eventActive = false;
        public static Vector3 position1;
        public static float position1Rotation;
        public static Vector3 position2;
        public static float position2Rotation;
        public static List<PlayerData> allJoinedPlayers = new List<PlayerData>();
        public static List<string> usedStorage = new List<string>();
//        public static List<string> usedExp = new List<string>();

        public List<string> Aliases
        {
            get
            {
                return new List<string>();
            }
        }

        public AllowedCaller AllowedCaller
        {
            get
            {
                return Rocket.API.AllowedCaller.Player; ;
            }
        }

        public string Help
        {
            get
            {
                return "Manages player maze events in-game.";

            }
        }

        public string Name
        {
            get
            {
                return "maze";
            }
        }

        public bool RunFromConsole
        {
            get { return false; }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "mazeevent.players" };
            }
        }

        public string Syntax
        {
            get
            {
                return "<mazeevent>";
            }
        }

        //       [RocketCommand("event", "Main command to start and manage events.", "<what to do>", AllowedCaller.Player)]
        public void Execute(IRocketPlayer caller, string[] parameters)
        {
            string[] command = new string[2];
            UnturnedPlayer pCaller = (UnturnedPlayer)caller;

            if (parameters.Count() == 1)
            {
                command[0] = parameters[0].ToLower();
            }
            else if(parameters.Count() == 2)
            {
                command[0] = parameters[0].ToLower();
                command[1] = parameters[1].ToLower();
            }
            else
            {
                UnturnedChat.Say(caller, MazeEvent.Instance.Translate("mazeevent_invalid_parameter"), MazeEvent.Instance.Configuration.Instance.ErrorColor);
            }

            #region Admin Commands
            if (pCaller.IsAdmin)
            {
                if (command[0] == "on")
                {
                    if (!eventActive)
                    {
                        allJoinedPlayers = new List<PlayerData>();
                        usedStorage = new List<string>();
//                        usedExp = new List<string>();

                        eventActive = true;

                        UnturnedChat.Say(caller, "Maze event started.", MazeEvent.Instance.Configuration.Instance.SuccessColor);
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "Maze event already started.", MazeEvent.Instance.Configuration.Instance.ErrorColor);
                        return;
                    }
                }

                if (command[0] == "off")
                {
                    if (eventActive)
                    {
                        eventActive = false;
                        
                        int playerCountBeforeDump = allJoinedPlayers.Count();
                        for (int x = 0; x < playerCountBeforeDump; x++)
                        {
                            Logger.Log("Inside Off Loop" + x);
                            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(new CSteamID(Convert.ToUInt64(allJoinedPlayers[x].getSteamID())));

                            Rocket.Core.R.Permissions.AddPlayerToGroup("Guest", player);
                            Rocket.Core.R.Permissions.RemovePlayerFromGroup("EventGroup", player);

                            var tempCharacterInfoDuplicate = allJoinedPlayers.FirstOrDefault(item => item.getSteamID() == player.CSteamID.ToString());

                            player.Teleport(tempCharacterInfoDuplicate.getPlayerPosition(), 0);
                   //         allJoinedPlayers.Remove(tempCharacterInfoDuplicate);
                            UnturnedChat.Say(caller, MazeEvent.Instance.Translate("mazeevent_left"), MazeEvent.Instance.Configuration.Instance.SuccessColor);
                        }
                        allJoinedPlayers = new List<PlayerData>();
                        UnturnedChat.Say(caller, MazeEvent.Instance.Translate("mazeevent_off"), MazeEvent.Instance.Configuration.Instance.SuccessColor);
                        UnturnedChat.Say(MazeEvent.Instance.Translate("mazeevent_ended_announcement"), MazeEvent.Instance.Configuration.Instance.AnnouncementColor);
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "Maze event isn't active.", MazeEvent.Instance.Configuration.Instance.ErrorColor);
                        return;
                    }
                }

                if (command[0] == "position1")
                {
                    position1 = pCaller.Position;
                    position1Rotation = pCaller.Rotation;
                    UnturnedChat.Say(caller, MazeEvent.Instance.Translate("mazeevent_position_set", "position1", position1), MazeEvent.Instance.Configuration.Instance.SuccessColor);
                    return;
                }

                if (command[0] == "position2")
                {
                    position2 = pCaller.Position;
                    position2Rotation = pCaller.Rotation;
                    UnturnedChat.Say(caller, MazeEvent.Instance.Translate("mazeevent_position_set", "position2", position2), MazeEvent.Instance.Configuration.Instance.SuccessColor);
                    return;
                }

                if (command[0] == "list")
                {
                    string listJoinedPlayers = "";
                    for (int x = 0; x < allJoinedPlayers.Count; x++)
                    {
                        listJoinedPlayers += allJoinedPlayers[x].getName() + " ";
                    }
                    UnturnedChat.Say(caller, MazeEvent.Instance.Translate("mazeevent_player_list", allJoinedPlayers.Count(), listJoinedPlayers));
                    return;
                }

                if (command[0] == "promote")
                {
                    if (eventActive)
                    {
                        if (command[1] != null)
                        {
                            string playerName = command[1];

                            //Find player
                            foreach (SteamPlayer plr in Provider.Players)
                            {
                                //So let's convert each SteamPlayer into an UnturnedPlayer
                                UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromSteamPlayer(plr);

                                if (unturnedPlayer.DisplayName.ToLower().IndexOf(playerName.ToLower()) != -1 || unturnedPlayer.CharacterName.ToLower().IndexOf(playerName.ToLower()) != -1 || unturnedPlayer.SteamName.ToLower().IndexOf(playerName.ToLower()) != -1 || unturnedPlayer.CSteamID.ToString().Equals(playerName))
                                {
                                    var tempCharacterInfoDuplicate = allJoinedPlayers.FirstOrDefault(item => item.getSteamID() == unturnedPlayer.CSteamID.ToString());

                                    tempCharacterInfoDuplicate.isPromoted = true;
                                    UnturnedChat.Say(unturnedPlayer, "Your spawn point has been upgraded.", Color.cyan);
                                    UnturnedChat.Say(caller, unturnedPlayer.DisplayName + "'s spawn has been upgraded to position2.", Color.green);
                                    return;
                                }
                            }
                            UnturnedChat.Say(caller, "Unable to promote, player " + playerName + " was not found!", Color.red);
                        }
                        else
                        {
                            UnturnedChat.Say(caller, "Used that wrong! Syntax: /maze promote (name).", MazeEvent.Instance.Configuration.Instance.ErrorColor);
                            return;
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "Maze event not active.", MazeEvent.Instance.Configuration.Instance.ErrorColor);
                        return;
                    }
                }

                if (command[0] == "demote")
                {
                    if (eventActive)
                    {
                        if (command[1] != null)
                        {
                            string playerName = command[1];

                            //Find player
                            foreach (SteamPlayer plr in Provider.Players)
                            {
                                //So let's convert each SteamPlayer into an UnturnedPlayer
                                UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromSteamPlayer(plr);

                                if (unturnedPlayer.DisplayName.ToLower().IndexOf(playerName.ToLower()) != -1 || unturnedPlayer.CharacterName.ToLower().IndexOf(playerName.ToLower()) != -1 || unturnedPlayer.SteamName.ToLower().IndexOf(playerName.ToLower()) != -1 || unturnedPlayer.CSteamID.ToString().Equals(playerName))
                                {
                                    var tempCharacterInfoDuplicate = allJoinedPlayers.FirstOrDefault(item => item.getSteamID() == unturnedPlayer.CSteamID.ToString());

                                    tempCharacterInfoDuplicate.isPromoted = false;
                                    UnturnedChat.Say(unturnedPlayer, "Your spawn point has been downgraded.", Color.cyan);
                                    UnturnedChat.Say(caller, unturnedPlayer.DisplayName + "'s spawn has been downgraded to position1.", Color.green);
                                    return;
                                }
                            }
                            UnturnedChat.Say(caller, "Unable to demote, player " + playerName + " was not found!", Color.red);
                        }
                        else
                        {
                            UnturnedChat.Say(caller, "Used that wrong! Syntax: /maze promote (name).", MazeEvent.Instance.Configuration.Instance.ErrorColor);
                            return;
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "Maze event not active.", MazeEvent.Instance.Configuration.Instance.ErrorColor);
                        return;
                    }
                }
            }
            else if(command[0] == "on" || command[0] == "off" || command[0] == "position1" || command[0] == "list")
            {
                UnturnedChat.Say(caller, MazeEvent.Instance.Translate("mazeevent_no_permission"), MazeEvent.Instance.Configuration.Instance.ErrorColor);
                return;
            }
            #endregion

            #region Player Commands

            if (eventActive)
            {
                if (command[0] == "join")
                {
                    var tempCharacterInfoDuplicate = allJoinedPlayers.FirstOrDefault(item => item.getSteamID() == pCaller.CSteamID.ToString());

                    if (tempCharacterInfoDuplicate == null)
                    {
                        PlayerData myPlayer = new PlayerData(pCaller.CSteamID.ToString(), pCaller.Position);

                        allJoinedPlayers.Add(myPlayer);
                        Rocket.Core.R.Permissions.AddPlayerToGroup("EventGroup", pCaller);
                        Rocket.Core.R.Permissions.RemovePlayerFromGroup("Guest", pCaller);
                        clearInventory(pCaller);
                        maxSkills(pCaller);
                        pCaller.Teleport(position1, position1Rotation);
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, MazeEvent.Instance.Translate("mazeevent_already_joined_game", pCaller.CharacterName), MazeEvent.Instance.Configuration.Instance.ErrorColor);
                        return;
                    }
                }


                if (command[0] == "storage")
                {
                    var tempCharacterInfoDuplicate = usedStorage.FirstOrDefault(item => item == pCaller.CSteamID.ToString());

                    if(tempCharacterInfoDuplicate == null)
                    {
                        pCaller.GiveItem(328, 4);
                        UnturnedChat.Say(caller, MazeEvent.Instance.Translate("mazeevent_storage_given"), MazeEvent.Instance.Configuration.Instance.SuccessColor);
                        usedStorage.Add(pCaller.CSteamID.ToString());
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, MazeEvent.Instance.Translate("mazeevent_storage_used"), MazeEvent.Instance.Configuration.Instance.ErrorColor);
                        return;
                    }
                }

                if(command[0] == "leave")
                {
                    var tempCharacterInfoDuplicate = allJoinedPlayers.FirstOrDefault(item => item.getSteamID() == pCaller.CSteamID.ToString());
                    if(tempCharacterInfoDuplicate != null)
                    {
                        Rocket.Core.R.Permissions.AddPlayerToGroup("Guest", pCaller);
                        Rocket.Core.R.Permissions.RemovePlayerFromGroup("EventGroup", pCaller);
                        pCaller.Teleport(tempCharacterInfoDuplicate.getPlayerPosition(), 0);
                        allJoinedPlayers.Remove(tempCharacterInfoDuplicate);
                        UnturnedChat.Say(caller, MazeEvent.Instance.Translate("mazeevent_left"), MazeEvent.Instance.Configuration.Instance.SuccessColor);
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, MazeEvent.Instance.Translate("mazeevent_not_joined"), MazeEvent.Instance.Configuration.Instance.ErrorColor);
                        return;
                    }
                }
            }
            else
            {
                UnturnedChat.Say(caller, MazeEvent.Instance.Translate("mazeevent_not_active"), MazeEvent.Instance.Configuration.Instance.ErrorColor);
                return;
            }

            #endregion
        }

        public void clearInventory(UnturnedPlayer tempPlayer)
        {
            var playerInventory = tempPlayer.Inventory;

            // "Remove "models" of items from player "body""
            tempPlayer.Player.channel.send("tellSlot", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (byte)0, (byte)0, new byte[0]);
            tempPlayer.Player.channel.send("tellSlot", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (byte)1, (byte)0, new byte[0]);

            // Remove items
            for (byte page = 0; page < 8; page++)
            {
                var count = playerInventory.getItemCount(page);

                for (byte index = 0; index < count; index++)
                {
                    playerInventory.removeItem(page, 0);
                }
            }

            // Remove clothes

            // Remove unequipped cloths
            System.Action removeUnequipped = () =>
            {
                for (byte i = 0; i < playerInventory.getItemCount(2); i++)
                {
                    playerInventory.removeItem(2, 0);
                }
            };

            // Unequip & remove from inventory
            tempPlayer.Player.clothing.askWearBackpack(0, 0, new byte[0], true);
            removeUnequipped();

            tempPlayer.Player.clothing.askWearGlasses(0, 0, new byte[0], true);
            removeUnequipped();

            tempPlayer.Player.clothing.askWearHat(0, 0, new byte[0], true);
            removeUnequipped();

            tempPlayer.Player.clothing.askWearPants(0, 0, new byte[0], true);
            removeUnequipped();

            tempPlayer.Player.clothing.askWearMask(0, 0, new byte[0], true);
            removeUnequipped();

            tempPlayer.Player.clothing.askWearShirt(0, 0, new byte[0], true);
            removeUnequipped();

            tempPlayer.Player.clothing.askWearVest(0, 0, new byte[0], true);
            removeUnequipped();
        }

        public static void maxSkills(UnturnedPlayer tempPlayer)
        {
 //           var tempCharacterInfoDuplicate = usedExp.FirstOrDefault(item => item == tempPlayer.CSteamID.ToString());

//            if (tempCharacterInfoDuplicate == null)
//            {
                tempPlayer.Experience += MazeEvent.Instance.Configuration.Instance.expAmount;
//                usedExp.Add(tempPlayer.CSteamID.ToString());
                UnturnedChat.Say(tempPlayer, "You received " + MazeEvent.Instance.Configuration.Instance.expAmount + " experience to upgrade your skills.", Color.cyan);
//            }
        }
    }
}
