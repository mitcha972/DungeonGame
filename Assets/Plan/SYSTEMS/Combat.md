# Combat and enemy NPCs

Prefab organization (2026-11-13): Enemy_Main and gameplay enemies retain their Enemies paths; explicitly old Enemy-Test variants moved to Enemies/Legacy. Giant/Spider/Metalon visual source models are grouped under Models/Characters. Projectile prefabs retain Projectiles paths and are distinct from visual weapon/arrow models. See [organization record](../MAINTENANCE.md#prefab-organization--2026-11-13).

**Implemented shared prototype.** Shared code moved to Combat, animation to Combat/Animation, definitions to Combat/Data and stats to Combat/Stats. PlayerStats belongs to Player; PlayerCombat stays with shared combat. EnemyAi/EnemyStats/Enemy/EnemyInteract/EnemyAnimator are under NPC/Enemies; tutorial AI and placeholder EnemyManager remain under NPC/Legacy. No class/API/enum renames.

CharacterCombat owns accepted attacks, busy/cooldown, cancellation, block and hit recovery. CharacterAnimator selects authored weapon clips. AttackHitBox samples/deduplicates contacts; Projectile sweeps nearest hits, excludes owner, handles range/full query buffers. Ordinary projectiles call StartUp: the old zero-damage/range finding is superseded. Interval crossing and explicit SampleDamage prevent skipped windows; current-pose sampling is not a historical swept-melee simulation.

CharacterCombat handles block reduction/presentation and delegates final armor/health to CharacterStats. PlayerStats owns player UI/progression/restart; EnemyStats owns tuning/death XP/loot. Existing block reduction is authoritative; durability/interception remains incomplete. PlayerController routes current-weapon tap/hold and shield input through PlayerCombat; heavy attacks require authored support. Dialogue cancels ongoing attacks and blocks later combat input. No dual-wield implementation exists.

EnemyAi is an **Update-driven FSM**, not the coroutine-heavy version in early audits. It owns perception/memory/facing, range hysteresis, throttled complete-path calculation and stuck/NavMesh recovery using EnemyStats.Definition and NavMeshAgent. Retain that owner unless verified need justifies replacement.

[Maintenance](../MAINTENANCE.md) records current interval tests and dialogue attack cancellation. Native collision/projectile behavior, enemy navigation, frame-rate feel and balance still need targeted gameplay checks. Inspect consumers before assuming exposed collision-mask fields are applied. Vendor Outline/navigation components were not removed.

Organization rerun: existing combat-window interval cases and dialogue attack-cancellation checks actually passed. Native collision/AI scenarios were not rerun.

Enemy_Main carries QuestEnemyBridge for attributed OnDeath quest credit. NPC_Main inherits that prefab's model, animation, equipment and navigation components with hostile AI/combat/stats and quest kill reporting disabled. Tests and limitations are recorded in [quests](Quests.md).

## Legacy animation-event console errors

During the 2026-11-13 gathering-quest regression pass, the original Quest_Test run emitted `Enemy Animator` / `Punch - Right` missing-receiver errors for EnableMovement, DisableMovement, EnableDamage and DisableDamage. Identical signatures were present in earlier captured console history. Quest assertions (including controlled EnemyStats death attribution) still passed, but this is not a console-clean combat run. The resource-delivery pass did not alter combat or animation-event ownership. Next combat work must inspect the affected Animator GameObject/receiver and legacy clip events against the authoritative CharacterCombat timeline; do not blindly forward old events and accidentally add duplicate damage windows or unlock movement early.

Independent skill XP integration: melee and projectile contact paths award Combat/Archery XP from actual hostile EnemyStats health loss, retaining deduplication, armor/block reduction and overkill limits. CharacterCombat grants Blocking XP only for a successful damage-reducing live-enemy impact while blocking. Existing character kill XP remains unchanged. See [Skills](Skills.md) for rules and actual tests; this does not complete shield durability or legacy ability effects.
