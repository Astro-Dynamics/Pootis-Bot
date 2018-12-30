﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core.ServerList;
using Pootis_Bot.Core.UserAccounts;

namespace Pootis_Bot.Modules
{
    //Profile Managment commands

    public class ProfileMang : ModuleBase<SocketCommandContext>
    {      
        [Command("makenotwarnable")]
        public async Task NotWarnable(IGuildUser user)
        {
            var userAccount = UserAccounts.GetAccount((SocketUser)user);
            var _user = Context.User as SocketGuildUser;
            var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == ServerLists.GetServer(_user.Guild).staffRoleName);

            if (_user.Roles.Contains(role))
            {
                if (userAccount.IsNotWarnable == true)
                {
                    await Context.Channel.SendMessageAsync($"The user {user} is already not warnable.");
                }
                else
                {
                    userAccount.IsNotWarnable = true;
                    userAccount.NumberOfWarnings = 0;
                    UserAccounts.SaveAccounts();
                    Console.WriteLine($"The user {user} was made not warnable.");
                    await Context.Channel.SendMessageAsync($"The user {user} was made not warnable.");
                }
            }              
        }
    
        [Command("makewarnable")]
        public async Task MakeWarnable(IGuildUser user)
        {
            var userAccount = UserAccounts.GetAccount((SocketUser)user);
            var _user = Context.User as SocketGuildUser;
            var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == ServerLists.GetServer(_user.Guild).staffRoleName);
            
            if(_user.Roles.Contains(role))
            {
                if (userAccount.IsNotWarnable == false)
                {
                    await Context.Channel.SendMessageAsync($"The user {user} is already warnable.");
                }
                else
                {
                    userAccount.IsNotWarnable = false;
                    UserAccounts.SaveAccounts();
                    Console.WriteLine($"The user {user} was made warnable.");
                    await Context.Channel.SendMessageAsync($"The user {user} was made warnable.");
                }
            }       
        }

        [Command("warn")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task WarnUser(IGuildUser user)
        {
            var userAccount = UserAccounts.GetAccount((SocketUser)user);
            var _user = Context.User as SocketGuildUser;
            var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == ServerLists.GetServer(_user.Guild).staffRoleName);

            if (_user.Roles.Contains(role))
            {
                if (userAccount.IsNotWarnable == true)
                {
                    await Context.Channel.SendMessageAsync($"A warning cannot be given to {user}. That person's account is set to not warnable.");
                    return;
                }
                else
                {
                    userAccount.NumberOfWarnings++;
                    UserAccounts.SaveAccounts();
                    Console.WriteLine($"A warning was given to {user}");
                    await Context.Channel.SendMessageAsync($"A warning was given to {user}");
                }

                if (userAccount.NumberOfWarnings >= 3)
                {
                    Console.WriteLine($"{user} was kicked due to having 3 warnings.");
                    await user.KickAsync("Was kicked due to having 3 warnings.");
                }

                if (userAccount.NumberOfWarnings >= 4)
                {
                    Console.WriteLine($"{user} was baned due to having 4 warnings.");
                    await user.Guild.AddBanAsync(user, 5, "Was baned due to having 4 warnings.");
                }
            }               
        }

        [Command("profile")]       
        public async Task Profile([Remainder]string arg = "")
        {
            SocketUser target = null;
            var metionUser = Context.Message.MentionedUsers.FirstOrDefault();
            target = metionUser ?? Context.User;

            var account = UserAccounts.GetAccount(target);
            string WarningText = $"{ target.Username} currently has {account.NumberOfWarnings} warnings.";
            string Desciption = $"{target.Username} has {account.XP} XP. \n{target.Username} Has { account.Points} points. \n \n" + WarningText;
            var embed = new EmbedBuilder();

            if (account.IsNotWarnable == true)
            {
                WarningText = $"{target.Username} account is not warnable.";
            }

            embed.WithThumbnailUrl(target.GetAvatarUrl());
            embed.WithTitle(target.Username + "'s Profile.");
            embed.WithDescription(Desciption);
            embed.WithColor(new Color(56, 56, 56));
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }       
    }
}
