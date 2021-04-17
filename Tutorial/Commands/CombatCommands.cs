﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using FImonBotDiscord.Game;
using FImonBotDiscord.Game.Abilities;
using FImonBotDiscord.Game.Combat;
using FImonBotDiscord.Game.FImons;
using FImonBotDiscord.Handlers.Dialogue;
using FImonBotDiscord.Handlers.Dialogue.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FImonBotDiscord.Commands
{
    public class CombatCommands : SharedBaseForCommands
    {
        [Command("fight")]
        public async Task Fight(CommandContext ctx)
        {
            var mentioned = ctx.Message.MentionedUsers;
            if (mentioned.Count != 1)
            {
                await SendErrorMessage("You need to tag someone to fight him",ctx);
                return;
            }
            var enemyUser = mentioned[0];
            var challenger = ctx.User;

            var myFImon = await SelectYourFImon(ctx.User, ctx.Channel, ctx.Client);
            if (myFImon == null)
            {
                await SendErrorMessage($"Fight can not proceed due to {challenger.Username} actions",ctx);
                return;
            }

            var enemyFImon = await SelectYourFImon(enemyUser, ctx.Channel, ctx.Client);
            if (enemyFImon == null)
            {
                await SendErrorMessage($"Fight can not proceed due to {enemyUser.Username} actions",ctx);
                return;
            }

            if (myFImon.AutoAttackID == null || myFImon.BasicAttackID == null || myFImon.SpecialAttackID == null ||
                myFImon.FinalAttackID == null || myFImon.DefensiveAbilityID == null)
            {
                await SendErrorMessage("Please set up all your abilities before you want to fight",ctx);
                return;
            }
            if (enemyFImon.AutoAttackID == null || enemyFImon.BasicAttackID == null || enemyFImon.SpecialAttackID == null ||
                enemyFImon.FinalAttackID == null || enemyFImon.DefensiveAbilityID == null)
            {
                await SendErrorMessage("Enemy FImon does not have all of his abilities set up",ctx);
                return;
            }

            var random = new Random();
            int indexChoice = random.Next(0, 2);
            var options = new List<DiscordUser> { { challenger }, { enemyUser } };
            var optionFImon = new List<FImon> { { myFImon }, { enemyFImon } };

            var first = options[indexChoice];
            var second = options.Find(s => s != first);

            var firstFImon = optionFImon[indexChoice];
            var secondFImon = optionFImon.Find(s => s != firstFImon);

            var firstAttacker = new InCombatFImon(firstFImon);
            var secondAttacker = new InCombatFImon(secondFImon);

            var embedShow = new DiscordEmbedBuilder()
            {
                Title = $"The first attacker will be... {firstAttacker.FImonBase.Name}",
                Color = DiscordColor.DarkGreen,
            };

            InCombatFImon currentFightingFImon = firstAttacker;
            InCombatFImon helpFimon = null;
            InCombatFImon waitingFightingFImon = secondAttacker;

            DiscordUser currentFightingUser = first;
            DiscordUser helpUser = null;
            DiscordUser waitingFightingUser = second;

            InCombatFImon winningFImon = null;
            InCombatFImon loosingFImon = null;
            string battleResult = "";
            
            while (true)
            {
                Dictionary<DiscordEmoji, ReactionStepData> attackOptions = GenerateAttackOptions(ctx, currentFightingFImon);

                ulong AttackerAbilityID = 0;
                var AttackStep = new ReactionStep("Select your attack", attackOptions);
                AttackStep.OnValidResult += (result) =>
                {
                    AttackerAbilityID = (ulong)attackOptions[result].optionalData;
                };

                AttackStep.optionalEmbed = GenerateFImonEmbed(TrainerManager.GetTrainer(currentFightingUser.Id), currentFightingFImon, false);

                var userChannel = ctx.Channel;
                var inputDialogueHandler = new DialogueHandler(ctx.Client, userChannel, currentFightingUser, AttackStep, false, false);

                bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

                Console.WriteLine("finished dialogue");
                if (!succeeded)
                {
                    Console.WriteLine("Surrender");
                    battleResult = $"{currentFightingFImon.FImonBase.Name} surrendered the fight";
                    winningFImon = waitingFightingFImon;
                    loosingFImon = currentFightingFImon;
                    break;
                }

                string commentary = "";
                Ability abilityToUse = AbilityManager.GetAbility(AttackerAbilityID);
                int damageToGive = currentFightingFImon.UseAbilityFImon(abilityToUse, waitingFightingFImon, out commentary);

                waitingFightingFImon.CurrentHealth -= damageToGive;

                var report = new DiscordEmbedBuilder()
                {
                    Title = $"Turn of: {currentFightingFImon.FImonBase.Name}",
                    Description = commentary
                };

                await SendCorrectMessage(report,ctx);

                if (waitingFightingFImon.CurrentHealth <= 0)
                {
                    Console.WriteLine("Oponent Died");
                    battleResult = $"{currentFightingFImon.FImonBase.Name} defeated {waitingFightingFImon.FImonBase.Name} in combat";
                    winningFImon = currentFightingFImon;
                    loosingFImon = waitingFightingFImon;
                    break;
                }

                helpFimon = currentFightingFImon;
                currentFightingFImon = waitingFightingFImon;
                waitingFightingFImon = helpFimon;
                helpFimon = null;

                helpUser = currentFightingUser;
                currentFightingUser = waitingFightingUser;
                waitingFightingUser = helpUser;
                helpUser = null;
            }

            int looserExp = random.Next(10, 20);
            int winnerExp = random.Next(30, 50);

            int winningExpReward = winningFImon.FImonBase.AwardExperience(winnerExp);
            int loosingExpReward = loosingFImon.FImonBase.AwardExperience(looserExp);
            var winningEmbed = new DiscordEmbedBuilder()
            {
                Title = $"And the winner is: {winningFImon.FImonBase.Name}",
                Color = DiscordColor.Gold,
                Description = $"The winner received {winningExpReward} exp and the looser receives {loosingExpReward} exp\n" + battleResult
            };
            await SendCorrectMessage(winningEmbed,ctx);
        }
    }
}