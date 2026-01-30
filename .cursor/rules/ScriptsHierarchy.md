# Script Hierarchy

Original Cannonical Scripts hierarchy before task assignments:

```
Scripts/
├─ Core/
│  ├─ EventBus.cs
│  ├─ FlagManager.cs
│  ├─ GameContracts.cs
│  ├─ SaveManager.cs
│  ├─ GameManager.cs
│  ├─ SceneController.cs
│  └─ GameStateMachine.cs
├─ Input/
│  └─ InputRouter.cs
├─ Dialogue/
│  ├─ ChaosEffect.cs
│  ├─ DialogueDataLoader.cs
│  ├─ DialogueDataModel.cs
│  ├─ DialogueCommandExecutor.cs
│  ├─ DialogueInputHandler.cs
│  ├─ DialogueManager.cs
│  └─ TypewriterEffect.cs
├─ Interaction/
│  ├─ IInteractable.cs
│  └─ InteractionController.cs
├─ Objects/
│  └─ ItemInteractable.cs
├─ Player/
│  ├─ PalyerController.cs
│  ├─ PlayerState.cs
│  └─ PlayerStateData.cs
├─ UI/
│  ├─ MenuController.cs
│  ├─ ChoiceUIController.cs
│  └─ DialogueUIController.cs
└─ 
```

## Scripts responsibilities and restrictions
├─ Core/
│ EventBus: Plain C#
Only contains three functions
   `Subscribe` and `Unsubscribe`: for systems/files that need to react to the facts
   `Publish`: Only publish IEvent (Changes that are listened by a lot of systems)
│ FlagManager: Plain C#
Only Set, Get, Has flag check, Save/Load flags
   `Get`: for single time check flag 
   `Set`: set the flag and publish. Changes derive events should listen to event's publish.
   `HasFlag`: for single time bool check
   `IReadOnlyDictionary<string, FlagValueDto> Snapshot();` : store all the flags for saving
   `void LoadFromSnapshot(Dictionary<string, FlagValueDto> snapshot);`: load the flags
│ GameContracts: Plain C#
For game contracts, no explicit calling, just rules
│ GameManager: Mono
Own Plain C# services and read-only accessors, Game-level flow entry points(Start Quit Save Load), bootstrapper for managers
No Per Scene logic
│ GameStateMachine: Mono
Only own and transit state, publish to eventBus
│ SceneController: mono
Listen to State Machine to load unload scenes for each state: Explore, Dialogue, Cutscene, Pause
│ SaveManager: Plain C#
Only save, load, and check has save
   `Save`: save the flag and playerstate
   `Load`: call flagManager and playerState to load snapshot
   `HasSave`: Check if there is save for display continue, or later decide to auto-load
├─ Input/
│ InputRouter: mono
Only this file gets input.
Publish key events, other system needs input subscribe to eventbus when active
├─ Dialogue/
│ ChaosEffect: mono
Effect only. Take PlayerState's currentSan, replace TypewritterEffect's context by random letter for a chaotic effect
│ TypewriterEffect: mono
Effect only. The type writer effect on dialogue context. 
│ DialogueDataLoader: Plain C#
Only Load the Dialogue 
│ DialogueDataModel: Plain C#
The Data structure for Dialogue
DialogueCommandExecutor: Plain C#
Extracted Command executer from Dialogue Manager. Referred by Manager
│ DialogueManager: mono
Dialogue scene loaded by Game manager, Dialogue manager in charge of the Dialogue scene
Take the user input routed by InputRouter, advance the dialogue with node to choices, more dialogue content
Should only start dialogue, advance, call choice, end dialogue
├─ Interaction/
│ IInteractable: Plain C#
Interface for interactables 
~InteractionController: mono~
Removed. Can directly use 2D collider rn
│ ItemInteractable: mono
"What happens after the trigger is fired"
expose Inspector-configurable data: 
	•	string itemId (optional, for inventory/logging)
	•	string setFlagKey
	•	int setFlagIntValue or bool setFlagBoolValue
	•	string dialogueId (optional)
	•	bool destroyOnTrigger = true
Execute after OnTriggerEnter()
├─ Player/
│ PalyerController: mono
Only applied movement to the player using the input from InputRouter. 
Do not interact with GSM 
│ PlayerStateDataModel: Plain C#
Define the data model for player's stats with Default/initialized value. 
Keep Dictionary of Inventory
│ PlayerState: Plain C#
Get Set Save Load similar to flagmanager 
├─ UI/
│ DialogueUIController: mono
Clamp split and run typewriter/chaoseffect
│ ChoiceUIController: mono
create/update choice buttons, handle visual timing (delay/animation), and forward the selected choice to DialogueManager
│ MenuController: mono
For Menu state, 
Handle the button callback and enables continue if HaveSave(), should not handle the logic.





