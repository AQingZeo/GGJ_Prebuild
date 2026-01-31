using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using GameContracts;

/// <summary>
/// Manages scene loading/unloading. Bootstrap persists, base scenes load additively.
/// </summary>
public class SceneController : MonoBehaviour
{
    [Header("Base Scenes")]
    [SerializeField] private string menuSceneName = "MenuScene";
    [SerializeField] private string exploreSceneName = "ExploreScene";

    [Header("Overlay Scenes")]
    [SerializeField] private string dialogueSceneName = "DialogueScene";
    [SerializeField] private string cutsceneSceneName = "CutsceneScene";

    [Header("References (assign in Bootstrap)")]
    [SerializeField] private PauseUIController pauseUIController;

    private readonly HashSet<string> overlaysLoaded = new HashSet<string>();
    private string baseSceneLoaded = "";
    private Coroutine currentTransition = null;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);

        if (GameStateMachine.Instance != null)
            ApplyState(GameStateMachine.Instance.CurrentState);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
    }

    private void OnGameStateChanged(GameStateChanged e)
    {
        if (e.NewState == GameState.Pause)
        {
            pauseUIController?.Show();
            return;
        }
        else if (e.PreviousState == GameState.Pause)
        {
            pauseUIController?.Hide();
        }

        ApplyState(e.NewState);
    }

    private void StartTransition(IEnumerator transitionRoutine)
    {
        if (currentTransition != null)
            StopCoroutine(currentTransition);
        currentTransition = StartCoroutine(TransitionWrapper(transitionRoutine));
    }

    private IEnumerator TransitionWrapper(IEnumerator transitionRoutine)
    {
        yield return StartCoroutine(transitionRoutine);
        currentTransition = null;
    }

    private void ApplyState(GameState state)
    {
        if (state == GameState.Menu)
            StartTransition(TransitionToMenu());
        else if (state == GameState.Explore)
            StartTransition(TransitionToExplore());
        else if (state == GameState.Dialogue)
            StartTransition(TransitionToDialogue());
        else if (state == GameState.CutScene)
            StartTransition(TransitionToCutscene());
    }

    private IEnumerator TransitionToMenu()
    {
        if (baseSceneLoaded == exploreSceneName)
            yield return StartCoroutine(UnloadBaseScene(exploreSceneName));
        yield return StartCoroutine(UnloadAllOverlays());
        if (baseSceneLoaded != menuSceneName)
            yield return StartCoroutine(LoadBaseScene(menuSceneName));
    }

    private IEnumerator TransitionToExplore()
    {
        if (baseSceneLoaded == menuSceneName)
            yield return StartCoroutine(UnloadBaseScene(menuSceneName));
        yield return StartCoroutine(UnloadAllOverlays());
        if (baseSceneLoaded != exploreSceneName)
            yield return StartCoroutine(LoadBaseScene(exploreSceneName));
    }

    private IEnumerator TransitionToDialogue()
    {
        if (baseSceneLoaded != exploreSceneName)
            yield return StartCoroutine(LoadBaseScene(exploreSceneName));
        if (overlaysLoaded.Contains(cutsceneSceneName))
            yield return StartCoroutine(UnloadOverlay(cutsceneSceneName));
        yield return StartCoroutine(LoadOverlay(dialogueSceneName));
    }

    private IEnumerator TransitionToCutscene()
    {
        if (baseSceneLoaded != exploreSceneName)
            yield return StartCoroutine(LoadBaseScene(exploreSceneName));
        if (overlaysLoaded.Contains(dialogueSceneName))
            yield return StartCoroutine(UnloadOverlay(dialogueSceneName));
        yield return StartCoroutine(LoadOverlay(cutsceneSceneName));
    }

    private IEnumerator LoadBaseScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) yield break;

        var existingScene = SceneManager.GetSceneByName(sceneName);
        if (existingScene.isLoaded)
        {
            baseSceneLoaded = sceneName;
            SceneManager.SetActiveScene(existingScene);
            yield break;
        }

        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!op.isDone) yield return null;

        baseSceneLoaded = sceneName;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        
        // Remove duplicate EventSystems (Bootstrap already has one)
        RemoveDuplicateEventSystems();
    }

    private IEnumerator UnloadBaseScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName) || baseSceneLoaded != sceneName) yield break;

        var scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            baseSceneLoaded = "";
            yield break;
        }

        var op = SceneManager.UnloadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        baseSceneLoaded = "";
    }

    private IEnumerator LoadOverlay(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName) || overlaysLoaded.Contains(sceneName)) yield break;

        var existingScene = SceneManager.GetSceneByName(sceneName);
        if (existingScene.isLoaded)
        {
            overlaysLoaded.Add(sceneName);
            yield break;
        }

        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!op.isDone) yield return null;

        overlaysLoaded.Add(sceneName);
    }

    private IEnumerator UnloadOverlay(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName) || !overlaysLoaded.Contains(sceneName)) yield break;

        var scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            overlaysLoaded.Remove(sceneName);
            yield break;
        }

        var op = SceneManager.UnloadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        overlaysLoaded.Remove(sceneName);
    }

    private IEnumerator UnloadAllOverlays()
    {
        if (overlaysLoaded.Count == 0) yield break;

        var list = new List<string>(overlaysLoaded);
        foreach (var s in list)
            yield return StartCoroutine(UnloadOverlay(s));
    }

    /// <summary>
    /// Removes duplicate EventSystems, keeping only the one from Bootstrap.
    /// </summary>
    private void RemoveDuplicateEventSystems()
    {
        var eventSystems = FindObjectsOfType<EventSystem>();
        if (eventSystems.Length <= 1) return;

        // Keep the first one (should be from Bootstrap), destroy the rest
        for (int i = 1; i < eventSystems.Length; i++)
        {
            Destroy(eventSystems[i].gameObject);
        }
    }
}
