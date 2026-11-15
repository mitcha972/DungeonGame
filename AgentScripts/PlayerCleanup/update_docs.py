import json
from pathlib import Path

plan=Path('Assets/Plan')
def read(p):
    try:
        return p.read_text(encoding='utf-8-sig')
    except UnicodeDecodeError:
        return p.read_text(encoding='cp1252')
def replace(file, old, new):
    p=plan/file
    text=read(p)
    if new in text:
        return
    if old not in text:
        raise RuntimeError(f'Missing documentation anchor in {p}: {old[:70]}')
    p.write_text(text.replace(old,new),encoding='utf-8')

replace('PROJECT_CONTEXT.md','audited against source/assets on **2026-11-13**','player architecture updated on **2026-11-15**')
replace('PROJECT_CONTEXT.md','Implemented: StarterAssets ThirdPersonController + PlayerInput/StarterAssetsInputs; PlayerController handles actions and PlayerControlState owns gameplay/UI eligibility. Camera wiring varies by scene.','Implemented: PlayerHolder + nested Player_Main; PlayerControlState collects input/owns eligibility, ThirdPersonController owns movement/facing/view, PlayerController dispatches actions. Shared CharacterAnimator/stats; jumping/heavy/sliding removed. See [architecture](Player_Architecture.md).')
replace('PROJECT_CONTEXT.md','PlayerContent/backup variants also remain; inspect their actual wiring.','Player_Main is the nested character source; older PlayerContent is now in Player/Variants. Inspect holder overrides when reusing the character.')
replace('PROJECT_CONTEXT.md','`Assets/GamePlay/Character/InputSystem/mehh.inputactions`. Adapted Starter Assets remain under `Assets/GamePlay/Character/`.','`Assets/Input/mehh.inputactions`. Project-owned movement remains StarterAssets.ThirdPersonController in `Assets/Scripts/Player/`; the StarterAssetsInputs bridge was removed.')
replace('PROJECT_CONTEXT.md','normal stair/jump traversal','normal stair traversal without jumping')
replace('DEVELOPMENT_ROADMAP.md','CharacterController locomotion with bounded acceleration/braking, jump buffering/coyote time, substeps and first/third-person support.','CharacterController walking/gravity with bounded acceleration/braking, substeps and first/third-person support. Jumping removed in the 2026-11-15 cleanup; see [player architecture](Player_Architecture.md).')
replace('DEVELOPMENT_ROADMAP.md','PlayerControlState, project-owned input actions and shared combat/interaction integration.','PlayerControlState as the sole gameplay input/eligibility boundary, one movement/facing/view owner, project-owned actions and shared character combat/animation/stats. [Cleanup report](Player_Cleanup_Report.md).')
replace('DEVELOPMENT_ROADMAP.md','**Normal stair/jump traversal — partial:**','**Normal stair traversal without jumping — partial:**')
replace('DEVELOPMENT_ROADMAP.md','Test real locomotion/gravity/timed jumping, stairs/slopes/door feel and frame rates','Jump is now intentionally absent. Test real walking/gravity/step clearance, stairs/slopes/door feel and frame rates')

replace('ARCHITECTURE.md','Input[PlayerInput / mehh] --> Bridge[StarterAssetsInputs]\n    Input --> State[PlayerControlState]\n    Bridge --> Move[ThirdPersonController]','Input[PlayerInput / mehh] --> State[PlayerControlState]\n    State -->|intentions| Move[ThirdPersonController movement / facing / view]')
replace('ARCHITECTURE.md','PlayerControlState owns cursor visibility/lock and gameplay eligibility, considering focus, death, timeScale, inventory, chest, shop and dialogue. Consumers retain subscriptions but respect that state.','PlayerControlState is the sole gameplay subscriber to PlayerInput.onActionTriggered. It collects Move/Look/Sprint/BlockHeld and routes action requests, cursor and gameplay eligibility for focus, death, timeScale, inventory, chest, shop, dialogue, journal, expanded map and gathering leases. Consumers receive intentions or domain events rather than polling hardware.')
replace('ARCHITECTURE.md','Player delegates to co-located PlayerStats, retaining a standalone demo fallback.','UI_SkillTree binds directly to PlayerStats; its UI-only demo creates a local PlayerSkills model. The redundant Player/Testing facade components were removed.')
(plan/'SYSTEMS/Player.md').write_text('''# Player and input

**Implemented 2026-11-15:** [Player architecture](../Player_Architecture.md) is the authoritative ownership/configuration guide. [Cleanup report](../Player_Cleanup_Report.md) contains the complete inspected script inventory, measured reduction, migration and actual validation. [Character technical debt](../Character_Technical_Debt.md) ranks unfinished work.

The playable aggregate is Assets/Prefabs/Player/PlayerHolder.prefab with the user's nested Player_Main.prefab. Holder supplies UI/camera references; Player_Main contains the character. The older PlayerContent prefab was moved by the user into Player/Variants and is retained as legacy/preview content. Preserve the user's nesting, serialized overrides and active scene state.

PlayerControlState collects Unity Input System intentions and owns cursor/gameplay eligibility. ThirdPersonController is the only movement/facing/view owner; PlayerController dispatches light attacks, shield intent, camera-center E interaction, U equipment, Q grabbing and quick slots. StarterAssetsInputs, PlayerCameraController, PlayerAim, Player/Testing/PlayerSword, HotKeySlot and empty Animator wrappers were removed or merged. There is no player jump, slide or heavy-attack path. Gravity, grounding, sprint and free fall remain. NPC heavy data is preserved.

Existing gathering leases, dialogue/shop handoff, inventory/chest UI, journal, minimap, independent skill XP and status effects retain their existing owners. Shared CharacterStats owns equipment subscription/modifiers and healing; PlayerStats owns class/health UI/XP/death restart. EnemyStats alone notifies EnemyAi. Shared CharacterAnimator does not disable player locomotion during combat.

Validation and limitations are linked in the report. Editor synthetic input is distinct from manual device/platform testing. Production stairs must be walkable without jumping; legacy variants, creature-specific rigs, optional grabbing configuration and coordinated persistence remain separate follow-up work.
''',encoding='utf-8')
for file,note in {
    'SYSTEMS/Combat.md':'Player cleanup (2026-11-15): shared CharacterAnimator replaces PlayerAnimator/EnemyAnimator; PlayerControlState alone collects gameplay input and PlayerController dispatches light-only attacks/shield intent. Player animation heavy states were removed, NPC heavy data retained. CharacterStats now owns common equipment subscriptions and healing; EnemyStats owns AI notification. No combat redesign. Current regression evidence is in [cleanup report](../Player_Cleanup_Report.md).',
    'SYSTEMS/Animation.md':'Player cleanup (2026-11-15): CharacterAnimator is now the direct player/NPC component. The player controller no longer contains jump/heavy states or Jump parameters; falling/landing transitions remain. NPC controller/clips are retained. The motor alone writes player locomotion parameters and facing; shared presentation retains weapon/hit/gathering/ability handling. Historical heavy-player checks above describe the earlier implementation. See [cleanup report](../Player_Cleanup_Report.md) for new validation.',
    'SYSTEMS/Inventory.md':'Player cleanup (2026-11-15): U and 1–6 use the central player action map; EquipmentManager no longer polls U on every character. Quick-slot configuration was merged into HotKeysUI and PlayerController dispatches it independently of hidden UI activation. ChestScript resolves the player canvas through PlayerControlState.InventoryUI rather than the removed Player facade. See [cleanup report](../Player_Cleanup_Report.md).',
    'SYSTEMS/Progression.md':'Player cleanup (2026-11-15): removed the duplicate Player progression facade, Testing bootstrap and obsolete PlayerSword E logger. UI_SkillTree directly binds PlayerStats when opened; the standalone UI demo creates its own PlayerSkills model. Independent PlayerSkillProgress is unchanged. See [player architecture](../Player_Architecture.md).',
    'SYSTEMS/UI.md':'Player cleanup (2026-11-15): quick-slot forwarding components and the separate skill-tree bootstrap were consolidated into existing owners. UI hierarchy/layout was preserved apart from obsolete components and an unused aim child; hidden quick-slot visuals no longer control whether gameplay shortcuts work. PlayerControlState remains the modal/cursor owner. See [cleanup report](../Player_Cleanup_Report.md).',
}.items():
    p=plan/file
    text=read(p)
    if note not in text:
        p.write_text(text.rstrip()+'\n\n'+note+'\n',encoding='utf-8')

p=plan/'SCRIPT_INDEX.md'
text=read(p)
removed=json.loads(Path('Temp/PlayerCleanup/deleted.json').read_text())
paths={x['path'] for x in removed}|{'Assets/Scripts/Inventory/UI/HotKeySlot.cs'}
lines=[line for line in text.splitlines() if not any(path in line for path in paths)]
text='\n'.join(lines)+'\n'
text=text.replace('Assets/Scripts/ThirdPersonController.cs','Assets/Scripts/Player/ThirdPersonController.cs').replace('../Scripts/ThirdPersonController.cs','../Scripts/Player/ThirdPersonController.cs')
p.write_text(text+'\n2026-11-15 player cleanup: twelve obsolete scripts removed; ThirdPersonController moved via AssetDatabase with its GUID/class/namespace retained. [Current architecture](Player_Architecture.md) and [inventory/report](Player_Cleanup_Report.md) describe consolidated responsibilities and validation. No runtime script was added.\n',encoding='utf-8')
print('Updated current context, boundaries, roadmap, subsystem references and script index.')
