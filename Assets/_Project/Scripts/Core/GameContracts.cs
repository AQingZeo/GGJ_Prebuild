using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameContracts
{
    public interface IEvent {}
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

    public enum FlagValueType
    {
        Bool,
        Int,
        String
    }

    [Serializable]
    public class FlagValueDto
    {
        public FlagValueType Type;
        public bool BoolValue;
        public int IntValue;
        public string StringValue;
    }

    public static class FlagKeys
    {
        public const string Flag = "testflag";
    }

    public static class InputActions
    {
        public const string Move = "move";
        public const string Interact = "interact";
        public const string Submit = "submit";
    }

    public struct GameStateChanged : IEvent
    {
        public GameState PreviousState; 
        public GameState NewState;      

        public GameStateChanged(GameState previousState, GameState newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }

    public readonly struct WorldModeChanged : IEvent
    {
        public readonly WorldMode WMode;
        public WorldModeChanged(WorldMode wMode)
        {
            WMode = wMode;
        }
    }
    public readonly struct FlagChanged : IEvent
    {
        public readonly string Key;
        public readonly FlagValueDto Value;

        public FlagChanged(string key, FlagValueDto value)
        {
            Key = key;
            Value = value;
        }
    }

    public readonly struct InputIntentEvent : IEvent
    {
        public readonly float Horizontal;      // -1, 0, +1
        public readonly bool SubmitDown;       // any non-pause key down
        public readonly bool ClickDown;        // any mouse button down
        public readonly Vector2 PointerScreen; // mouse position when ClickDown

        public InputIntentEvent(float horizontal, bool submitDown, bool clickDown, Vector2 pointerScreen)
        {
            Horizontal = horizontal;
            SubmitDown = submitDown;
            ClickDown = clickDown;
            PointerScreen = pointerScreen;
        }
    }

}