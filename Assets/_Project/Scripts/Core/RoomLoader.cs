using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameContracts;

/// <summary>
/// Lives in ExploreScene. Loads/unloads room scenes additively.
/// Registers with GameManager on Awake so actions can request room changes.
/// </summary>
public class RoomLoader : MonoBehaviour
{
    [Tooltip("Optional: player transform to move to spawn point when loading a room.")]
    [SerializeField] private Transform playerTransform;

    [Header("Initial room (loaded when Explore loads)")]
    [Tooltip("e.g. Room_StartRoom. Leave empty to load no room until an interactable requests one.")]
    [SerializeField] private string initialRoomSceneName = "";
    [Tooltip("Optional spawn point name in the initial room (e.g. Spawn_FromStartRoom).")]
    [SerializeField] private string initialSpawnPoint = "";

    private string _currentRoomSceneName = "";
    private Coroutine _transition;

    private void Awake()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetRoomLoader(this);
    }

    private void Start()
    {
        // Prefer saved room (after LoadGame); otherwise load initial room
        if (!string.IsNullOrEmpty(_currentRoomSceneName)) return;

        var flags = GameManager.Instance?.Flags;
        var savedRoom = flags != null ? flags.Get(CurrentRoomFlagKey, "") : "";
        if (!string.IsNullOrEmpty(savedRoom))
            LoadRoom(savedRoom, null);
        else if (!string.IsNullOrEmpty(initialRoomSceneName))
            LoadRoom(initialRoomSceneName, string.IsNullOrEmpty(initialSpawnPoint) ? null : initialSpawnPoint);
    }

    /// <summary>Flag key used for save/load. Stored as string in Flags.</summary>
    public const string CurrentRoomFlagKey = "current_room";

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetRoomLoader(null);
    }

    /// <summary>
    /// Load room additively and unload previous room. Optional: move player to spawn point by name.
    /// </summary>
    public void LoadRoom(string roomSceneName, string spawnPointName = null)
    {
        if (string.IsNullOrEmpty(roomSceneName)) return;
        if (_transition != null) StopCoroutine(_transition);
        _transition = StartCoroutine(LoadRoomRoutine(roomSceneName, spawnPointName));
    }

    private IEnumerator LoadRoomRoutine(string roomSceneName, string spawnPointName)
    {
        string previous = _currentRoomSceneName;

        if (!string.IsNullOrEmpty(previous))
        {
            var unloadScene = SceneManager.GetSceneByName(previous);
            if (unloadScene.isLoaded)
            {
                var op = SceneManager.UnloadSceneAsync(previous);
                while (op != null && !op.isDone) yield return null;
            }
        }

        var existing = SceneManager.GetSceneByName(roomSceneName);
        if (!existing.isLoaded)
        {
            var loadOp = SceneManager.LoadSceneAsync(roomSceneName, LoadSceneMode.Additive);
            while (loadOp != null && !loadOp.isDone) yield return null;
        }

        _currentRoomSceneName = roomSceneName;
        if (GameManager.Instance != null)
            GameManager.Instance.Flags.Set(CurrentRoomFlagKey, roomSceneName);

        if (!string.IsNullOrEmpty(spawnPointName) && playerTransform != null)
        {
            var scene = SceneManager.GetSceneByName(roomSceneName);
            if (scene.isLoaded)
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    var t = FindInChildren(root.transform, spawnPointName);
                    if (t != null)
                    {
                        playerTransform.position = t.position;
                        break;
                    }
                }
            }
        }

        _transition = null;
    }

    private static Transform FindInChildren(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            var found = FindInChildren(parent.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }
}
