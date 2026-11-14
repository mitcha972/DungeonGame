# Combat and enemy NPCs

Prefab organization (2026-11-13): Enemy_Main and gameplay enemies retain their Enemies paths; explicitly old Enemy-Test variants moved to Enemies/Legacy. Giant/Spider/Metalon visual source models are grouped under Models/Characters. Projectile prefabs retain Projectiles paths and are distinct from visual weapon/arrow models. See [organization record](../MAINTENANCE.md#prefab-organization--2026-11-13).

**Implemented shared prototype.** Shared code moved to Combat, animation to Combat/Animation, definitions to Combat/Data and stats to Combat/Stats. PlayerStats belongs to Player; PlayerCombat stays with shared combat. EnemyAi/EnemyStats/Enemy/EnemyInteract/EnemyAnimator are under NPC/Enemies; tutorial AI and placeholder EnemyManager remain under NPC/Legacy. No class/API/enum renames.

CharacterCombat owns accepted attacks, busy/cooldown, cancellation, block and hit recovery. CharacterAnimator selects authored weapon clips. AttackHitBox samples/deduplicates contacts; Projectile sweeps nearest hits, excludes owner, handles range/full query buffers. Ordinary projectiles call StartUp: the old zero-damage/range finding is superseded. Interval crossing and explicit SampleDamage prevent skipped windows; current-pose sampling is not a historical swept-melee simulation.

CharacterCombat handles block reduction/presentation and delegates final armor/health to CharacterStats. PlayerStats owns player UI/progression/restart; EnemyStats owns tuning/death XP/loot. Active frontal blocking subtracts the equipped shield armorModifier from incoming damage before normal armor, with a minimum of 1 final damage; durability remains incomplete. PlayerController sends one light attack on left-button press and holds right-button shield block through PlayerCombat; holding/releasing attack does not produce heavy or additional attacks. Dialogue cancels ongoing attacks and blocks later combat input. No dual-wield implementation exists.

EnemyAi is an **Update-driven FSM**, not the coroutine-heavy version in early audits. It owns perception/memory/facing, range hysteresis, throttled complete-path calculation and stuck/NavMesh recovery using EnemyStats.Definition and NavMeshAgent. Retain that owner unless verified need justifies replacement.

[Maintenance](../MAINTENANCE.md) records current interval tests and dialogue attack cancellation. Native collision/projectile behavior, enemy navigation, frame-rate feel and balance still need targeted gameplay checks. Inspect consumers before assuming exposed collision-mask fields are applied. Vendor Outline/navigation components were not removed.

Organization rerun: existing combat-window interval cases and dialogue attack-cancellation checks actually passed. Native collision/AI scenarios were not rerun.

Enemy_Main carries QuestEnemyBridge for attributed OnDeath quest credit. NPC_Main inherits that prefab's model, animation, equipment and navigation components with hostile AI/combat/stats and quest kill reporting disabled. Tests and limitations are recorded in [quests](Quests.md).

## Legacy animation-event console errors

**Superseded by the 2026-11-13 animation pass:** obsolete damage/movement/projectile/AttackHitEvent callbacks were removed from twelve project-owned clips using AnimationUtility; CharacterCombat remains the timing owner. A real accepted Enemy_Main attack played Punch - Right on the child Animator without receiver errors. See [animation/models](Animation.md) for fixes, remaining monster/controller content gaps and validation scope. The following paragraph preserves the original finding.

During the 2026-11-13 gathering-quest regression pass, the original Quest_Test run emitted `Enemy Animator` / `Punch - Right` missing-receiver errors for EnableMovement, DisableMovement, EnableDamage and DisableDamage. Identical signatures were present in earlier captured console history. Quest assertions (including controlled EnemyStats death attribution) still passed, but this is not a console-clean combat run. The resource-delivery pass did not alter combat or animation-event ownership. Next combat work must inspect the affected Animator GameObject/receiver and legacy clip events against the authoritative CharacterCombat timeline; do not blindly forward old events and accidentally add duplicate damage windows or unlock movement early.

Independent skill XP integration: melee and projectile contact paths award Combat/Archery XP from actual hostile EnemyStats health loss, retaining deduplication, armor/block reduction and overkill limits. CharacterCombat grants Blocking XP only for a successful damage-reducing live-enemy impact while blocking. Existing character kill XP remains unchanged. See [Skills](Skills.md) for rules and actual tests; this does not complete shield durability or legacy ability effects.

## Simplified combat — 2026-11-13

Implemented in PlayerController/PlayerCombat: left press immediately requests one light attack; holding never repeats or charges, release never attacks, and right press is shield-only. PlayerCombat coerces legacy heavy requests to light while shared animation/data compatibility remains intact. Modal/focus/death input gates and cooldowns are retained.

CharacterAnimator busy state still controls attack timing/readiness, but no longer disables player movement or rotation. Non-player movement events and gathering's PlayerControlState action lease retain their existing ownership.

AttackHitBox renders a translucent orange box at the exact sampled animated weapon bounds (fallback: BoxCollider bounds), only during active damage windows. Projectile adds a visible box around its world-space collision radius while in flight; existing projectile meshes remain. Runtime materials are cleaned up. Arrow data now explicitly enables ranged execution and retains speed 35/range 40. The Arrow prefab Projectile component is enabled (it was disabled, leaving arrows flying without sweep/range updates).

CharacterCombat requires both active blocking and an equipped off-hand Shield before reducing an impact marked frontal by melee/projectile contact. Reduction uses shield armorModifier as a flat extra reduction, followed by existing passive armor; minimum damage stays 1. Shield armor still contributes passive armor while equipped. Release, loss of shield, modal state and death drop player block. Skill XP remains tied to actual damage-reducing blocks.

Validation: see the final results below and [runtime evidence](../ARCHIVE/Evidence/SimpleCombatValidation.txt). Controlled checks use Combat_Test, existing Player, a runtime Enemy_Main, actual input/timelines/collisions, and enlarged target colliders to isolate weapon execution from aiming precision. This fixture does not certify production navigation or monster rig/controller content.

Final clean-session result: **40 assertions passed, 0 failed** in Combat_Test. Covered player unarmed, Wood Sword, Iron Sword and Bow; enemy Slash/Arrow/Unarmed acceptance and real player health loss; one light attack for a three-second hold with no release attack; visible active volumes; movement/rotation eligibility throughout attacks and actual W displacement; Wood/Iron shield active/inactive/release cases; minimum-one damage and no-shield rejection. Native physics used normal scene scenery in the final run, with enlarged test target colliders. Console ground-truth at completion: 0 errors, 0 warnings; compilationFailed=false. Editor returned to stopped Combat_Test without saving runtime fixtures. Earlier interrupted/hot-reload runs were discarded; final evidence is authoritative. Remaining: production feel/balance and monster-specific rig/controller coverage listed above.

Temporary effects now integrate through CharacterStats derived movement/damage values and CharacterCombat attack snapshots; DoT uses existing health/armor/death rather than attack hit recovery. EnemyAi consumes the movement multiplier. See [status effects](StatusEffects.md) for configuration, policies and validation.
