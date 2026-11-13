# Progression and skills

**XP/skill-state infrastructure implemented; skill effects mostly prototypes.** Files moved to Progression/Leveling and Progression/Skills. PlayerStats remains the main player state owner.

LevelSystem ignores nonpositive XP, uses a long intermediate for large awards, emits one event per level crossed and one XP event per award, and discards excess at cap. Levels are zero-based 0â€“39, normally displayed 1â€“40; thresholds are unchanged. LevelSystemAnimated uses the local LevelSystemAnimatedUpdater and explicitly disposes subscriptions; CodeMonkey FunctionUpdater no longer drives it.

PlayerStats owns XP/PlayerSkills and receives enemy kill XP. Retained Player delegates GetPlayerSkills to co-located PlayerStats and creates separate state only for a standalone demo. The historical unconditional duplicate-state claim is superseded. A Testing bootstrap assigned to another standalone Player can still target a different demo context, so inspect actual scene wiring.

PlayerSkills owns points/prerequisites/unlocks; repeated/None unlocks do not spend points. UI_SkillTree consumes that state. PlayerSword still polls E and logs selected skill use: it is not a complete ability system and does not itself respect dialogue eligibility. This limitation is recorded, not changed by folder organization.

Source verifies bounds/disposal/delegation. Historical standalone tests cover XP boundaries/overflow; the Editor validator repeats them. [Maintenance](../MAINTENANCE.md) owns current results. Unfinished: designed effects, production bootstrap validation, persistence and an explicit penalty API if negative XP is later required.

Organization rerun: the existing Editor XP boundary/overflow/event tests actually passed. Full skill effects are still unimplemented.
