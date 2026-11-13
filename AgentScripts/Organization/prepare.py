import pathlib,re,json,hashlib,zipfile,collections
R=pathlib.Path.cwd(); A=R/'Assets'; out=A/'Plan/ARCHIVE/Organization';out.mkdir(parents=True,exist_ok=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def rel(p):return p.relative_to(R).as_posix()
# Inventory all project-authored planning/report content, including ignored checkpoints.
docs=[]
for base in ['docs','Assets/Plans','Assets/Plan','Assets/Doc','GameplayCheckpoints']:
 for p in (R/base).rglob('*'):
  if not p.is_file() or 'Organization_20261112' in p.parts or 'Organization' in p.parts:continue
  if p.suffix.lower() in ['.md','.txt','.json']: docs.append(p)
docs.append(R/'SCRIPT_ORGANIZATION_AUDIT.json')
basefiles=set(docs)
for folder in ['Assets/Scripts','Assets/Editor','AgentScripts']:
 for p in (R/folder).rglob('*'):
  if p.is_file() and 'Organization' not in p.parts:basefiles.add(p)
for p in docs:
 m=p.with_name(p.name+'.meta')
 if m.exists():basefiles.add(m)
basefiles.add(R/'ProjectSettings/EditorBuildSettings.asset')
backup=R/'GameplayCheckpoints/Organization_20261112/restore.zip';backup.parent.mkdir(parents=True,exist_ok=True)
assert not backup.exists(),'Restore archive already exists; do not overwrite'
with zipfile.ZipFile(backup,'w',zipfile.ZIP_DEFLATED) as z:
 for p in sorted(basefiles): z.write(p,rel(p))
# Retain third-party/adapted Camera and Starter Assets locations. No assembly boundary changes.
manual={
'Controller.cs':'Core/Legacy/Controller.cs','GameController.cs':'Core/GameController.cs','LoadingScript.cs':'Core/Scenes/LoadingScript.cs','MenuController.cs':'UI/Menus/MenuController.cs','SettingsMenu.cs':'UI/Menus/SettingsMenu.cs','Drop.cs':'UI/Inventory/Drop.cs','DamageFlash.cs':'UI/Feedback/DamageFlash.cs','DoorScript.cs':'World/Interactions/DoorScript.cs','EnemyAnimator.cs':'NPC/Enemies/EnemyAnimator.cs','HealthTextDisplay.cs':'UI/HUD/HealthTextDisplay.cs','MapController.cs':'World/Map/MapController.cs','ResetAnimatorBool.cs':'Combat/Animation/ResetAnimatorBool.cs',
'Controllers/CharacterAnimator.cs':'Combat/Animation/CharacterAnimator.cs','Controllers/CharacterAnimationEventReceiver.cs':'Combat/Animation/CharacterAnimationEventReceiver.cs','Controllers/FPC.cs':'Player/Legacy/FPC.cs','Controllers/CameraController.cs':'Player/Legacy/CameraController.cs','Controllers/UICanvasController.cs':'UI/UICanvasController.cs',
'GamePlay/Abilities.cs':'Combat/Data/Abilities.cs','GamePlay/AttackHitBox.cs':'Combat/AttackHitBox.cs','GamePlay/Class.cs':'Player/Data/Class.cs','GamePlay/DungeonExit.cs':'World/Dungeon/DungeonExit.cs','GamePlay/Loot.cs':'Inventory/Items/Loot.cs','GamePlay/PlayerAim.cs':'Player/PlayerAim.cs','GamePlay/PlayerDamageFlash.cs':'Player/PlayerDamageFlash.cs','GamePlay/Player/EnemyManager.cs':'NPC/Legacy/EnemyManager.cs','GamePlay/Player/PlayerMotor.cs':'Player/Legacy/PlayerMotor.cs','GamePlay/Player/PlayerManager.cs':'Player/Legacy/PlayerManager.cs','GamePlay/Stats/PlayerStats.cs':'Player/PlayerStats.cs','GamePlay/Stats/EnemyStats.cs':'NPC/Enemies/EnemyStats.cs','GamePlay/WorldInteracts/ChestScript.cs':'Inventory/Chests/ChestScript.cs','GamePlay/WorldInteracts/ShopScript.cs':'Shops/ShopScript.cs','Dialogue/DialogueWorldMarker.cs':'Tests/Runtime/DialogueWorldMarker.cs'}
scriptmoves=[]; inventory=[]
for p in sorted((A/'Scripts').rglob('*.cs')):
 old=p.relative_to(A/'Scripts').as_posix(); n=p.name;new=manual.get(old,old)
 if old.startswith('Controllers/Player'):new='Player/'+n
 elif old.startswith('Dungeon/'):new='World/Dungeon/'+n
 elif old.startswith('GamePlay/Combat/'):new=('Combat/Data/' if n=='WeaponClass.cs' else 'Combat/')+n
 elif old.startswith('GamePlay/Enemy/'):new=('NPC/Legacy/' if n=='EnemyAiTutorial.cs' else 'NPC/Enemies/')+n
 elif old.startswith('GamePlay/Inventory/'):new=('Shops/' if n.startswith('Shop') else 'Inventory/Chests/' if n.startswith('Chest') else 'Inventory/UI/' if n!='Inventory.cs' else 'Inventory/')+n
 elif old.startswith('GamePlay/Items/'):
  new=('World/Interactions/' if n in ['Interactable.cs','ItemInteract.cs'] else 'Inventory/Equipment/' if 'EquipmentManager' in n else 'Inventory/Items/')+n
 elif old.startswith('GamePlay/Player/') and old not in manual:new='Player/'+n
 elif old.startswith('GamePlay/Stats/') and old not in manual:new='Combat/Stats/'+n
 elif old.startswith('Systems/DestructibleObject_Scripts/'):new='World/Destructibles/'+n
 elif old.startswith('Systems/DragAndDrrop/'):new='World/Grabbing/'+n
 elif old.startswith('Systems/Helper/'):new='World/Utilities/'+n
 elif old.startswith('Systems/LevelingSystemScripts/'):new='Progression/Leveling/'+n
 elif old.startswith('Systems/SkillTree/'):new='Progression/Skills/'+n
 elif old.startswith('Systems/ToolTip/'):new='UI/Tooltips/'+n
 elif old.startswith('UI/') and n!='UIPointerEvents.cs':new=('UI/Inventory/' if n in ['DragDrop.cs','HotKeysUI.cs'] else 'UI/HUD/')+n
 dest='Assets/Scripts/'+new
 m=p.with_name(n+'.meta');guid=re.search(r'^guid: (\w+)',m.read_text(),re.M).group(1)
 entry={'old':rel(p),'new':dest,'guid':guid,'sha256':sha(p),'meta_sha256':sha(m)};inventory.append(entry)
 if rel(p)!=dest:scriptmoves.append(entry)
for p in sorted((A/'Editor').glob('*.cs')):
 m=p.with_name(p.name+'.meta');e={'old':rel(p),'new':'Assets/Editor/Validation/'+p.name,'guid':re.search(r'^guid: (\w+)',m.read_text(),re.M).group(1),'sha256':sha(p),'meta_sha256':sha(m)};inventory.append(e);scriptmoves.append(e)
assert len({x['new'] for x in inventory})==len(inventory)
assetdocs=[];externaldocs=[];duplicates=[]
for p in docs:
 old=rel(p)
 if old.startswith('Assets/Plan/'):continue
 if old in ['docs/COMPLETE_CODEBASE_AUDIT.txt','GameplayCheckpoints/BeforeDirective02_20261108_131002/Assets/Doc/COMPLETE_CODEBASE_AUDIT.txt']:
  canonical=R/'Assets/Doc/COMPLETE_CODEBASE_AUDIT.txt';assert p.read_bytes()==canonical.read_bytes();duplicates.append({'old':old,'new':'Assets/Plan/ARCHIVE/History/COMPLETE_CODEBASE_AUDIT.txt','decision':'REMOVE exact duplicate; canonical and restore ZIP preserve full content','sha256':sha(p)});continue
 if old.startswith(('Assets/Plans/','Assets/Doc/','docs/')):
  target='Assets/Plan/ARCHIVE/History/'+p.name
  if p.name=='DIALOGUE_SYSTEM.md':target='Assets/Plan/ARCHIVE/History/DIALOGUE_IMPLEMENTATION_20261112.md'
  d={'old':old,'new':target,'decision':'MERGE current facts into authoritative docs; ARCHIVE historical original','sha256':sha(p)}
 elif old=='SCRIPT_ORGANIZATION_AUDIT.json':d={'old':old,'new':'Assets/Plan/ARCHIVE/History/SCRIPT_ORGANIZATION_AUDIT.json','decision':'ARCHIVE superseded 32-move proposal','sha256':sha(p)}
 else:
  tail=old.removeprefix('GameplayCheckpoints/');d={'old':old,'new':'Assets/Plan/ARCHIVE/Evidence/'+tail,'decision':'ARCHIVE historical test evidence; never treat failures as current passes','sha256':sha(p)}
 (assetdocs if old.startswith('Assets/') else externaldocs).append(d)
protected={}
for p in A.rglob('*'):
 if not p.is_file() or 'Plan' in p.parts:continue
 if p.suffix.lower() in ['.cs','.prefab','.unity','.asset','.anim','.controller','.overridecontroller','.inputactions','.asmdef','.asmref']:
  protected[rel(p)]=sha(p)
manifest={'date':'2026-11-12','restore':rel(backup),'scriptInventory':inventory,'scriptMoves':scriptmoves,'assetDocuments':assetdocs,'externalDocuments':externaldocs,'duplicates':duplicates,'protected':protected,'documentsReviewed':[{'path':rel(p),'sha256':sha(p)} for p in docs],'sceneAliases':{'Assets/Scenes/Main.unity':'Assets/Scenes/Game/Start.unity','Assets/Scenes/Loading.unity':'Assets/Scenes/Game/Loading.unity','Assets/Scenes/DungeonFloorTest.unity':'Assets/Scenes/Game/Main.unity','Assets/Scenes/Test.unity':'Assets/Scenes/Test_Scenes/Combat_Test.unity','Assets/Scenes/DialogueMerchantTest.unity':'Assets/Scenes/Test_Scenes/DialogueMerchant_Test.unity','Assets/Scenes/DungeonGeneration.unity':'Assets/Scenes/Test_Scenes/DungeonGeneration_Test.unity','Assets/Scenes/SkillTree.unity':'Assets/Scenes/Test_Scenes/SkillTree_Test.unity'}}
(out/'organization-manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print(json.dumps({'customScripts':len(inventory),'scriptMoves':len(scriptmoves),'assetDocs':len(assetdocs),'externalDocs':len(externaldocs),'duplicates':len(duplicates),'protectedAssets':len(protected),'restore':str(backup)},indent=2))
