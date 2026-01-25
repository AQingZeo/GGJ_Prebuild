using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerStateData 
{
    //placeholder for the player states 
    public int level = 0;
    public int maxHealth = 100;
    public int currentHealth = 100;
    public int currentSan = 70;
    public int minSan = 0;

    public Dictionary<string, object> flags = new Dictionary<string, object>();
    public Dictionary<string, object> inventory = new Dictionary<string, object>();
}