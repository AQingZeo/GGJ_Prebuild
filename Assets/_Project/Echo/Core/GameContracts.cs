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
        CutScene, //For any auto play scene
        Dialogue  //For text dialogue
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

    public readonly struct GameStateChanged
    {
        public readonly GameState From;
        public readonly GameState To;

        private GameStateChanged(GameState from, GameState to)
        {
            From = from;
            To = to;
        }
    }

    public readonly struct WorldModeChanged
    {
        public readonly WorldMode WMode;

        private WorldModeChanged(WorldMode wMode)
        {
            WMode = wMode;
        }
    }

    public readonly struct FlagChanged
    {
        public readonly string Flag;
        public readonly object Value;
        private FlagChanged(string flag, object value)
        {
            Flag = flag;
            Value = value;
        }
    }
}