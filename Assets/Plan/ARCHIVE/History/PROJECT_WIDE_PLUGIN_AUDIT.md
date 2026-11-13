> Historical snapshot. Superseded by [current project context](../../PROJECT_CONTEXT.md) and the subsystem references. Original requirements, file names, counts, and test limits below describe the earlier task, not current instructions. Original bytes are preserved in the organization restore ZIP.

# Project-wide cleanup and Code Monkey dependency audit — 2026-11-11

## A. Project-wide cleanup

Discovered 213 active C# source files under Assets before this pass: 107 in Assets/Scripts, 8 elsewhere under Assets, and 98 vendor files under Assets/Plugins. One new helper brings the active inventory to 214.

Also identified 110 C# files in GameplayCheckpoints/BeforeDirective02_20261108_131002. These are archived copies outside Unity's active Assets tree; they were inventoried, not rewritten. Their historical plugin references are intentionally retained.

This continues the earlier gameplay review. The project-wide pass maps ownership and dependencies across active source, reads the newly expanded character/input/demo/editor integrations, and reviews the changed application consumers in detail. Vendor review focuses on integration points, dependency boundaries, and serialized references rather than rewriting or certifying every vendor implementation.

Modified 14 existing application/integration scripts and added one helper. No vendor source, package-managed code, assembly definitions, metadata, scenes, prefabs, materials, animations, or project settings were changed. No scripts were moved, renamed, or deleted.

### Concrete cleanup

- Replaced game-owned Code Monkey button callback bindings with native EventSystem pointer handlers.
- Replaced the skill-tree color utility call with the equivalent Color32 constant.
- Removed 12 active CodeMonkey using declarations, including four imports with no live usage, plus obsolete commented dependencies.
- Skill selection now uses the UI_SkillTree instance that created the button rather than GameObject.Find with a hard-coded scene name. This also avoids selecting the wrong tree when more than one exists.
- Skill buttons cache their pointer handler instead of repeatedly calling GetComponent. Unlocked buttons explicitly regain interaction, and absent text/link/icon references are guarded.
- InventorySlot resolves its owning inventory UI through parents rather than assuming an exact two-parent hierarchy.
- ChestInventorySlot verifies that the source inventory still exists and contains the displayed item before transferring it.
- Tooltip callback setup handles absent targets; tooltip update methods handle missing content delegates.
- PlayerSword removes unused private prototype state and empty branches, and compares skill enums directly instead of converting them to strings. Existing public fields remain.
- CombatCheckpointRunner now resets run state on a new Arm call, attaches handlers idempotently, removes its play-mode callback and pending delayed callback on completion, resets its running flag, and handles an interrupted play session. Its output directory is created when that utility is explicitly run. The utility was not executed during this task.

## B. Code Monkey dependency removal

The installed utilities identify themselves as Code Monkey Utilities version 1.02. All 12 source files in Assets/Plugins/CodeMonkey and the README remain unchanged. Original tutorial attribution in adapted game scripts is retained.

### Dependencies found before this pass

| Dependency | Actual use | Result |
| --- | --- | --- |
| Button_UI | Left/right click, tooltip enter/exit, and skill-button enabled state across eight application scripts | Application callbacks moved to UIPointerEvents |
| UtilsClass.GetColorFromString | One constant, 4B677D, in UI_SkillTree | Replaced with Color32(0x4B, 0x67, 0x7D, 0xFF) |
| using CodeMonkey in ExperienceBar | No live API call | Removed |
| using CodeMonkey.Utils in both destructible scripts and Tooltip_Warning | No live API call | Removed |
| Commented PlayerSword imports and World_Bar prototype | Inactive sample code | Removed without deleting public fields |
| FunctionUpdater/timers/world bars | No active application use at the beginning of this pass; leveling's updater had already been replaced in the earlier pass | No new timer/bar framework introduced |

### Native replacement

Assets/Scripts/UI/UIPointerEvents.cs implements native IPointerClickHandler, IPointerEnterHandler, and IPointerExitHandler. It exposes only the four callback hooks used by the application. It is added at runtime to existing UI objects, so no prefab edits or new Inspector assignments are required.

It does not recreate Code Monkey's utility framework or hover-rendering implementation. Existing serialized Button_UI components keep their hover visuals. A narrow optional bridge locates that legacy component by full type name, preserves its enabled gate and click-exit setting, and synchronizes enabled-state changes made by skill buttons. Missing legacy components are tolerated.

**Result:** active application code has no CodeMonkey using declarations, Button_UI generic type lookups, or UtilsClass/timer/world-bar calls. One deliberate legacy type-name string remains in UIPointerEvents. This is removal of direct application type dependencies, not removal of every runtime Code Monkey component.

The existing vendor's callback fields are not serialized UnityEvents. All application assignments to them were moved together, avoiding duplicate application actions from the old and new handlers. A text-asset/source scan found no direct calls or persistent method bindings to the vendor pointer/manual-exit methods searched during compatibility review.

### Serialized references that prevent deletion

Button_UI has **195 GUID references across six scene/prefab files**:

| Asset | References |
| --- | ---: |
| `Assets/Prefabs/Player/PlayerBackUp/PlayerContent.prefab` | 36 |
| `Assets/Prefabs/Player/PlayerContent.prefab` | 49 |
| `Assets/Prefabs/Player/PlayerHolder.prefab` | 49 |
| `Assets/Prefabs/UI_SkillTree.prefab` | 6 |
| `Assets/Scenes/DungeonTest.unity` | 6 |
| `Assets/Scenes/Main.unity` | 49 |

Of these button instances, 36 use configured hover-color changes; 159 use Custom hover behavior. Inspected instances have movement and exit-on-click disabled. The native adapter leaves the vendor presentation component intact and also tolerates an enabled exit-on-click setting.

No asset GUID references to the other 11 Code Monkey script files were found in the scanned text scenes, prefabs, assets, controllers, override controllers, or animations. Their vendor-internal class dependencies still form a cluster. Source/GUID scans do not prove the absence of binary, dynamically loaded, or future dependencies, so none were deleted.

## C. Third-party plugins and packages

| Integration | Source files | Evidence and decision |
| --- | ---: | --- |
| Code Monkey | 12 | Keep serialized Button_UI components and vendor files during migration |
| DungeonGenerator | 2 | RoomBehaviour/WallCollision are referenced by game code and 16 game assets; retain |
| QuickOutline | 1 | Enemy interaction uses Outline; 18 game assets reference its script; retain |
| SlimUI Modern Menu | 10 | Five game assets reference four of its scripts; retain menu/settings functionality |
| Standard Assets | 38 | Legacy FPC uses CrossPlatformInput and MouseLook; four game assets also reference a vendor script; retain |
| Legacy NavMeshComponents bundle | 35 | Observed script GUID references are in its own vendor examples. Its source, editor tools, tests, and six assembly definitions remain intact; candidate for a separate legacy-navigation cleanup |
| Adapted Starter Assets | 3 outside Assets/Scripts | Game-specific movement/input integration; reviewed and retained |
| BLINK editor helper and DemoPlayer helpers | 4 outside Assets/Scripts | Imported/uncertain provenance and asset usage; inspected and retained |
| Project editor checkpoint runner | 1 | Source-only lifecycle cleanup; not run |

Observed vendor-script GUID reference summary:

| Plugin / asset scope | Asset files | GUID references | Distinct scripts |
| --- | ---: | ---: | ---: |
| CodeMonkey | Game assets | 6 | 195 | 1 |
| DungeonGenerator | Game assets | 16 | 84 | 2 |
| NavMeshComponents-master | Vendor assets | 3 | 4 | 2 |
| QuickOutline | Game assets | 18 | 18 | 1 |
| SlimUI | Game assets | 5 | 325 | 4 |
| SlimUI | Vendor assets | 4 | 161 | 5 |
| Standard Assets | Game assets | 4 | 4 | 1 |
| Standard Assets | Vendor assets | 2 | 3 | 3 |

The project declares Unity 6000.3.28f1 and registry packages including AI Navigation 2.0.16, Input System 1.20.1, Cinemachine 3.1.7, UGUI 2.0.0, and URP 17.3.0. These registrations were inspected as dependency context and left unchanged. Editor/IDE packages are not declared unnecessary merely because game scripts do not call them.

Text Mesh Pro and LevelingSystem folders contain asset content rather than additional discovered C# source in this inventory. Their assets were not treated as deletion candidates based on script counts.

## D. Folder organization and compatibility

No new folders were created. No existing files or metadata were moved or renamed. The new helper is in the existing Assets/Scripts/UI folder. Vendor sources remain separate under Assets/Plugins, and the editor-only runner remains under Assets/Editor.

Existing component names, filenames, namespaces, inheritance, enum values, serialized field names, and public member names were retained. No FormerlySerializedAs attributes were needed.

Public entry points such as ItemSelect, Tooltip.AddTooltip and its extensions, LevelWindow model setters, and CombatCheckpointRunner.Arm remain available. Native pointer callback fields are runtime bindings; no serialized UnityEvent target was rewritten.

Normal Unity import and visual/runtime verification of the new script are still required. Existing GUID metadata is unchanged; this task did not launch Unity to create/import new metadata.

## E. Remaining work

### Removal candidates — not deleted

After scene/prefab migration and Editor verification, these unused-from-application Code Monkey files can be evaluated together with their internal dependencies:

- `Assets/Plugins/CodeMonkey/Assets.cs`
- `Assets/Plugins/CodeMonkey/Utils/Button_Sprite.cs`
- `Assets/Plugins/CodeMonkey/Utils/FunctionPeriodic.cs`
- `Assets/Plugins/CodeMonkey/Utils/FunctionTimer.cs`
- `Assets/Plugins/CodeMonkey/Utils/FunctionUpdater.cs`
- `Assets/Plugins/CodeMonkey/Utils/UI_Bar.cs`
- `Assets/Plugins/CodeMonkey/Utils/UI_BarMultiple.cs`
- `Assets/Plugins/CodeMonkey/Utils/UtilsClass.cs`
- `Assets/Plugins/CodeMonkey/Utils/World_Bar.cs`
- `Assets/Plugins/CodeMonkey/Utils/World_Mesh.cs`
- `Assets/Plugins/CodeMonkey/Utils/World_Sprite.cs`

Button_UI itself is not a deletion candidate until all six referenced assets have been migrated with their GUID references and hover settings deliberately handled. Removing the whole plugin now would leave missing serialized script references even though application callback code no longer needs the vendor type to compile.

The legacy NavMeshComponents bundle is another candidate for a separate cleanup. Its example/test assemblies, example assets, and distinction from the current Unity.AI.Navigation package need review before removal. Its lack of observed game-asset GUID references is not authorization to delete it blindly.

### Issues deliberately left for later

- Native pointer behavior, hover visuals, right-clicks, tooltips, skill locks/selections, inventory actions, and XP demo buttons need actual Editor verification.
- The optional legacy type-name bridge can be removed after asset migration; until then it is an explicit compatibility dependency.
- BLINK's imported MaterialTilingOffset editor helper queries an EditorWindow-derived type through a GameObject component API and uses a hard-coded material-output path. These are potential editor-tool defects; vendor-derived source was not changed or executed.
- The checkpoint runner contains an unfinished/unreachable phase-16 AI scenario after its earlier completion path. Defining or extending that scenario would require gameplay test design; this pass only repairs lifecycle/repeat-run handling.
- Imported DemoPlayer scripts use legacy input/cursor behavior. Their intended coexistence with the main controller requires scene verification before changing movement or balancing.
- Earlier unresolved shop, shield, collision-layer, shared-renderer, camera, and first-floor-failure concerns remain in REMAINING_CONCERNS_REPORT.md. No mechanics or asset settings were invented to resolve them.
- Registry package removal, binary/dynamic dependency validation, and an AssetDatabase-based dependency audit were not performed.

## Verification

- Discovered and categorized active source across Assets, vendor folders, Editor folders, and the archived checkpoint tree.
- Traced every active application Code Monkey import/call pattern found and inspected the relevant vendor APIs before replacement.
- Inspected package registrations, all six asset assembly definitions, script GUID metadata, vendor attribution, and relevant serialized asset references read-only.
- Captured source/checksums before editing and guarded each existing source write against concurrent changes.
- Reread all 15 changed/new C# files; every saved file matched the intended source.
- Checked balanced delimiters and retained public names, serialized/public field declarations, and component base classes at source level.
- Confirmed the final active application scan contains only the intentional legacy type-name bridge, with no static Code Monkey imports or utility calls.
- Verified **328 protected files byte-for-byte unchanged**: vendor source, existing C# metadata, assembly definitions, package/version configuration, sampled attribution/license files, and all six assets containing Code Monkey buttons.
- No Unity launch, scene run, gameplay test, checkpoint execution, package installation, package removal, generated-directory modification, or project compilation was performed. These checks do not establish runtime or Inspector correctness.

## Files changed this pass

- `Assets/Editor/CombatCheckpointRunner.cs`
- `Assets/Scripts/GamePlay/Inventory/ChestInventorySlot.cs`
- `Assets/Scripts/GamePlay/Inventory/EquipmentSlotUI.cs`
- `Assets/Scripts/GamePlay/Inventory/InventorySlot.cs`
- `Assets/Scripts/GamePlay/Inventory/UISlot.cs`
- `Assets/Scripts/Systems/DestructibleObject_Scripts/DestructibleConObject.cs`
- `Assets/Scripts/Systems/DestructibleObject_Scripts/DestructibleObject.cs`
- `Assets/Scripts/Systems/LevelingSystemScripts/ExperienceBar.cs`
- `Assets/Scripts/Systems/LevelingSystemScripts/LevelWindow.cs`
- `Assets/Scripts/Systems/SkillTree/PlayerSword.cs`
- `Assets/Scripts/Systems/SkillTree/UI_SkillTree.cs`
- `Assets/Scripts/Systems/ToolTip/Tooltip.cs`
- `Assets/Scripts/Systems/ToolTip/Tooltip_Warning.cs`
- `Assets/Scripts/Systems/ToolTip/Window.cs`
- `Assets/Scripts/UI/UIPointerEvents.cs` — new helper

## Complete active C# ownership inventory

The following index classifies all 214 current C# files under Assets. Vendor classification does not imply a complete line-by-line vendor code review or permission to remove that file.

| Source | Ownership |
| --- | --- |
| Assets/Editor/CombatCheckpointRunner.cs | Project editor |
| Assets/GamePlay/Character/InputSystem/StarterAssetsInputs.cs | Adapted Starter Assets integration |
| Assets/GamePlay/Character/LowPoly Characters/Characters/LowPoly/Editor/MaterialTilingOffset.cs | Imported/demo/editor helper |
| Assets/GamePlay/Character/Scripts/BasicRigidBodyPush.cs | Adapted Starter Assets integration |
| Assets/GamePlay/Character/Scripts/ThirdPersonController.cs | Adapted Starter Assets integration |
| Assets/Plugins/CodeMonkey/Assets.cs | Vendor: CodeMonkey |
| Assets/Plugins/CodeMonkey/Utils/Button_Sprite.cs | Vendor: CodeMonkey |
| Assets/Plugins/CodeMonkey/Utils/Button_UI.cs | Vendor: CodeMonkey |
| Assets/Plugins/CodeMonkey/Utils/FunctionPeriodic.cs | Vendor: CodeMonkey |
| Assets/Plugins/CodeMonkey/Utils/FunctionTimer.cs | Vendor: CodeMonkey |
| Assets/Plugins/CodeMonkey/Utils/FunctionUpdater.cs | Vendor: CodeMonkey |
| Assets/Plugins/CodeMonkey/Utils/UI_Bar.cs | Vendor: CodeMonkey |
| Assets/Plugins/CodeMonkey/Utils/UI_BarMultiple.cs | Vendor: CodeMonkey |
| Assets/Plugins/CodeMonkey/Utils/UtilsClass.cs | Vendor: CodeMonkey |
| Assets/Plugins/CodeMonkey/Utils/World_Bar.cs | Vendor: CodeMonkey |
| Assets/Plugins/CodeMonkey/Utils/World_Mesh.cs | Vendor: CodeMonkey |
| Assets/Plugins/CodeMonkey/Utils/World_Sprite.cs | Vendor: CodeMonkey |
| Assets/Plugins/DungeonGenerator/DungeonGenerator_Scripts/RoomBehaviour.cs | Vendor: DungeonGenerator |
| Assets/Plugins/DungeonGenerator/DungeonGenerator_Scripts/WallCollision.cs | Vendor: DungeonGenerator |
| Assets/Plugins/NavMeshComponents-master/Assets/Examples/Editor/NavMeshPrefabInstanceEditor.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Examples/Scripts/AgentLinkMover.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Examples/Scripts/ClickToMove.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Examples/Scripts/DestroyOnTrigger.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Examples/Scripts/DungeonManager.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Examples/Scripts/EnableIffSleeping.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Examples/Scripts/FreeCam.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Examples/Scripts/LocalNavMeshBuilder.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Examples/Scripts/MeshTool.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Examples/Scripts/NavMeshPrefabInstance.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Examples/Scripts/NavMeshSourceTag.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Examples/Scripts/Oscillator.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Examples/Scripts/RandomInstancing.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Examples/Scripts/RandomWalk.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Examples/Scripts/SpawnPrefabOnKeyDown.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/NavMeshComponents/Editor/NavMeshAssetManager.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/NavMeshComponents/Editor/NavMeshComponentsGUIUtility.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/NavMeshComponents/Editor/NavMeshLinkEditor.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/NavMeshComponents/Editor/NavMeshModifierEditor.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/NavMeshComponents/Editor/NavMeshModifierVolumeEditor.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/NavMeshComponents/Editor/NavMeshSurfaceEditor.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/NavMeshComponents/Scripts/NavMeshLink.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/NavMeshComponents/Scripts/NavMeshModifier.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/NavMeshComponents/Scripts/NavMeshModifierVolume.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/NavMeshComponents/Scripts/NavMeshSurface.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Tests/Editor/NavMeshModifierVolumeInPrefabTests.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Tests/Editor/NavMeshSurfaceInPrefabTests.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Tests/Editor/NavMeshSurfaceInPrefabVariantTests.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Tests/Editor/PrefabSavingUtil.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Tests/Editor/PrefabStageAutoSavingUtil.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Tests/NavMeshSurfaceAgentTests.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Tests/NavMeshSurfaceLinkTests.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Tests/NavMeshSurfaceModifierTests.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Tests/NavMeshSurfaceModifierVolumeTests.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/NavMeshComponents-master/Assets/Tests/NavMeshSurfaceTests.cs | Vendor: NavMeshComponents-master |
| Assets/Plugins/QuickOutline/Scripts/Outline.cs | Vendor: QuickOutline |
| Assets/Plugins/SlimUI/Modern Menu 1/Editor/SlimUIWindow.cs | Vendor: SlimUI |
| Assets/Plugins/SlimUI/Modern Menu 1/Scripts/CheckMusicVolume.cs | Vendor: SlimUI |
| Assets/Plugins/SlimUI/Modern Menu 1/Scripts/CheckSFXVolume.cs | Vendor: SlimUI |
| Assets/Plugins/SlimUI/Modern Menu 1/Scripts/ExtraLinks.cs | Vendor: SlimUI |
| Assets/Plugins/SlimUI/Modern Menu 1/Scripts/FlexibleUI.cs | Vendor: SlimUI |
| Assets/Plugins/SlimUI/Modern Menu 1/Scripts/FlexibleUIData.cs | Vendor: SlimUI |
| Assets/Plugins/SlimUI/Modern Menu 1/Scripts/FlexibleUIElement.cs | Vendor: SlimUI |
| Assets/Plugins/SlimUI/Modern Menu 1/Scripts/MainMenuNew.cs | Vendor: SlimUI |
| Assets/Plugins/SlimUI/Modern Menu 1/Scripts/OptionsMenuNew.cs | Vendor: SlimUI |
| Assets/Plugins/SlimUI/Modern Menu 1/Scripts/ResetDemo.cs | Vendor: SlimUI |
| Assets/Plugins/Standard Assets/Characters/FirstPersonCharacter/Scripts/FirstPersonController.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Characters/FirstPersonCharacter/Scripts/HeadBob.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Characters/FirstPersonCharacter/Scripts/MouseLook.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Characters/FirstPersonCharacter/Scripts/RigidbodyFirstPersonController.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/CrossPlatformInput/Scripts/AxisTouchButton.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/CrossPlatformInput/Scripts/ButtonHandler.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/CrossPlatformInput/Scripts/CrossPlatformInputManager.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/CrossPlatformInput/Scripts/InputAxisScrollbar.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/CrossPlatformInput/Scripts/Joystick.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/CrossPlatformInput/Scripts/MobileControlRig.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/CrossPlatformInput/Scripts/PlatformSpecific/MobileInput.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/CrossPlatformInput/Scripts/PlatformSpecific/StandaloneInput.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/CrossPlatformInput/Scripts/TiltInput.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/CrossPlatformInput/Scripts/TouchPad.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/CrossPlatformInput/Scripts/VirtualInput.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/ActivateTrigger.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/AlphaButtonClickMask.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/AutoMobileShaderSwitch.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/AutoMoveAndRotate.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/CameraRefocus.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/CurveControlledBob.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/DragRigidbody.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/DynamicShadowSettings.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/EventSystemChecker.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/FollowTarget.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/ForcedReset.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/FOVKick.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/FPSCounter.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/LerpControlledBob.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/ObjectResetter.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/ParticleSystemDestroyer.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/PlatformSpecificContent.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/SimpleMouseRotator.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/SmoothFollow.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/TimedObjectActivator.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/TimedObjectDestructor.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/WaypointCircuit.cs | Vendor: Standard Assets |
| Assets/Plugins/Standard Assets/Utility/WaypointProgressTracker.cs | Vendor: Standard Assets |
| Assets/Prefabs/Player/DemoPlayer/MouseCamLook.cs | Imported/demo/editor helper |
| Assets/Prefabs/Player/DemoPlayer/PlayerMovement.cs | Imported/demo/editor helper |
| Assets/Prefabs/Player/DemoPlayer/ScreenshotMaker.cs | Imported/demo/editor helper |
| Assets/Scripts/Camera/CameraShakeInstance.cs | Game-maintained/adapted |
| Assets/Scripts/Camera/CameraShaker.cs | Game-maintained/adapted |
| Assets/Scripts/Camera/CameraShaker1.cs | Game-maintained/adapted |
| Assets/Scripts/Camera/CameraUtilities.cs | Game-maintained/adapted |
| Assets/Scripts/Camera/ShakeDemo.cs | Game-maintained/adapted |
| Assets/Scripts/Controller.cs | Game-maintained/adapted |
| Assets/Scripts/Controllers/CameraController.cs | Game-maintained/adapted |
| Assets/Scripts/Controllers/CharacterAnimationEventReceiver.cs | Game-maintained/adapted |
| Assets/Scripts/Controllers/CharacterAnimator.cs | Game-maintained/adapted |
| Assets/Scripts/Controllers/FPC.cs | Game-maintained/adapted |
| Assets/Scripts/Controllers/PlayerCameraController.cs | Game-maintained/adapted |
| Assets/Scripts/Controllers/PlayerController.cs | Game-maintained/adapted |
| Assets/Scripts/Controllers/PlayerControlState.cs | Game-maintained/adapted |
| Assets/Scripts/Controllers/UICanvasController.cs | Game-maintained/adapted |
| Assets/Scripts/DamageFlash.cs | Game-maintained/adapted |
| Assets/Scripts/DoorScript.cs | Game-maintained/adapted |
| Assets/Scripts/Drop.cs | Game-maintained/adapted |
| Assets/Scripts/Dungeon/DungeonGenerator.cs | Game-maintained/adapted |
| Assets/Scripts/EnemyAnimator.cs | Game-maintained/adapted |
| Assets/Scripts/GameController.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Abilities.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/AttackHitBox.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Class.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Combat/BlockHitBox.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Combat/CharacterCombat.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Combat/EnemyCombat.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Combat/PlayerCombat.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Combat/Projectile.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Combat/WeaponClass.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/DungeonExit.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Enemy/Enemy.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Enemy/EnemyAi.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Enemy/EnemyAiTutorial.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Enemy/EnemyInteract.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Inventory/ChestInventory.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Inventory/ChestInventorySlot.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Inventory/ChestInventoryUI.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Inventory/EquipmentSlotUI.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Inventory/HotKeySlot.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Inventory/Inventory.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Inventory/InventorySlot.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Inventory/InventoryUI.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Inventory/ShopInventory.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Inventory/ShopInventorySlot.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Inventory/ShopInventoryUI.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Inventory/UISlot.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Items/Consumable.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Items/EnemyEquipmentManager.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Items/Equipment.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Items/EquipmentManager.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Items/EquipmentManager1.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Items/EquipmentManagerUI.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Items/Interactable.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Items/Item.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Items/ItemDrop.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Items/ItemInteract.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Items/PlayerEquipmentManager.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Items/Potion.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Loot.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Player/EnemyManager.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Player/PlayerAnimator.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Player/PlayerContent.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Player/PlayerManager.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Player/PlayerMotor.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/PlayerAim.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/PlayerDamageFlash.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Stats/CharacterStats.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Stats/EnemyStats.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Stats/PlayerStats.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/Stats/Stat.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/WorldInteracts/ChestScript.cs | Game-maintained/adapted |
| Assets/Scripts/GamePlay/WorldInteracts/ShopScript.cs | Game-maintained/adapted |
| Assets/Scripts/HealthTextDisplay.cs | Game-maintained/adapted |
| Assets/Scripts/LoadingScript.cs | Game-maintained/adapted |
| Assets/Scripts/MapController.cs | Game-maintained/adapted |
| Assets/Scripts/MenuController.cs | Game-maintained/adapted |
| Assets/Scripts/ResetAnimatorBool.cs | Game-maintained/adapted |
| Assets/Scripts/SettingsMenu.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/DestructibleObject_Scripts/BrokenObject.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/DestructibleObject_Scripts/DestructibleConObject.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/DestructibleObject_Scripts/DestructibleObject.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/DestructibleObject_Scripts/HealthSystem.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/DestructibleObject_Scripts/IDamageable.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/DragAndDrrop/ObjectGrabbable.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/DragAndDrrop/PlayerPickUpDrop.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/Helper/MeshFromSkinnedMesh.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/LevelingSystemScripts/ExperienceBar.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/LevelingSystemScripts/LevelSystem.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/LevelingSystemScripts/LevelSystemAnimated.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/LevelingSystemScripts/LevelSystemAnimatedUpdater.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/LevelingSystemScripts/LevelWindow.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/SkillTree/MaterialTintColor.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/SkillTree/Player.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/SkillTree/PlayerSkills.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/SkillTree/PlayerSword.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/SkillTree/Testing.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/SkillTree/UI_SkillTree.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/ToolTip/ItemInfo.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/ToolTip/Tooltip_ItemStats.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/ToolTip/Tooltip_Warning.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/ToolTip/Tooltip.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/ToolTip/Window_Items.cs | Game-maintained/adapted |
| Assets/Scripts/Systems/ToolTip/Window.cs | Game-maintained/adapted |
| Assets/Scripts/UI/DragDrop.cs | Game-maintained/adapted |
| Assets/Scripts/UI/HealthChange.cs | Game-maintained/adapted |
| Assets/Scripts/UI/HotKeysUI.cs | Game-maintained/adapted |
| Assets/Scripts/UI/ScreenDamage.cs | Game-maintained/adapted |
| Assets/Scripts/UI/UIPointerEvents.cs | Game-maintained/adapted |
