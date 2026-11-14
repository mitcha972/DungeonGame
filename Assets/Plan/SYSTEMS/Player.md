# Player and input

Animation assessment (2026-11-13): named right/left equipment grip layers replace hardcoded attack/right-hand indices; PlayerCombat resolves child Animators. Current input supports attacks/blocking and gathering, while full ability and dodge controls remain unauthored. [Animation/models](Animation.md) owns the detailed results and asset folder guide.

Prefab organization (2026-11-13): active PlayerHolder and PlayerContent retain their established Player paths. Existing backups moved to Player/Legacy/Backups; the older binary PlayerContent formerly in GamePlay/Character/Scripts is now Player/Variants/StarterAssets_PlayerContent. Its existing missing script was retained, not repaired. HitDamage now lives in UI/Feedback. Starter Assets camera/model assets remain with their owner. See [organization validation](../MAINTENANCE.md#prefab-organization--2026-11-13).

**Implemented.** [Architecture](../ARCHITECTURE.md) owns relationships; [index](../SCRIPT_INDEX.md) owns paths. Custom controllers/stats/content moved into Assets/Scripts/Player; legacy FPC/CameraController/PlayerMotor/PlayerManager remain under Player/Legacy. Adapted Starter Assets were not moved.

PlayerInput uses the project-owned mehh.inputactions. StarterAssetsInputs bridges callbacks and resets transient movement/look/jump/sprint/toggle values. ThirdPersonController owns CharacterController locomotion, buffered/coyote jumps, gravity and camera-target rotation. PlayerController owns tap/hold attacks, shield routing, focus and camera-center interaction, not locomotion. PlayerCameraController selects mode; inspect camera assignments per scene.

PlayerControlState owns eligibility/cursor state. Subscriptions use onActionTriggered and detach on disable. Focus loss, death and modals deny gameplay. Dialogue never changes timeScale: E opens Interactable dialogue, E/Tab are ignored during it, and Escape closes it. Shop/chest interaction closes on E/Escape. Ordinary pause still freezes time. The historical Escape-always-pauses requirement is superseded for interaction/dialogue.

PlayerStats consumes Class.health/default equipment and owns XP/skills/death restart. PlayerContent applies name/class. Retained legacy PlayerManager reloads by build index; PlayerStats reloads by current scene name.

Source and historical merchant Input System tests verify input gating/restoration. [Maintenance](../MAINTENANCE.md) records current tests. Remaining: production-versus-test camera wiring, slopes/ceilings/multiple frame rates, focus/death transitions and the separate E-polling PlayerSword prototype. A file move does not certify every controller variant.

PlayerHolder now includes QuestController/catalog and the uGUI journal with a PlayerControlState journal reference. Enemy/NPC quest integration and the limits of scene-owned progress are documented in [quests](Quests.md).

Resource gathering (2026-11-13): PlayerControlState now supports an exclusive owner-checked action lease while independently retaining modal/focus/death eligibility. PlayerGathering uses it to block conflicting gameplay without pausing or freeing the cursor. Escape cancels the action; other player action keys are ignored until completion. Existing UI ownership still wins when the lease is released. PlayerController reuses its camera-center raycast/range and TMP prompt. See [resource nodes](ResourceNodes.md) for cancellation and validation.

ThirdPersonController now scales walk/sprint target velocity by CharacterStats.MovementMultiplier without changing base speeds. PlayerHolder carries the shared status controller and HUD; see [status effects](StatusEffects.md).
