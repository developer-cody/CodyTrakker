// Hey! If you're snooping around my code, NO JUDGING!  
// This masterpiece includes my bloopers, failed experiments, and  
// some of the worst ways to do things— "intentionally".  
// The comments? Yeah, they’re part of the blooper reel.  
// Enjoy the chaos! 😈

using BepInEx;
using GorillaNetworking;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace CodyTrakker.CodyTrakker
{
    [BepInPlugin(Constants.GUID, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static bool InModdedRoom => NetworkSystem.Instance.InRoom && NetworkSystem.Instance.GameModeString.Contains("MODDED");
        public static bool InRoom;
        private bool hasSentJoinMessage;
        private bool hasSentLeaveMessage;
        private bool hasBeenInRoom;
        private static string currentRoomCode;
        private List<DiscordHelper> discordHelpers;

        void Start()
        {
            ConfigManager.CreateConfig();

            discordHelpers = ConfigManager.GetWebhookUrls()
                .Select(url => new DiscordHelper(url, $"{ConfigManager.Name.Value} Tracker", null))
                .ToList();

            _ = SendGameStartedMessage();

            // Lmao, I didn't know there was a Unity event for when your game closes

            // Application.quitting += OnApplicationQuitting;
            // Debug.Log("Quitting event subscribed.");
        }

        void Update()
        {
            try
            {
                /*  This was for seeing the Maps names, because lemming is weird...

                if (PhotonNetwork.InRoom)
                {
                    Debug.Log(NetworkSystem.Instance.GameModeString);
                }
                */

                if (PhotonNetwork.InRoom && !hasSentJoinMessage)
                {
                    currentRoomCode = PhotonNetwork.CurrentRoom?.Name ?? "UNKNOWN";
                    InRoom = true;
                    hasBeenInRoom = true;
                    hasSentJoinMessage = true;
                    hasSentLeaveMessage = false;

                    string message = $"{ConfigManager.Name.Value.ToUpper()} HAS JOINED THE CODE `{currentRoomCode}`!";
                    _ = SendRoomJoinedMessage(message);
                }

                if (!PhotonNetwork.InRoom && hasBeenInRoom && !hasSentLeaveMessage)
                {
                    hasSentLeaveMessage = true;
                    hasSentJoinMessage = false;

                    string message = $"{ConfigManager.Name.Value.ToUpper()} HAS LEFT THE CODE `{currentRoomCode}`";
                    _ = SendRoomLeftMessage(message);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception in Update: {ex.Message}");
            }
        }

        // SHUT UP, I didn't know there was a Unity event for when your game closes

        /*
        void OnApplicationQuitting()
        {
            try
            {
                Debug.Log("Application is quitting...");

                Task.Run(async () => await SendGameClosedMessage()).GetAwaiter().GetResult();

                foreach (var helper in discordHelpers)
                {
                    helper.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception in OnApplicationQuitting: {ex.Message}");
            }
        }
        */

        void OnApplicationQuit()
        {
            try
            {
                Debug.Log("Application is quitting...");

                // _ = SendRoomLeftMessage($"{ConfigManager.Name.Value.ToUpper()} HAS LEFT THE CODE `{currentRoomCode}`"); This is a bad way to do it.....

                Task.Run(async () => await SendRoomLeftMessage($"{ConfigManager.Name.Value.ToUpper()} HAS LEFT THE CODE `{currentRoomCode}`")).GetAwaiter().GetResult();
                Task.Run(async () => await SendGameClosedMessage()).GetAwaiter().GetResult();

                foreach (var helper in discordHelpers)
                {
                    helper.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception in OnApplicationQuitting: {ex.Message}");
            }
        }

        private string GetGamemode()
        {
            string gamemode = NetworkSystem.Instance.GameModeString;

            if (string.IsNullOrEmpty(gamemode))
                return "None? How did this happen.";

            gamemode = gamemode.ToLower();

            /*
            Okay, I AM THAT STUPUD >:C

            if (InModdedRoom)
            {
                if (gamemode.Contains("casual")) return "[M] Casual";
                if (gamemode.Contains("infection")) return "[M] Infection";
                if (gamemode.Contains("battle")) return "[M] Paintbrawl";
                if (gamemode.Contains("freeze tag")) return "[M] Freeze Tag";
                if (gamemode.Contains("hunt")) return "[M] Hunt";
                if (gamemode.Contains("guardian")) return "[M] Guardian";
            }
            else
            {
                if (gamemode.Contains("casual")) return "Casual";
                if (gamemode.Contains("infection")) return "Infection";
                if (gamemode.Contains("battle")) return "Paintbrawl";
                if (gamemode.Contains("freeze tag")) return "Freeze Tag";
                if (gamemode.Contains("hunt")) return "Hunt";
                if (gamemode.Contains("guardian")) return "Guardian";
            }
            */

            /*
            I should prob use Switch.......

            if (gamemode.Contains("casual")) return InModdedRoom ? "[M] Casual" : "Casual";
            if (gamemode.Contains("infection")) return InModdedRoom ? "[M] Infection" : "Infection";
            if (gamemode.Contains("battle")) return InModdedRoom ? "[M] Paintbrawl" : "Paintbrawl";
            if (gamemode.Contains("freeze tag")) return InModdedRoom ? "[M] Freeze Tag" : "Freeze Tag";
            if (gamemode.Contains("hunt")) return InModdedRoom ? "[M] Hunt" : "Hunt";
            if (gamemode.Contains("guardian")) return InModdedRoom ? "[M] Guardian" : "Guardian";
            */

            switch (gamemode)
            {
                case string s when s.Contains("casual"):
                    return InModdedRoom ? "[M] Casual" : "Casual";
                case string s when s.Contains("infection"):
                    return InModdedRoom ? "[M] Infection" : "Infection";
                case string s when s.Contains("battle"):
                    return InModdedRoom ? "[M] Paintbrawl" : "Paintbrawl";
                case string s when s.Contains("freeze tag"):
                    return InModdedRoom ? "[M] Freeze Tag" : "Freeze Tag";
                case string s when s.Contains("hunt"):
                    return InModdedRoom ? "[M] Hunt" : "Hunt";
                case string s when s.Contains("guardian"):
                    return InModdedRoom ? "[M] Guardian" : "Guardian";
                default:
                    return "Unknown";
            }
        }

        private string GetMap()
        {
            string map = NetworkSystem.Instance.GameModeString;

            if (string.IsNullOrEmpty(map))
                return "None? How did this happen.";

            map = map.ToLower();

            /*
            I reecon I should use a switch statement.... >:3
            
            if (map.Contains("forest")) return "Forest";
            if (map.Contains("canyon")) return "Canyons";
            if (map.Contains("cave")) return "Caves";
            if (map.Contains("mines")) return "Mines";
            if (map.Contains("beach")) return "Beach";
            if (map.Contains("city")) return "City";
            if (map.Contains("mountain")) return "Mountains";
            if (map.Contains("arcade")) return "Arcade";
            if (map.Contains("rotating")) return "Rotating";

            return "Other";
            */

            switch (true)
            {
                case var _ when map.Contains("forest"):
                    return "Forest";
                case var _ when map.Contains("canyon"):
                    return "Canyons";
                case var _ when map.Contains("cave"):
                    return "Caves";
                case var _ when map.Contains("mines"):
                    return "Mines";
                case var _ when map.Contains("beach"):
                    return "Beach";
                case var _ when map.Contains("city"):
                    return "City";
                case var _ when map.Contains("mountain"):
                    return "Mountains";
                case var _ when map.Contains("arcade"):
                    return "Arcade";
                case var _ when map.Contains("rotating"):
                    return "Rotating";
                default:
                    return "Private/Other";
            }
        }

        private async Task BroadcastAsync(Func<DiscordHelper, Task> action)
        {
            foreach (var helper in discordHelpers)
            {
                try
                {
                    await action(helper);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error sending message to webhook: {ex.Message}");
                }
            }
        }

        private async Task SendGameStartedMessage()
        {
            await BroadcastAsync(async helper =>
            {
                helper.NewMessage();
                helper.AddEmbed("GAME STARTED", $"{ConfigManager.Name.Value} has started the game.", "#00FF00");
                await helper.SendAsync();
            });
        }

        private async Task SendRoomJoinedMessage(string description)
        {
            await BroadcastAsync(async helper =>
            {
                helper.NewMessage();
                helper.AddEmbed("Room joined", description, "#00FF00");
                helper.AddField("Room:", NetworkSystem.Instance.RoomName);
                helper.AddField("Username:", NetworkSystem.Instance.LocalPlayer?.NickName);
                helper.AddField("Players in room:", NetworkSystem.Instance.RoomPlayerCount.ToString());
                helper.AddField("Map:", GetMap());
                helper.AddField("Gamemode:", GetGamemode());
                helper.AddField("Queue:", GorillaComputer.instance.currentQueue);
                await helper.SendAsync();
            });
        }

        private async Task SendRoomLeftMessage(string description)
        {
            await BroadcastAsync(async helper =>
            {
                helper.NewMessage();
                helper.AddEmbed($"{ConfigManager.Name.Value.ToUpper()} LEFT A ROOM", description, "#FFFF00");
                await helper.SendAsync();
            });
        }

        private Task SendGameClosedMessage()
        {
            List<Task> tasks = new List<Task>();

            foreach (var helper in discordHelpers)
            {
                tasks.Add(Task.Run(async () =>
                {
                    helper.NewMessage();
                    helper.AddEmbed($"{ConfigManager.Name.Value.ToUpper()} HAS CLOSED HIS GAME",
                                    $"{ConfigManager.Name.Value} has exited the game.", "#FF0000");
                    await helper.SendAsync();
                }));
            }

            return Task.WhenAll(tasks);
        }
    }
}