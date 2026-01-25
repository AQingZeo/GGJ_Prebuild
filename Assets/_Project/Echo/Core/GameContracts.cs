using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameContracts
{
    public enum GameState
    {
        Explore,
        Menu,
        Pause,
        CutScene,
        Dialogue
    }

    public enum WorldMode
    {
        Reality,
        Dream
    }

    public static class FlagKeys
    {
        public const string Flag = "flagText";
    }

    public static class InputActions
    {
        public const string Move = "move";
        public const string Interact = "interact";
        public const string Submit = "submit";
    }

    public struct GameStateChanged
    {
        public GameState PreviousState; 
        public GameState NewState;      

        public GameStateChanged(GameState previousState, GameState newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }

    public readonly struct WorldModeChanged
    {
        public readonly WorldMode WMode;
        public WorldModeChanged(WorldMode wMode)
        {
            WMode = wMode;
        }
    }
    public readonly struct FlagChanged
    {
        public readonly string Flag;
        public readonly object Value;
        public FlagChanged(string flag, object value)
        {
            Flag = flag;
            Value = value;
        }
    }

    public readonly struct InteractionEvent
    {
        public readonly string ID;
        public InteractionEvent(string id)
        {
            ID = id;
        }
    }
}