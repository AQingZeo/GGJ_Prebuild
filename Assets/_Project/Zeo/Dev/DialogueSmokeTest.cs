using System.Linq;
using UnityEngine;

/// <summary>
/// Smoke test for DialogueDataLoader and DialogueDataModel.
/// Attach to a GameObject and run in Play mode to verify dialogue loading.
/// </summary>
public class DialogueLoaderSmokeTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private string testDialogueId = "intro";
    [SerializeField] private bool runOnStart = true;

    private DialogueDataLoader loader;
    private int passCount;
    private int failCount;

    void Start()
    {
        if (runOnStart)
        {
            RunAllTests();
        }
    }

    [ContextMenu("Run All Tests")]
    public void RunAllTests()
    {
        passCount = 0;
        failCount = 0;
        loader = new DialogueDataLoader();

        Debug.Log("=== DIALOGUE SMOKE TEST STARTED ===");

        TestLoadDialogue();
        TestDialogueIdNotNull();
        TestNodesNotNull();
        TestNodesNotEmpty();
        TestNodeStructure();
        TestMissingDialogue();

        Debug.Log($"=== DIALOGUE SMOKE TEST COMPLETE: {passCount} PASSED, {failCount} FAILED ===");
    }

    private void TestLoadDialogue()
    {
        var data = loader.Load(testDialogueId);
        AssertNotNull(data, "Load dialogue returns non-null DialogueDataModel");
    }

    private void TestDialogueIdNotNull()
    {
        var data = loader.Load(testDialogueId);
        if (data == null) { Skip("TestDialogueIdNotNull - data is null"); return; }

        AssertNotNullOrEmpty(data.dialogueID, "DialogueDataModel.dialogueID is set");
    }

    private void TestNodesNotNull()
    {
        var data = loader.Load(testDialogueId);
        if (data == null) { Skip("TestNodesNotNull - data is null"); return; }

        AssertNotNull(data.nodes, "DialogueDataModel.nodes dictionary is not null");
    }

    private void TestNodesNotEmpty()
    {
        var data = loader.Load(testDialogueId);
        if (data == null || data.nodes == null) { Skip("TestNodesNotEmpty - data or nodes is null"); return; }

        AssertTrue(data.nodes.Count > 0, "DialogueDataModel.nodes has at least one entry");
    }

    private void TestNodeStructure()
    {
        var data = loader.Load(testDialogueId);
        if (data == null || data.nodes == null || data.nodes.Count == 0) 
        { 
            Skip("TestNodeStructure - no nodes to test"); 
            return; 
        }

        var firstNode = data.nodes.Values.First();
        AssertNotNull(firstNode, "First node is not null");

        // Log node details for manual inspection
        Debug.Log($"[INFO] First node - Speaker: '{firstNode.speaker}', Text: '{firstNode.text}', NextNodeId: '{firstNode.nextNodeId}'");

        if (firstNode.choices != null && firstNode.choices.Count > 0)
        {
            Debug.Log($"[INFO] First node has {firstNode.choices.Count} choice(s)");
            foreach (var choice in firstNode.choices)
            {
                Debug.Log($"  - Choice: '{choice.text}' -> {choice.nextNodeId}");
            }
        }
    }

    private void TestMissingDialogue()
    {
        var data = loader.Load("nonexistent_dialogue_12345");
        AssertNull(data, "Loading missing dialogue returns null");
    }

    #region Assertion Helpers

    private void AssertTrue(bool condition, string testName)
    {
        if (condition)
        {
            Debug.Log($"<color=green>[PASS]</color> {testName}");
            passCount++;
        }
        else
        {
            Debug.LogError($"<color=red>[FAIL]</color> {testName}");
            failCount++;
        }
    }

    private void AssertNotNull(object obj, string testName)
    {
        AssertTrue(obj != null, testName);
    }

    private void AssertNull(object obj, string testName)
    {
        AssertTrue(obj == null, testName);
    }

    private void AssertNotNullOrEmpty(string str, string testName)
    {
        AssertTrue(!string.IsNullOrEmpty(str), testName);
    }

    private void Skip(string reason)
    {
        Debug.LogWarning($"<color=yellow>[SKIP]</color> {reason}");
    }

    #endregion
}
