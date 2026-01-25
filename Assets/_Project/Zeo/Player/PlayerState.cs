using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public PlayerStateData CurrentState { get; private set; }
    public void NewGame() => CurrentState = new PlayerStateData();

}
