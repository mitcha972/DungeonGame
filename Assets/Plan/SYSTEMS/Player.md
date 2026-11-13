# Player and input

**Implemented.** [Architecture](../ARCHITECTURE.md) owns relationships; [index](../SCRIPT_INDEX.md) owns paths. Custom controllers/stats/content moved into Assets/Scripts/Player; legacy FPC/CameraController/PlayerMotor/PlayerManager remain under Player/Legacy. Adapted Starter Assets were not moved.

PlayerInput uses the project-owned mehh.inputactions. StarterAssetsInputs bridges callbacks and resets transient movement/look/jump/sprint/toggle values. ThirdPersonController owns CharacterController locomotion, buffered/coyote jumps, gravity and camera-target rotation. PlayerController owns tap/hold attacks, shield routing, focus and camera-center interaction, not locomotion. PlayerCameraController selects mode; inspect camera assignments per scene.

PlayerControlState owns eligibility/cursor state. Subscriptions use onActionTriggered and detach on disable. Focus loss, death and modals deny gameplay. Dialogue never changes timeScale: E opens Interactable dialogue, E/Tab are ignored during it, and Escape closes it. Shop/chest interaction closes on E/Escape. Ordinary pause still freezes time. The historical Escape-always-pauses requirement is superseded for interaction/dialogue.

PlayerStats consumes Class.health/default equipment and owns XP/skills/death restart. PlayerContent applies name/class. Retained legacy PlayerManager reloads by build index; PlayerStats reloads by current scene name.

Source and historical merchant Input System tests verify input gating/restoration. [Maintenance](../MAINTENANCE.md) records current tests. Remaining: production-versus-test camera wiring, slopes/ceilings/multiple frame rates, focus/death transitions and the separate E-polling PlayerSword prototype. A file move does not certify every controller variant.
