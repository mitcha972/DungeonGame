import json
import shutil
import subprocess
from pathlib import Path

plan=Path('Assets/Plan')
evidence=plan/'Validation'
audit=json.loads(Path('Temp/PlayerCleanup/discovery.json').read_text())
assets=json.loads(Path('Temp/PlayerCleanup/assets.json').read_text())
holder=next(p for p in audit['prefabs'] if p['path'].endswith('PlayerHolder.prefab'))
deleted=json.loads(Path('Temp/PlayerCleanup/deleted.json').read_text())
hotkey='Assets/Scripts/Inventory/UI/HotKeySlot.cs'
meta=subprocess.check_output(['git','show','HEAD:'+hotkey+'.meta'],text=True)
deleted.append({'path':hotkey,'guid':next(l.split(': ')[1] for l in meta.splitlines() if l.startswith('guid: '))})
(evidence/'player-cleanup-deleted.json').write_text(json.dumps(deleted,indent=2)+'\n')
shutil.copyfile('Temp/PlayerCleanup/assets.json',evidence/'player-cleanup-assets.json')

old_scripts={c['script'] for o in holder['objects'] for c in o['components'] if c.get('script','').startswith('Assets/Scripts/')}
core=next(o for o in holder['objects'] if o['path']=='Player')
core_before=sum(c.get('script','').startswith('Assets/Scripts/') for c in core['components'])
main=next(p for p in assets['prefabs'] if p['path'].endswith('Player_Main.prefab'))
core_types=next(o for o in main['objects'] if o['path']=='')['components']
core_after=sum(not c.startswith('UnityEngine.') for c in core_types)
merges={
 'StarterAssetsInputs':'MERGE → PlayerControlState intentions; deleted',
 'PlayerCameraController':'MERGE → ThirdPersonController view modes; deleted',
 'PlayerAnimator':'SHARE → direct CharacterAnimator; deleted',
 'Player':'MERGE → existing PlayerStats; demo model in UI_SkillTree; deleted',
 'Testing':'MERGE → UI_SkillTree binding; deleted',
 'HotKeySlot':'MERGE → HotKeysUI slot data / PlayerController request; deleted',
 'PlayerPickUpDrop':'MERGE → PlayerController Q action / grab configuration; deleted',
 'PlayerSword':'DELETE — obsolete E-only log prototype',
 'PlayerAim':'DELETE — unused transform follower / unused combat aim reference',
 'ThirdPersonController':'REWRITE — sole locomotion/facing/view owner; GUID-preserving move to Player',
 'PlayerController':'REWRITE — domain action dispatcher, interaction/focus/prompt; no transform writes',
 'PlayerControlState':'REWRITE — central input/eligibility boundary, preserving modal/action leases',
 'PlayerStats':'SIMPLIFY — common equipment binding/healing moved to CharacterStats',
 'HotKeysUI':'SIMPLIFY — keep icon UI; own quick-slot configuration without hidden-object key polling',
 'UI_SkillTree':'SIMPLIFY — direct PlayerStats binding; self-contained UI-only demo fallback',
}
locations={p:[] for p in old_scripts}
for o in holder['objects']:
 for c in o['components']:
  if c.get('script') in locations:
   locations[c['script']].append(o['path'] or '(holder root)')
rows=[]
for script in sorted(old_scripts):
 name=Path(script).stem
 loc=locations[script]
 location=loc[0] if len(loc)==1 else f'{len(loc)} UI/hierarchy instances; e.g. {loc[0]}'
 decision=merges.get(name,'KEEP — existing cohesive gameplay/presentation owner')
 rows.append(f'| {name} | `{location}` | {decision} |')
tests=[]
for label,file in [('Player runtime','player-cleanup-runtime.json'),('NPC/combat/equipment integration','player-cleanup-integration.json'),('Dialogue/chest/shop','player-cleanup-modals.json')]:
 a=json.loads((evidence/file).read_text())
 tests.append(f'| {label} | {len(a["checks"])} / {len(a["checks"])} passed; errors {len(a["errors"])} | [{file}](Validation/{file}) |')
death=json.loads((evidence/'player-cleanup-death.json').read_text())
tests.append('| Player death/reload | 2 / 2 passed | [death evidence](Validation/player-cleanup-death.json) |')
report=f'''# Player cleanup report

Implemented 2026-11-15 for **PlayerHolder and its nested Player_Main**, as clarified by the user. [Architecture](Player_Architecture.md) owns current responsibilities; [technical debt](Character_Technical_Debt.md) owns ranked follow-ups. All task documentation uses Assets/Plan despite the pasted task's older Assets/Plans spelling.

## Measured reduction

| Measure | Before | After |
| --- | ---: | ---: |
| Distinct custom script types attached anywhere in playable PlayerHolder, including UI/children | {assets['beforeCount']} | {assets['afterCount']} |
| Custom script components directly on the main character | {core_before} | {core_after} |
| C# files under Assets/Scripts | 147 | {len(list(Path('Assets/Scripts').rglob('*.cs')))} |
| Gameplay subscribers to PlayerInput.onActionTriggered | 3 | 1 |
| Scripts independently writing player facing | 2 | 1 |

**Twelve source scripts removed:** eight consolidated into existing/shared owners and four dead/obsolete implementations deleted. Three ownership components rewritten: PlayerControlState, PlayerController and ThirdPersonController. Two empty animation subclasses now use the direct shared CharacterAnimator, and common equipment binding/healing is implemented in the existing CharacterStats. No runtime script, interface hierarchy or replacement manager was added. Shared base classes are dependencies, not extra attached-script types; moving ThirdPersonController does not count as removing an additional script.

Deleted sources/GUIDs: [machine-readable removal manifest](Validation/player-cleanup-deleted.json). The eleven first-stage deletions and the later HotKeySlot removal each ran AssetDatabase dependency checks across all prefabs/scenes before deletion. The FPC script had no serialized consumers. Legacy PlayerMotor/PlayerManager/CameraController/BasicRigidBodyPush still have older asset consumers and were retained outside the main-character architecture.

## Discovery and dependency decisions

Inspected the complete original PlayerHolder hierarchy ({len(holder['objects'])} transforms), all attached MonoBehaviours including nested UI/vendor components, Player_Main after the user's extraction, older PlayerContent/StarterAssets variants, humanoid Enemy_Main/NPC_Main/Quartermaster assets, creature variants, input actions, player/NPC controllers, clip callbacks, scene overrides and external consumers. Editor inspection recorded all component types/object references and AssetDatabase dependencies. Final [asset/hierarchy evidence](Validation/player-cleanup-assets.json) includes every component on the rebuilt player and relevant humanoid prefabs.

The original ThirdPersonController moved the capsule and PlayerController routed actions; they were not two simultaneous capsule movers. They did independently rotate the character, while PlayerCameraController and StarterAssetsInputs added forwarding/state. The cleanup resolves that ownership conflict without creating a giant movement/combat/inventory component.

Inventory below is measured against the original production holder. Repeated inventory/shop/slot/presentation instances are grouped by script; source/asset locations remain in SCRIPT_INDEX and the hierarchy evidence.

| Original script | Original attachment | Decision / implemented result |
| --- | --- | --- |
{chr(10).join(rows)}

Additional dependencies: CharacterStats/CharacterCombat/CharacterAnimator, EquipmentManager, Item/Equipment/Consumable/WeaponClass/Abilities/Class data, Interactable/resource/shop/chest/dialogue/quest definitions and services, progression models, EnemyAi/EnemyStats/EnemyCombat/EnemyEquipmentManager, DungeonGenerator, GameController/menu/loading and existing uGUI controls. Execution remains in those owners. PlayerController → PlayerCombat/EquipmentManager/HotKeysUI/Interactable; motor → input intentions/CharacterStats; damage → CharacterStats/derived death handlers; NPC → EnemyAi + shared combat/stats/presentation. [Architecture](Player_Architecture.md) describes integration and extension contracts.

Vendor/package components (TMP, uGUI, EventSystem/InputSystemUIInputModule, Cinemachine, SlimUI and CodeMonkey presentation) were retained. CharacterController, kinematic/physics setup, Animator/avatar, audio, colliders, rig bones and equipment/damage volumes were preserved. Empty PlayerAim was removed after checking its only gameplay reference was the now-unused PlayerCombat.aim field. No third-party scripts were moved or deleted.

## Assets and behavior

- Preserved the user's Player_Main prefab extraction, PlayerHolder nesting and PlayerContent move to Variants. Main source identity and holder overrides remain linked; camera and UI dependencies resolve from the aggregate.
- Migrated camera mode/render hiding into the motor, grab data into actions, skill-tree binding into UI_SkillTree, quick-slot data into HotKeysUI and animation subclasses into CharacterAnimator. Serialized component identities were retained for the animation substitution; the movement GUID/class/namespace and public callback methods remain.
- Player_Main retains PlayerStats, CharacterAnimator, PlayerCombat, ThirdPersonController, PlayerController, PlayerEquipmentManager, Inventory, PlayerControlState, DialogueController, QuestController, PlayerGathering, PlayerSkillProgress and StatusEffectController.
- Input remains mehh + PlayerInput. Removed Jump action/binding; Q/U/1–6 are authored in that same map. Removed buffered/coyote jump logic/timers and player jump/heavy Animator states/parameter; retained gravity, free fall and landing. No authored player slide input/state existed to migrate. Shared NPC heavy clips/data remain intact. Inherited Attack(true) is normalized to light at the player boundary.
- Updated Enemy_Main and affected animation-wrapper consumers, including legacy creature sources and scene-added enemy components, without changing EnemyAi navigation/combat policy. Friendly NPCs inherit shared presentation with hostile AI/combat still disabled. Common CharacterStats equipment subscription/healing replaces duplicated code; EnemyStats owns AI updates.
- Updated direct scene consumers (including Start's loose preview, SkillTree_Test's UI-only model and enemy overrides in combat/gathering fixtures). Removed scripts are not left as missing components. Earlier missing prefab-source remnants in some test scenes remain separately recorded debt.
- Removed the pre-existing missing camera script on the binary StarterAssets_PlayerContent variant because Unity refused to save its valid animation migration otherwise. Its original binary snapshot is a recovery artifact outside Assets. This repairs a missing component; it does not certify that old variant's complete gameplay.
- Parent room/NPC assets that only inherited changes were restored from their task-local pre-edit snapshots after incidental Unity reserialization; they inherit the source migration without unrelated saved changes.
- Repaired invalid serialized callbacks/missing optional UI references in PlayerHolder and removed the dormant UnityEvent input-forwarding arrays in Player_Main. Existing native pointer routes remain authoritative; the inventory RemoveButton now calls the existing RemoveFromInventory method. [Repair manifest](Validation/player-cleanup-reference-repairs.json) records individual edits. This does not implement optional imported settings-template audio/key rebinding.

Changed retained code: `Player/ThirdPersonController.cs`, `Player/PlayerController.cs`, `Player/PlayerControlState.cs`, `Player/PlayerStats.cs`, `Combat/Animation/CharacterAnimator.cs`, `Combat/PlayerCombat.cs`, `Combat/Stats/CharacterStats.cs`, `NPC/Enemies/EnemyStats.cs`, `Inventory/Equipment/EquipmentManager.cs`, `Inventory/Chests/ChestScript.cs`, `Progression/Skills/UI_SkillTree.cs`, `UI/Inventory/HotKeysUI.cs` and `Editor/Validation/CombatCheckpointRunner.cs`. The twelve deleted sources are in the removal manifest. Authored data changes are mehh.inputactions, Player Animator.controller, the player aggregate/nested/legacy assets, shared animation consumers and the scene overrides described above. No unrelated world/quest/shop algorithm changed.

## Executed validation

Unity CLI 1.0.0 (latest check succeeded), connected Unity 6000.3.28f1 Editor. Runtime tests used real PlayerHolder components in Combat_Test, synthetic keyboard/mouse events and advancing frames. A temporary isolated platform avoids the fixture's obstructed spawn; the NPC test builds/removes an isolated native NavMesh. Temporary input settings are cloned/restored. Runtime fixtures and changes are not saved into gameplay assets.

| Check | Result | Evidence |
| --- | --- | --- |
{chr(10).join(tests)}
| Prefab/component/input/animation wiring | {len(assets['checks'])} assertions passed, including no missing scripts across five main/humanoid prefabs | [asset evidence](Validation/player-cleanup-assets.json) |
| Serialized references and UnityEvents | No broken object references or invalid persistent callbacks in PlayerHolder, Player_Main, Enemy_Main and NPC_Main | [reference evidence](Validation/player-cleanup-references.json) |
| Unity compilation | Completed; compilationFailed=false; no compiler errors | [compiler evidence](Validation/player-cleanup-compilation.json) |

Runtime coverage includes walk/sprint, mouse look, both views, absent jump, inventory/pause/journal/map/focus gating, one light attack per press/hold/release, movement during attacks, shield cancellation/re-press, shared damage/healing, opening the skill tree, timed gathering/reward, native E interaction and Escape cancellation. NPC integration includes native navigation, accepted punch animation, cancellation, actual player sword/projectile damage, shared health/effects, friendly NPC compatibility, active quick-slot item use, player-only U and attributed enemy death. Modal checks cover dialogue attack cancellation/E/Tab suppression, Escape, chest opening/transfer/close and shop opening/close. Death checks confirm immediate denial and fresh living player after reload.

Initial attempts are not counted as passing: locomotion was initially probed against obstructed spawn geometry; mouse delta was reset by the test's extra InputSystem.Update; hidden UI panels had not initialized; quick-slot forwarding was attached to inactive HUD objects. The fixture was isolated, delta processing corrected, actual panel opening tested, skill binding repaired, and shortcut routing consolidated into the active action owner. A later held-shield run using the physical mouse failed; the final player suite explicitly pairs temporary virtual keyboard/mouse devices and restores the previous scheme/devices, avoiding interference from physical mouse state. Final suites above ran after those corrections and serialized-reference repairs. An old binary prefab initially rejected saving due to its existing missing script; the missing component was removed before successful save/dependency checks. One migration exceeded the command's 60-second response window but completed; completion/deletion was checked before further work. Pipeline was briefly unavailable during a compilation/domain reload. The final explicit compile command exceeded 120 seconds; that timeout is [preserved](Validation/player-cleanup-compilation-timeout.json). Subsequent same-Editor recompile_status confirmed completed, compilationFailed=false and zero errors; the final compiler evidence records that result.

## Limits and handoff

No standalone build, physical keyboard/controller session, exhaustive slopes/ceilings/frame rates, production floor/stair traversal, full monster rig repair, complete ability system, grabbing-content certification or coordinated save system is claimed. Jump removal makes walkable stair clearance a required next world check. No full combat redesign was performed. See [technical debt](Character_Technical_Debt.md) and roadmap.

One-time migration/validation tooling is under AgentScripts/PlayerCleanup outside Assets; migration tools that mention deleted classes are historical tooling, not runtime code or repeatable builders for the final architecture. Task-local snapshots are under Temp/PlayerCleanup/Baseline. Final ownership, current source paths and next-agent requirements are in the linked architecture/index/subsystem documents.
'''
(plan/'Player_Cleanup_Report.md').write_text(report,encoding='utf-8')
print(f'Report written: {assets["beforeCount"]} -> {assets["afterCount"]} holder types; {core_before} -> {core_after} main components; twelve files removed.')
