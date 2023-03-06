/* ||| !!! SHITCODE ATTENTION !!! ||| */
/* ||| !!! SHITCODE ATTENTION !!! ||| */
/* ||| !!! SHITCODE ATTENTION !!! ||| */
using System;
using System.Xml.Serialization;
using Rocket.API;
using Rocket.Core.Plugins;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using UnityEngine;
using SDG.Unturned;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Rocket.Unturned;
using Rocket.Core;
using Steamworks;
using Rocket.API.Collections;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Commands;

namespace intrcptn {
    public class dict_element {
        public string group_id;
        public List<string> allowed_groups_add;
        public List<string> allowed_groups_remove;
    }

    public class group_limit {
        public string group_id;
        public List<string> disallowed_groups;

        public static bool find(string id, out int index) {
            for (int i = 0; i < main.cfg.limits.Count; i++) {
                if (main.cfg.limits[i].group_id == id) {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }
    }

    public static class util {
        public static bool find_group(List<Rocket.API.Serialisation.RocketPermissionsGroup> gps, out int index) {
            for (int i = 0; i < gps.Count; i++) {
                for (int j = 0; j < main.cfg.dict.Count; j++) {
                    if (main.cfg.dict[j].group_id == gps[i].Id) {
                        index = j;
                        return true;
                    }
                }
            }
            index = -1;
            return false;
        }

        public static bool idk(UnturnedPlayer p, string group_name) {
            var gps = R.Permissions.GetGroups(p, true);
            for (int i = 0; i < gps.Count; i++) {
                if (gps[i].Id == group_name) return true;
            }
            return false;
        }

        public static bool idk(IRocketPlayer p, string group_name) {
            var gps = R.Permissions.GetGroups(p, true);
            for (int i = 0; i < gps.Count; i++) {
                if (gps[i].Id == group_name) return true;
            }
            return false;
        }
    }

    internal class cmd_wgroup : IRocketCommand {
        public void Execute(IRocketPlayer caller, params string[] command) {
            UnturnedPlayer p = (UnturnedPlayer)caller;
            if (command.Length < 3) {
                UnturnedChat.Say(p, Syntax, Color.red);
                return;
            }
            //UnturnedPlayer p2 = UnturnedPlayer.FromName(command[1]);
            IRocketPlayer p2 = command.GetUnturnedPlayerParameter(1);
            if (p2 == null) p2 = command.GetRocketPlayerParameter(1);
            //if (p2 == null || p2.Player == null) {
            if (p2 == null) {
                UnturnedChat.Say(p, main.instance.Translate("player_not_found"), Color.red);
                return;
            }
            if (command[0].ToLower() == "add") {
                if (!p.HasPermission("interception.wgroup.wgroup.add") /*|| !p.HasPermission("interception.wgroup.wgroup.add." + command[2])*/) {
                    UnturnedChat.Say(p, main.instance.Translate("no_permission"), Color.red);
                    return;
                }
                var gps = R.Permissions.GetGroups(p, true);
                int index;
                if (!util.find_group(gps, out index)) {
                    UnturnedChat.Say(p, main.instance.Translate("no_permission"), Color.red);
                    return;
                }
                if (!main.cfg.dict[index].allowed_groups_add.Contains(command[2])) {
                    UnturnedChat.Say(p, main.instance.Translate("no_permission"), Color.red);
                    return;
                }
                int lindex;
                var limit = group_limit.find(command[2], out lindex);
                if (limit) {
                    for (int i = 0; i < main.cfg.limits[lindex].disallowed_groups.Count; i++) {
                        if (util.idk(p2, main.cfg.limits[lindex].disallowed_groups[i])) {
                            UnturnedChat.Say(p, main.instance.Translate("group_limited", command[2], main.cfg.limits[lindex].disallowed_groups[i]), Color.red);
                            return;
                        };
                    }
                }
                switch (Rocket.Core.R.Permissions.AddPlayerToGroup(command[2], p2)) {
                    case RocketPermissionsProviderResult.Success:
                        UnturnedChat.Say(caller, U.Translate("command_p_group_player_added", p2.DisplayName, command[2]));
                        return;
                    case RocketPermissionsProviderResult.DuplicateEntry:
                        UnturnedChat.Say(caller, U.Translate("command_p_duplicate_entry", p2.DisplayName, command[2]));
                        return;
                    case RocketPermissionsProviderResult.GroupNotFound:
                        UnturnedChat.Say(caller, U.Translate("command_p_group_not_found", p2.DisplayName, command[2]));
                        return;
                    case RocketPermissionsProviderResult.PlayerNotFound:
                        UnturnedChat.Say(caller, U.Translate("command_p_player_not_found", p2.DisplayName, command[2]));
                        return;
                    default:
                        UnturnedChat.Say(caller, U.Translate("command_p_unknown_error", p2.DisplayName, command[2]));
                        return;
                }
            }
            else if (command[0].ToLower() == "remove") {
                if (!p.HasPermission("interception.wgroup.wgroup.remove") /*|| !p.HasPermission("interception.wgroup.wgroup.remove." + command[2])*/) {
                    UnturnedChat.Say(p, main.instance.Translate("no_permission"), Color.red);
                    return;
                }
                var gps = R.Permissions.GetGroups(p, true);
                int index;
                if (!util.find_group(gps, out index)) {
                    UnturnedChat.Say(p, main.instance.Translate("no_permission"), Color.red);
                    return;
                }
                if (!main.cfg.dict[index].allowed_groups_remove.Contains(command[2])) {
                    UnturnedChat.Say(p, main.instance.Translate("no_permission"), Color.red);
                    return;
                }
                switch (Rocket.Core.R.Permissions.RemovePlayerFromGroup(command[2], p2)) {
                    case RocketPermissionsProviderResult.Success:
                        UnturnedChat.Say(caller, U.Translate("command_p_group_player_removed", p2.DisplayName, command[2]));
                        return;
                    case RocketPermissionsProviderResult.DuplicateEntry:
                        UnturnedChat.Say(caller, U.Translate("command_p_duplicate_entry", p2.DisplayName, command[2]));
                        return;
                    case RocketPermissionsProviderResult.GroupNotFound:
                        UnturnedChat.Say(caller, U.Translate("command_p_group_not_found", p2.DisplayName, command[2]));
                        return;
                    case RocketPermissionsProviderResult.PlayerNotFound:
                        UnturnedChat.Say(caller, U.Translate("command_p_player_not_found", p2.DisplayName, command[2]));
                        return;
                    default:
                        UnturnedChat.Say(caller, U.Translate("command_p_unknown_error", p2.DisplayName, command[2]));
                        return;
                }
            }
            else {
                UnturnedChat.Say(p, Syntax, Color.red);
                return;
            }
        }

        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "wgroup";
        public string Help => "null";
        public string Syntax => "/wgroup [add/remove] [player] [group]";
        public List<string> Aliases => new List<string>() { "wg" };
        public List<string> Permissions => new List<string> { "interception.wgroup.wgroup" };
    }

    public class config : IRocketPluginConfiguration, IDefaultable {
        public List<dict_element> dict;
        public List<group_limit> limits;

        public void LoadDefaults() {
            dict = new List<dict_element>() { 
                new dict_element() { 
                    group_id = "group1",
                    allowed_groups_add = new List<string>() { 
                        "group2", "group3"
                    },
                    allowed_groups_remove = new List<string>() {
                        "group4", "group5"
                    },
                }
            };
            limits = new List<group_limit>() {
                new group_limit() { 
                    group_id = "group2",
                    disallowed_groups = new List<string>() {
                        "group3", "group4", "group5"
                    }
                },
                new group_limit() {
                    group_id = "group3",
                    disallowed_groups = new List<string>() {
                        "group2", "group4", "group5"
                    }
                }
            };
        }
    }

    public class main : RocketPlugin<config> {
        internal static main instance;
        internal static config cfg;

        protected override void Load() {
            instance = this;
            cfg = instance.Configuration.Instance;
            GC.Collect();
        }

        protected override void Unload() {
            cfg = null;
            instance = null;
            GC.Collect();
        }

        public override TranslationList DefaultTranslations => new TranslationList
        {
            { "player_not_found", "Player not found" },
            { "no_permission", "Operation is not permitted" },
            { "group_limited", "Can't set group {0} because player already have group {1}" }
        };
    }
}

