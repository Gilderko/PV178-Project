﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using FImonBot.Game;
using System;
using System.Threading.Tasks;

namespace FImonBot.CommandAttributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RequireNotBanned : CheckBaseAttribute
    {
        public RequireNotBanned()
        {

        }

        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null || ctx.Member == null)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(!BanManager.IsBanned(ctx.Member.Id));
        }
    }
}
