#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Test save/load without entering Play mode.
/// Menu: Tools > Test Save Load
/// Verifies flags, player state, inventory, and interactables (consumed + states) round-trip.
/// Uses the same path as runtime: Application.persistentDataPath/savegame.json
/// </summary>
public static class SaveLoadTest
{
    private const string MENU = "Tools/Test Save Load";

    [MenuItem(MENU)]
    public static void Run()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "savegame.json");
        Debug.Log($"[SaveLoadTest] Save file path: {path}");

        var flags = new FlagManager();
        var player = new PlayerState();
        var interactables = new InteractableSaveService();
        var save = new SaveManager();

        // ---- Set known values ----
        flags.Set("test_save_flag", true);
        flags.Set("test_int", 42);
        player.SetLevel(5);
        player.SetCurrentHealth(80);
        player.SetCurrentSan(60);
        player.AddToInventory("test_item", 1);
        player.AddToInventory("poison", 2);
        interactables.Consume("poison_picked");
        interactables.SetState("door_01", 2);

        save.Save(flags, player, interactables);
        Debug.Log("[SaveLoadTest] Save completed.");

        // ---- Fresh instances (simulate game restart) ----
        var flags2 = new FlagManager();
        var player2 = new PlayerState();
        var interactables2 = new InteractableSaveService();

        if (!save.HasSave())
        {
            Debug.LogError("[SaveLoadTest] HasSave() is false after save!");
            return;
        }

        save.Load(flags2, player2, interactables2);
        Debug.Log("[SaveLoadTest] Load completed.");

        // ---- Verify ----
        bool ok = true;

        if (!flags2.HasFlag("test_save_flag"))
        {
            Debug.LogError("[SaveLoadTest] Flag test_save_flag missing after load.");
            ok = false;
        }
        if (player2.GetLevel() != 5)
        {
            Debug.LogError($"[SaveLoadTest] Level: expected 5, got {player2.GetLevel()}.");
            ok = false;
        }
        if (player2.GetCurrentHealth() != 80)
        {
            Debug.LogError($"[SaveLoadTest] Health: expected 80, got {player2.GetCurrentHealth()}.");
            ok = false;
        }
        if (player2.GetCurrentSan() != 60)
        {
            Debug.LogError($"[SaveLoadTest] San: expected 60, got {player2.GetCurrentSan()}.");
            ok = false;
        }
        if (!player2.HasInInventory("test_item"))
        {
            Debug.LogError("[SaveLoadTest] Inventory: test_item missing after load.");
            ok = false;
        }
        if (!player2.HasInInventory("poison"))
        {
            Debug.LogError("[SaveLoadTest] Inventory: poison missing after load.");
            ok = false;
        }
        if (!interactables2.IsConsumed("poison_picked"))
        {
            Debug.LogError("[SaveLoadTest] Interactables: poison_picked not consumed after load.");
            ok = false;
        }
        if (interactables2.GetState("door_01", 0) != 2)
        {
            Debug.LogError($"[SaveLoadTest] Interactables: door_01 state expected 2, got {interactables2.GetState("door_01", 0)}.");
            ok = false;
        }

        if (ok)
            Debug.Log("<color=green>[SaveLoadTest] PASSED: flags, player, inventory, and interactables round-trip correctly.</color>");
        else
            Debug.LogError("[SaveLoadTest] FAILED: see errors above.");
    }
}
#endif
