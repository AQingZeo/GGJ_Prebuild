using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Chaos effect that corrupts text based on player's sanity (currentSan).
/// Works on top of TypewriterEffect to replace characters with random error codes.
/// Lower sanity = more corruption, higher sanity = less corruption.
/// </summary>
public class ChaosEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerStateData playerStateData; // Direct reference option
    [SerializeField] private TypewriterEffect typewriterEffect;

    [Header("Chaos Settings")]
    [SerializeField] private string chaosCharacters = "$%#^&*@!?~`";
    [SerializeField] private int maxSan = 100;
    [SerializeField] private int minSan = 0;

    private System.Random random;

    private void Awake()
    {
        random = new System.Random();
        
        // Auto-find references if not assigned
        if (playerState == null)
        {
            playerState = FindObjectOfType<PlayerState>();
        }
        
        if (typewriterEffect == null)
        {
            typewriterEffect = GetComponent<TypewriterEffect>();
        }
    }

    /// <summary>
    /// Get the character transformation function based on current sanity.
    /// </summary>
    /// <returns>Function that transforms characters: (char, index) => char</returns>
    public System.Func<char, int, char> GetCharacterTransform()
    {
        return (originalChar, index) => TransformCharacter(originalChar, index);
    }

    /// <summary>
    /// Transform a character based on current sanity level.
    /// </summary>
    /// <param name="originalChar">The original character</param>
    /// <param name="index">The character index in the text</param>
    /// <returns>The transformed character (either original or chaos character)</returns>
    private char TransformCharacter(char originalChar, int index)
    {
        // Skip whitespace and newlines - don't corrupt these
        if (char.IsWhiteSpace(originalChar) || originalChar == '\n' || originalChar == '\r')
        {
            return originalChar;
        }

        // Get current sanity
        int currentSan = GetCurrentSanity();
        
        // Calculate corruption ratio
        // currentSan represents how much text should be shown complete
        // Lower currentSan = more corruption (less normal text)
        // Higher currentSan = less corruption (more normal text)
        float sanityRatio = Mathf.Clamp01((float)(currentSan - minSan) / (float)(maxSan - minSan));
        
        // Corruption chance is inverse of sanity ratio
        // At 100 sanity: 0% corruption chance
        // At 0 sanity: 100% corruption chance
        float corruptionChance = 1f - sanityRatio;
        
        // Randomly decide if this character should be corrupted
        if (random.NextDouble() < corruptionChance)
        {
            // Return a random chaos character
            int chaosIndex = random.Next(chaosCharacters.Length);
            return chaosCharacters[chaosIndex];
        }
        
        // Return original character
        return originalChar;
    }

    /// <summary>
    /// Get the current sanity value from PlayerState or PlayerStateData.
    /// </summary>
    /// <returns>Current sanity value, or maxSan if PlayerState not found</returns>
    private int GetCurrentSanity()
    {
        // First, try direct reference to PlayerStateData
        if (playerStateData != null)
        {
            return playerStateData.currentSan;
        }
        
        // Then try to get it from PlayerState
        if (playerState != null)
        {
            // Try to access PlayerStateData through reflection
            // Handle both "PlayerStatsData" (typo) and "PlayerStateData" (correct)
            var playerStateType = playerState.GetType();
            
            // Try PlayerStatsData first (the typo in PlayerState.cs)
            var playerStatsDataField = playerStateType.GetField("PlayerStatsData", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
            
            if (playerStatsDataField == null)
            {
                // Try PlayerStateData (correct name)
                playerStatsDataField = playerStateType.GetField("PlayerStateData", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.NonPublic);
            }
            
            if (playerStatsDataField != null)
            {
                var stateData = playerStatsDataField.GetValue(playerState) as PlayerStateData;
                if (stateData != null)
                {
                    return stateData.currentSan;
                }
            }
            
            // Try to find a property instead
            var playerStatsDataProperty = playerStateType.GetProperty("PlayerStatsData", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (playerStatsDataProperty == null)
            {
                playerStatsDataProperty = playerStateType.GetProperty("PlayerStateData", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            }
            
            if (playerStatsDataProperty != null)
            {
                var stateData = playerStatsDataProperty.GetValue(playerState) as PlayerStateData;
                if (stateData != null)
                {
                    return stateData.currentSan;
                }
            }
        }
        
        // Fallback: return max sanity if we can't find player state
        return maxSan;
    }

    /// <summary>
    /// Check if chaos effect should be applied based on current sanity.
    /// </summary>
    /// <returns>True if chaos should be applied (sanity is low enough)</returns>
    public bool ShouldApplyChaos()
    {
        int currentSan = GetCurrentSanity();
        return currentSan < maxSan;
    }
}
