# Overview (generated locally)

## High-level purpose (3–7 bullets)
- Manages dialogue flow: start, advance, select choice, end dialogue
- Subscribes to InputIntentEvent from InputRouter to handle player input during dialogue
- Executes dialogue commands (flag, sound, world mode, log) embedded in dialogue nodes
- Coordinates between DialogueUIController and ChoiceUIController for display
- Tracks dialogue state and manages node traversal through dialogue graph
- Handles game state transitions (sets Dialogue state, returns to previous state on end)

## Key entrypoints / how it runs
- `StartDialogue(string dialogueId)` - Public API: begins a dialogue by loading JSON and processing first node
- `Advance()` - Public API: moves to next dialogue node (auto-advance when no choices)
- `SelectChoice(int choiceIndex)` - Public API: handles player choice selection, executes choice commands
- `End()` - Public API: cleans up dialogue state and returns to previous game state
- `OnInputIntent(InputIntentEvent)` - Event handler: processes input to skip typewriter or advance dialogue
- `OnGameStateChanged(GameStateChanged)` - Event handler: tracks dialogue state lifecycle

## Notable dependencies / configs
- Depends on: DialogueDataLoader, DialogueUIController, ChoiceUIController, GameManager.Instance.Flags, GameStateMachine.Instance
- Uses EventBus for: InputIntentEvent, GameStateChanged, WorldModeChanged (publishes)
- Dialogue data loaded from Resources/Dialogue/{dialogueId}.json via DialogueDataLoader
- Commands executed from dialogue nodes: flag, sfx/sound/playsound, world/worldmode, log/print/debug

## Change Suggestion

**Problem**: File is 562 lines with mixed responsibilities (flow control + command execution + input handling).

**Suggested refactoring**:
1. **Extract DialogueCommandExecutor** (new class):
   - Move `ExecuteCommands()`, `ExecuteCommand()`, and all `Handle*Command()` methods (~200 lines)
   - Makes command system extensible and testable independently
   - DialogueManager delegates command execution: `commandExecutor.Execute(commands)`

2. **Simplify DialogueManager** to core flow:
   - Keep: StartDialogue, Advance, SelectChoice, End, SkipTypewriter
   - Keep: Event subscriptions and input handling
   - Keep: Node processing and UI coordination
   - Remove: All command parsing/execution logic

3. **Benefits**:
   - DialogueManager: ~350 lines (focused on dialogue flow)
   - DialogueCommandExecutor: ~200 lines (focused on command execution)
   - Easier to add new commands without touching DialogueManager
   - Better separation of concerns

**Alternative (if keeping single file)**: At minimum, group command methods into a `#region Dialogue Commands` section for better organization.
