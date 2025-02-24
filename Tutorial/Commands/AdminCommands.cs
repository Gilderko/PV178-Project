﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using FImonBot.CommandAttributes;
using FImonBot.Game;
using System.Linq;
using System.Threading.Tasks;


namespace FImonBot.Commands
{
    public class AdminCommands : SharedBaseForCommands
    {
        private delegate bool ManageOtherBot(DiscordUser user, out string comment);

        [Command("ban")]
        [RequireChannelNameIncludes("admin")]
        [RequireAdmin]
        public async Task Ban(CommandContext ctx)
        {
            await BanOrUnbanBot(true, ctx);
        }

        [Command("unban")]
        [RequireChannelNameIncludes("admin")]
        [RequireAdmin]
        public async Task Unban(CommandContext ctx)
        {
            await BanOrUnbanBot(false, ctx);
        }

        public async Task BanOrUnbanBot(bool shouldBan, CommandContext ctx)
        {
            var message = ctx.Message;
            if (message.MentionedUsers.Count == 0 || message.MentionEveryone)
            {
                SendErrorMessage("You failed to tag someone", ctx.Channel);
                return;
            }

            ManageOtherBot ManageOtherBot = shouldBan ? BanManager.BanUser : BanManager.UnBanUser;

            foreach (var user in message.MentionedUsers)
            {
                string commentary;

                if (!user.IsBot)
                {
                    var managedToBan = ManageOtherBot(user, out commentary);
                    if (managedToBan)
                    {
                        await SendCorrectMessage($"`{commentary}`", ctx.Channel);
                    }
                    else
                    {
                        SendErrorMessage(commentary, ctx.Channel);
                    }
                }
                else
                {
                    SendErrorMessage($"The user {user.Username} you want to ban is not a BOT", ctx.Channel);
                }
            }
        }

        [Command("getbans")]
        [RequireChannelNameIncludes("admin")]
        [RequireAdmin]
        public async Task GetBannedPeople(CommandContext ctx)
        {
            var bansEmbed = new DiscordEmbedBuilder()
            {
                Title = "Banned users are",
            };

            var bannedIds = BanManager.GetBannedUsersIDs();

            if (bannedIds.Count() == 0)
            {
                bansEmbed.ImageUrl = "https://media.giphy.com/media/xSM46ernAUN3y/giphy.gif";
                bansEmbed.Description = "None";
            }
            else
            {
                foreach (var userID in bannedIds)
                {
                    var user = await ctx.Guild.GetMemberAsync(userID);
                    bansEmbed.Description += $"{user.Username} ";
                }
                bansEmbed.ImageUrl = "https://media.giphy.com/media/Dv3sF2ZUTmVtm/giphy.gif";
            }
            await SendCorrectMessage(bansEmbed, ctx.Channel);
        }
    }
}
