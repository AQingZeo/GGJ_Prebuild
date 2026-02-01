using UnityEngine;
using UnityEngine.SceneManagement;
using GameContracts;

public class GameManager : MonoBehaviour, IInteractableContext
{
    public static GameManager Instance { get; private set; }

    public FlagManager Flags { get; private set; }
    public PlayerState Player { get; private set; }
    public SaveManager Save { get; private set; }
    public GameStateMachine StateMachine { get; private set; }
    public InventoryService Inventory { get; private set; }
    public InteractableSaveService Interactables { get; private set; }

    public DialogueManager Dialogue { get; set; }
    public string PendingDialogueId { get; set; }
    public RoomLoader RoomLoader { get; private set; }
    public ImagePopUIController ImagePopController { get; private set; }

    /// <summary>Called by RoomLoader in ExploreScene on Awake/OnDestroy. No FindObjectOfType.</summary>
    public void SetRoomLoader(RoomLoader loader) { RoomLoader = loader; }
    /// <summary>Called by ImagePopUIController on Awake/OnDestroy. No FindObjectOfType.</summary>
    public void SetImagePopController(ImagePopUIController c) { ImagePopController = c; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Flags = new FlagManager();
        Player = new PlayerState();
        Save = new SaveManager();
        Inventory = new InventoryService(Player);
        Interactables = new InteractableSaveService();

        Debug.Log("<color=green>【GameManager】所有核心系统初始化完毕！</color>");
    }

    private void Start()
    {
        // Get StateMachine in Start() to ensure GameStateMachine.Awake() has run first
        // They're on different GameObjects, so use Instance instead of GetComponent
        StateMachine = GameStateMachine.Instance;
        
        if (StateMachine == null)
        {
            Debug.LogError("<color=red>【GameManager】GameStateMachine.Instance is null! Check if GSM GameObject exists in Bootstrap scene.</color>");
        }
    }

    // Game-level flow entry points

    /// <summary>
    /// Start a new game. Initializes player state and sets initial game state.
    /// </summary>
    public void StartGame()
    {
        Debug.Log("<color=cyan>【GameManager】StartGame() called</color>");
        
        if (Player != null)
        {
            Player.NewGame();
            Interactables?.NewGame();
            Debug.Log("<color=green>【GameManager】Player.NewGame() called</color>");
        }
        else
        {
            Debug.LogError("<color=red>【GameManager】Player is null!</color>");
        }

        // Clear saved room so Explore loads initial room, not a previous save's room
        Flags.Set(RoomLoader.CurrentRoomFlagKey, "");

        // Ensure we have StateMachine reference (in case Start() hasn't run yet)
        if (StateMachine == null)
        {
            StateMachine = GameStateMachine.Instance;
        }
        
        if (StateMachine != null)
        {
            Debug.Log($"<color=cyan>【GameManager】Setting state to Explore (current: {StateMachine.CurrentState})</color>");
            StateMachine.SetState(GameState.Explore);
        }
        else
        {
            Debug.LogError("<color=red>【GameManager】StateMachine is null! GameStateMachine.Instance is also null. Check Bootstrap scene setup.</color>");
        }
    }

    /// <summary>
    /// Load a saved game. Restores flags and player state from save file.
    /// </summary>
    public void LoadGame()
    {
        if (Player == null)
        {
            Debug.LogWarning("PlayerState not found. Cannot load game.");
            return;
        }

        Save.Load(Flags, Player, Interactables);
        Inventory?.RaiseInventoryChanged();

        // If already in Explore (e.g. load from pause), apply saved room so we're in the right place
        string savedRoom = Flags.Get(RoomLoader.CurrentRoomFlagKey, "");
        if (RoomLoader != null && !string.IsNullOrEmpty(savedRoom))
            RoomLoader.LoadRoom(savedRoom, null);

        if (StateMachine != null)
        {
            StateMachine.SetState(GameState.Explore);
        }
    }

    /// <summary>
    /// Save the current game state (flags and player state).
    /// </summary>
    public void SaveGame()
    {
        if (Player == null)
        {
            Debug.LogWarning("PlayerState not found. Cannot save game.");
            return;
        }

        Save.Save(Flags, Player, Interactables);
        Debug.Log("<color=cyan>【GameManager】游戏已保存</color>");
    }

    /// <summary>
    /// Quit the game. Can be called from UI or other systems.
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}