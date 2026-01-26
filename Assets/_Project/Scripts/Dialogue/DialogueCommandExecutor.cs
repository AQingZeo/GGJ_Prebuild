using System.Collections.Generic;
using UnityEngine;
using GameContracts;

/// <summary>
/// Executes dialogue commands (flag, sfx, world, log, etc.).
/// Handles parsing and execution of commands embedded in dialogue nodes.
/// </summary>
public class DialogueCommandExecutor
{
    /// <summary>
    /// Execute a list of dialogue commands (flag, sfx, world, etc.).
    /// </summary>
    public void ExecuteCommands(List<DialogueCommand> commands)
    {
        if (commands == null)
        {
            return;
        }

        foreach (var command in commands)
        {
            ExecuteCommand(command);
        }
    }

    /// <summary>
    /// Execute a single dialogue command.
    /// </summary>
    private void ExecuteCommand(DialogueCommand command)
    {
        if (command == null || string.IsNullOrEmpty(command.command))
        {
            return;
        }

        string cmd = command.command.ToLower();
        List<string> args = command.args ?? new List<string>();

        switch (cmd)
        {
            case "flag":
            case "setflag":
                HandleFlagCommand(args);
                break;

            case "sfx":
            case "sound":
            case "playsound":
                HandleSoundCommand(args);
                break;

            case "world":
            case "worldmode":
                HandleWorldCommand(args);
                break;

            case "log":
            case "print":
            case "debug":
                HandleLogCommand(args);
                break;

            default:
                Debug.LogWarning($"Unknown dialogue command: {command.command}");
                break;
        }
    }

    /// <summary>
    /// Handle log commands: ["message"] - prints message to console
    /// Useful for debugging and testing command execution.
    /// </summary>
    private void HandleLogCommand(List<string> args)
    {
        if (args == null || args.Count < 1)
        {
            Debug.LogWarning("Log command requires at least 1 argument (message)");
            return;
        }

        string message = string.Join(" ", args);
        Debug.Log($"<color=cyan>[DialogueCommand]</color> {message}");
    }

    /// <summary>
    /// Handle flag commands: ["set", "flagName", "value"] or ["get", "flagName"]
    /// </summary>
    private void HandleFlagCommand(List<string> args)
    {
        if (args == null || args.Count < 2)
        {
            Debug.LogWarning("Flag command requires at least 2 arguments");
            return;
        }

        if (GameManager.Instance == null || GameManager.Instance.Flags == null)
        {
            Debug.LogWarning("FlagManager not found");
            return;
        }

        string operation = args[0].ToLower();
        string flagName = args[1];
        var flags = GameManager.Instance.Flags;

        switch (operation)
        {
            case "set":
                if (args.Count >= 3)
                {
                    string valueStr = args[2];
                    // Try to parse as bool, int, or keep as string
                    if (bool.TryParse(valueStr, out bool boolValue))
                    {
                        flags.Set(flagName, boolValue);
                        Debug.Log($"<color=green>[Flag]</color> Set {flagName} = {boolValue}");
                    }
                    else if (int.TryParse(valueStr, out int intValue))
                    {
                        flags.Set(flagName, intValue);
                        Debug.Log($"<color=green>[Flag]</color> Set {flagName} = {intValue}");
                    }
                    else
                    {
                        flags.Set(flagName, valueStr);
                        Debug.Log($"<color=green>[Flag]</color> Set {flagName} = {valueStr}");
                    }
                }
                break;

            case "get":
                // Get flag value (we don't know the type, so just check if it exists)
                bool hasFlag = flags.HasFlag(flagName);
                Debug.Log($"<color=green>[Flag]</color> Get {flagName} = exists: {hasFlag}");
                break;

            default:
                Debug.LogWarning($"Unknown flag operation: {operation}");
                break;
        }
    }

    /// <summary>
    /// Handle sound commands: ["play", "soundName"] or ["stop", "soundName"]
    /// </summary>
    private void HandleSoundCommand(List<string> args)
    {
        if (args == null || args.Count < 2)
        {
            Debug.LogWarning("Sound command requires at least 2 arguments");
            return;
        }

        string operation = args[0].ToLower();
        string soundName = args[1];

        // TODO: Implement sound playing logic
        Debug.Log($"Sound command: {operation} {soundName}");
    }

    /// <summary>
    /// Handle world mode commands: ["set", "Reality"] or ["set", "Dream"]
    /// </summary>
    private void HandleWorldCommand(List<string> args)
    {
        if (args == null || args.Count < 2)
        {
            Debug.LogWarning("World command requires at least 2 arguments");
            return;
        }

        string operation = args[0].ToLower();
        string modeName = args[1];

        if (operation == "set")
        {
            // Parse world mode
            if (System.Enum.TryParse<WorldMode>(modeName, true, out WorldMode mode))
            {
                // Publish WorldModeChanged event
                EventBus.Publish(new WorldModeChanged(mode));
                Debug.Log($"World mode changed to: {mode}");
            }
            else
            {
                Debug.LogWarning($"Invalid world mode: {modeName}");
            }
        }
    }
}
