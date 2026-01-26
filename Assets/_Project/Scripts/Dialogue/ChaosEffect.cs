using UnityEngine;

/// <summary>
/// Effect only. Take PlayerState's currentSan, replace TypewriterEffect's context by random letter for a chaotic effect.
/// </summary>
public class ChaosEffect : MonoBehaviour
{
    [Header("Chaos Settings")]
    [SerializeField] private string chaosCharacters = "$%#^&*@!?~`";
    [SerializeField] private int maxSan = 100;
    [SerializeField] private int minSan = 0;
    [SerializeField] [Range(0f, 1f)] private float maxCorruptionChance = 0.3f; // Maximum corruption chance even at 0 sanity (30% = mild)

    private System.Random random;

    private void Awake()
    {
        random = new System.Random();
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
    /// Replace TypewriterEffect's context by random letter for a chaotic effect.
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

        // Take PlayerState's currentSan
        int currentSan = GetCurrentSanity();
        
        // No chaos when sanity is at maximum
        if (currentSan >= maxSan)
        {
            return originalChar;
        }
        
        // Calculate corruption ratio: 0 at maxSan, 1 at minSan
        // Lower currentSan = more corruption (more random letters)
        // Higher currentSan = less corruption (more normal text)
        float sanityRatio = Mathf.Clamp01((float)(currentSan - minSan) / (float)(maxSan - minSan));
        
        // Corruption chance scales from 0 (at maxSan) to maxCorruptionChance (at minSan)
        // This ensures even at 0 sanity, corruption is mild (e.g., 30% max)
        float corruptionChance = (1f - sanityRatio) * maxCorruptionChance;
        
        // Randomly decide if this character should be replaced by random letter
        if (random.NextDouble() < corruptionChance)
        {
            // Replace by random letter from chaosCharacters
            int chaosIndex = random.Next(chaosCharacters.Length);
            return chaosCharacters[chaosIndex];
        }
        
        // Return original character
        return originalChar;
    }

    /// <summary>
    /// Get the current sanity value from PlayerState.
    /// </summary>
    /// <returns>Current sanity value, or maxSan if PlayerState not found</returns>
    private int GetCurrentSanity()
    {
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            return GameManager.Instance.Player.GetCurrentSan();
        }
        
        // Fallback: return max sanity if we can't find player state
        return maxSan;
    }

    /// <summary>
    /// Check if chaos effect should be applied based on current sanity.
    /// </summary>
    /// <returns>True if chaos should be applied (sanity is below maximum)</returns>
    public bool ShouldApplyChaos()
    {
        int currentSan = GetCurrentSanity();
        // Only apply chaos if sanity is below maximum (100 = no chaos)
        return currentSan < maxSan;
    }
}
