import pathlib,json,os,re
R=pathlib.Path.cwd();P=R/'Assets/Plan';m=json.loads((P/'ARCHIVE/Organization/organization-manifest.json').read_text())
desc={}
text='''Controller|Empty compatibility component; serialized identity retained.
GameController|Session name/class/floor/difficulty and player setup.
LoadingScript|Validated asynchronous build-scene loading.
MenuController|Character selection and Start→Loading transition.
SettingsMenu|Pause/resume tabs and return to Start menu.
PlayerControlState|Central gameplay eligibility, cursor and modal input.
PlayerController|Combat input, focus and Interactable raycast dispatch.
PlayerCameraController|Camera mode selection and player renderer visibility.
PlayerStats|Health/class gear, XP/skills and death restart.
PlayerContent|Apply player identity/class and class changes.
PlayerAnimator|CharacterAnimator specialization for player.
PlayerManager|Legacy singleton build-index reload.
PlayerMotor|Legacy NavMeshAgent movement/follow controller.
FPC|Legacy first-person controller with Standard Assets input.
CameraController|Legacy orbit/zoom camera controller.
PlayerAim|Orient player aim from main camera.
PlayerDamageFlash|Player screen damage flash presentation.
Class|ScriptableObject starting health/equipment data.
Enemy|ScriptableObject enemy tuning/equipment/loot data.
EnemyAi|Update-driven perception/navigation/combat FSM.
EnemyAiTutorial|Retained tutorial AI implementation.
EnemyInteract|Enemy focus/Outline presentation.
EnemyStats|Enemy stats, damage/death, XP and loot.
EnemyAnimator|CharacterAnimator specialization for enemy.
EnemyManager|Retained enemy-instance placeholder.
CharacterAnimator|Weapon clips and equip/busy/hit/death presentation.
CharacterAnimationEventReceiver|Retained animation event receiver component.
ResetAnimatorBool|Animator state behavior resetting a bool.
CharacterCombat|Attack timeline, cancellation, damage/block/cooldown owner.
PlayerCombat|Player block eligibility and projectile/ability adapter.
EnemyCombat|Enemy attack gating and projectile/ability adapter.
AttackHitBox|Melee overlap sampling and per-attack deduplication.
BlockHitBox|Shield volume and incomplete projectile/durability bookkeeping.
Projectile|Swept nearest-hit projectile collision and range.
WeaponClass|ScriptableObject weapon clips/timing/projectile data.
Abilities|ScriptableObject ability configuration; some fields unused.
CharacterStats|Shared armor/health/modifiers and death state.
Stat|Generic integer modifier collection.
Inventory|Fixed slot storage, transfers and notifications.
InventoryUI|Discover/render player inventory slots.
UISlot|Shared item display/use/drop/drag behavior.
InventorySlot|Native pointer inventory-item use adapter.
EquipmentSlotUI|Equipment display and guarded inventory transfer.
HotKeySlot|Inventory slot hotkey adapter.
Item|ScriptableObject base item data/use contract.
Equipment|Equipment slot/mesh/stat data and equip adapter.
Consumable|Validated health consumable effect and slot removal.
Potion|Retained compatibility component.
ItemDrop|World drop configuration/helper component.
Loot|Serializable pickup/chance configuration.
EquipmentManager|Equipment state/meshes/events/reset/unequip/drop.
PlayerEquipmentManager|Player equipment UI and equip guards.
EnemyEquipmentManager|Probabilistic enemy default equipment.
EquipmentManager1|Empty compatibility component.
EquipmentManagerUI|Retained EquipmentManager specialization.
ChestInventory|Capacity-limited chest item list.
ChestScript|Chest loot generation and Interactable opening.
ChestInventoryUI|Chest slot binding and close behavior.
ChestInventorySlot|Failure-aware chest-to-player transfer.
ShopInventory|Merchant stock list and compatibility singleton.
ShopScript|Stock generation, optional dialogue and shop opening.
ShopInventoryUI|Existing shop row binding and close synchronization.
ShopInventorySlot|Price/icon and prototype free item grant.
Interactable|Shared focus and Interact(owner) contract.
ItemInteract|Guarded world pickup/add-before-destroy.
DoorScript|Interactable door opening/closing.
DungeonGenerator|Connected maze, room/NavMesh assembly and rollback.
DungeonExit|Request generator floor advancement.
MapController|Retained map placeholder.
BrokenObject|Debris fading and owned-material cleanup.
DestructibleObject|IDamageable health/effects and break behavior.
DestructibleConObject|Destructible specialization/effect callback.
HealthSystem|Reusable clamped health/damage/heal/death events.
IDamageable|World damage interface.
ObjectGrabbable|Physics grab-point movement and release.
PlayerPickUpDrop|Separate raycast rigidbody-grab prototype.
MeshFromSkinnedMesh|Edit-mode bind-mesh conversion; not auto-executed.
LevelSystem|Bounded XP thresholds/levels/events model.
LevelSystemAnimated|Displayed XP interpolation and owned subscriptions.
LevelSystemAnimatedUpdater|Local runtime interpolation/lifecycle driver.
ExperienceBar|XP bar display and retained demo setup.
LevelWindow|Level/XP display and example callbacks.
PlayerSkills|Skill points/prerequisites/unlock model.
Player|PlayerStats progression delegation; standalone demo fallback.
Testing|Skill-tree bootstrap using selected Player wrapper.
PlayerSword|Legacy E-polling log-only skill prototype.
UI_SkillTree|Skill selection, visual links and pointer callbacks.
MaterialTintColor|Owned material tint/fade for skills presentation.
UICanvasController|Inventory/player UI toggles and tabs.
UIPointerEvents|Native pointer adapter with legacy hover compatibility.
Drop|Retained UI drop callback.
DragDrop|Inventory drag event forwarding.
HotKeysUI|Inventory hotkey icon/event refresh.
HealthChange|World health HUD, renderer flash and owner cleanup.
HealthTextDisplay|Floating health/damage text setup.
ScreenDamage|HUD damage image feedback.
DamageFlash|Renderer damage-color feedback; ownership needs review.
Tooltip|Generic pointer-bound tooltip API/extensions.
Tooltip_ItemStats|Item stats tooltip and clamped canvas placement.
Tooltip_Warning|Warning tooltip presentation.
ItemInfo|Retained item/tooltip demo component.
Window|Native pointer tooltip-window example.
Window_Items|Retained item-window example.
CameraShakeInstance|Adapted EZCameraShake state/fade simulation.
CameraShaker|Adapted EZCameraShake LateUpdate rotation offset.
CameraShaker1|Retained compatibility shake component.
CameraUtilities|Adapted EZCameraShake math helpers.
ShakeDemo|Game-facing shake entry point/demo adapter.
DialogueAction|Extensible ScriptableObject response action contract.
DialogueController|Per-player conversation lifecycle and handoff.
DialogueConversation|ScriptableObject graph/node/response data.
DialogueInteractable|Ordinary NPC adapter to Interactable.
DialogueNpc|Speaker/data/shop/service-request context.
DialoguePanel|TMP/uGUI response list and numeric shortcuts.
ServiceDialogueAction|Shop/end actions and bank/quest/service hooks.
DialogueWorldMarker|Runtime test-only scaled-time witness.
CombatCheckpointRunner|Editor combat checkpoint runner; mutates live fixture.
GameplayModernizationValidation|Editor scene/XP/interval/300-graph checks.'''
for l in text.splitlines():k,v=l.split('|',1);desc[k]=v
lines=['# Custom script index','','Current 2026-11-12 locations. Classes/namespaces/GUIDs retained. [Architecture](ARCHITECTURE.md) owns relationships; this index owns file paths/responsibilities. Imported vendor/package sources are excluded; adapted Camera code and legacy shells are identified.','',f"## Imported custom/adapted source ({len(m['scriptInventory'])} files)",'','| File | Responsibility |','| --- | --- |']
def row(path,description):return '| ['+path+'](<'+os.path.relpath(R/path,P).replace('\\','/')+'>) | '+description+' |'
for e in sorted(m['scriptInventory'],key=lambda e:e['new']):
 name=pathlib.Path(e['new']).stem;assert name in desc,name;lines.append(row(e['new'],desc[name]))
lines+=['','## Retained integration locations','','Not moved; do not treat these as deletion candidates.','','| File | Responsibility |','| --- | --- |']
for p in sorted((R/'Assets').rglob('*.cs')):
 if any(x in p.parts for x in ['Plugins','Scripts']) or p.parts[len(R.parts)+1]=='Editor':continue
 path=p.relative_to(R).as_posix();description={'ThirdPersonController':'Adapted Starter Assets movement/camera-target owner.','StarterAssetsInputs':'PlayerInput callback bridge and input reset/gating.','BasicRigidBodyPush':'Starter Assets rigidbody push adapter.','MaterialTilingOffset':'Imported BLINK editor helper, kept under Editor.','MouseCamLook':'Imported demo mouse-look.','PlayerMovement':'Imported demo movement.','ScreenshotMaker':'Imported demo screenshot helper.'}.get(p.stem,'Retained integration helper.');lines.append(row(path,description))
lines+=['','## Ephemeral Unity CLI tools','','Outside Assets intentionally; run with script run, not runtime components.','','| File | Responsibility |','| --- | --- |']
for p in sorted((R/'AgentScripts').rglob('*.cs')):
 description={'BuildMerchantDialogue':'Regenerate dedicated merchant assets/scene; not a test command.','VerifyMerchantDialogue':'Input System keyboard/control/world/shop checks.','VerifyDialogueMouse':'Temporary paired mouse and actual uGUI pointer checks.','VerifyDialogueLifecycle':'Combat cancellation, disable/reopen and service hooks.','OrganizeProjectAssets':'One-shot archived-manifest migration; do not rerun after completion.'}.get(p.stem,'Organization/reference validation utility.')
 lines.append(row(p.relative_to(R).as_posix(),description))
(P/'SCRIPT_INDEX.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
print('Indexed',len(m['scriptInventory']),'imported custom/adapted scripts')
