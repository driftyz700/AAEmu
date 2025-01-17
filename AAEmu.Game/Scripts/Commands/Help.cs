﻿using AAEmu.Game.Core.Managers;
using AAEmu.Game.Core.Managers.UnitManagers;
using AAEmu.Game.Models.Game;
using AAEmu.Game.Models.Game.Char;
using AAEmu.Game.Utils.Scripts;

namespace AAEmu.Game.Scripts.Commands;

public class Help : ICommand
{
    public void OnLoad()
    {
        string[] name = { "help", "?" };
        CommandManager.Instance.Register(name, this);
    }

    public string GetCommandLineHelp()
    {
        return "[topic]";
    }

    public string GetCommandHelpText()
    {
        return "Displays help about a command <topic>. If no <topic> is provided, a list of all GM commands will be displayed";
    }

    public void Execute(Character character, string[] args, IMessageOutput messageOutput)
    {
        var list = CommandManager.Instance.GetCommandKeys();
        list.Sort();

        if (args.Length > 0)
        {
            var thisCommand = args[0].ToLower();
            var foundIt = false;
            foreach (var command in list)
            {
                if (command != thisCommand)
                    continue;

                foundIt = true;
                var cmd = CommandManager.Instance.GetCommandInterfaceByName(thisCommand);
                var argText = cmd.GetCommandLineHelp();
                var helpText = cmd.GetCommandHelpText();
                character.SendMessage("Help for: |cFFFFFFFF" + CommandManager.CommandPrefix + thisCommand + " " + argText + "|r\n|cFF999999" + helpText + "|r");
            }
            if (!foundIt)
                character.SendMessage("Command not found: " + CommandManager.CommandPrefix + thisCommand);
            return;
        }

        character.SendMessage("|cFF80FFFFList of available GM Commands|r\n-------------------------\n");
        var characterAccessLevel = CharacterManager.Instance.GetEffectiveAccessLevel(character);
        foreach (var command in list)
        {
            if (command == "help")
                continue;
            if (AccessLevelManager.Instance.GetLevel(command) > characterAccessLevel)
                continue;

            var cmd = CommandManager.Instance.GetCommandInterfaceByName(command);
            var argHelp = cmd.GetCommandLineHelp();
            if (argHelp != string.Empty)
                character.SendMessage(CommandManager.CommandPrefix + command + " |cFF999999" + argHelp + "|r");
            else
                character.SendMessage(CommandManager.CommandPrefix + command);
        }
    }

}
