# Combat and enemy NPCs

**Implemented shared prototype.** Shared code moved to Combat, animation to Combat/Animation, definitions to Combat/Data and stats to Combat/Stats. PlayerStats belongs to Player; PlayerCombat stays with shared combat. EnemyAi/EnemyStats/Enemy/EnemyInteract/EnemyAnimator are under NPC/Enemies; tutorial AI and placeholder EnemyManager remain under NPC/Legacy. No class/API/enum renames.

CharacterCombat owns accepted attacks, busy/cooldown, cancellation, block and hit recovery. CharacterAnimator selects authored weapon clips. AttackHitBox samples/deduplicates contacts; Projectile sweeps nearest hits, excludes owner, handles range/full query buffers. Ordinary projectiles call StartUp: the old zero-damage/range finding is superseded. Interval crossing and explicit SampleDamage prevent skipped windows; current-pose sampling is not a historical swept-melee simulation.

CharacterCombat handles block reduction/presentation and delegates final armor/health to CharacterStats. PlayerStats owns player UI/progression/restart; EnemyStats owns tuning/death XP/loot. Existing block reduction is authoritative; durability/interception remains incomplete. PlayerController routes current-weapon tap/hold and shield input through PlayerCombat; heavy attacks require authored support. Dialogue cancels ongoing attacks and blocks later combat input. No dual-wield implementation exists.

EnemyAi is an **Update-driven FSM**, not the coroutine-heavy version in early audits. It owns perception/memory/facing, range hysteresis, throttled complete-path calculation and stuck/NavMesh recovery using EnemyStats.Definition and NavMeshAgent. Retain that owner unless verified need justifies replacement.

[Maintenance](../MAINTENANCE.md) records current interval tests and dialogue attack cancellation. Native collision/projectile behavior, enemy navigation, frame-rate feel and balance still need targeted gameplay checks. Inspect consumers before assuming exposed collision-mask fields are applied. Vendor Outline/navigation components were not removed.

Organization rerun: existing combat-window interval cases and dialogue attack-cancellation checks actually passed. Native collision/AI scenarios were not rerun.
