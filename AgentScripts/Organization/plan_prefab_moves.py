import json,hashlib,zipfile,re
from pathlib import Path
a=json.loads(Path('Temp/prefab-inventory.json').read_text()); moves=[]
def add(p,q,reason):
 meta=Path(p+'.meta'); moves.append(dict(old=p,new=q,guid=re.search(r'^guid: (\w+)',meta.read_text(),re.M)[1],reason=reason))
folders={
'Assets/Prefabs/ToolTip':'Assets/Prefabs/UI/Tooltips',
'Assets/Prefabs/Dungeons/Dungeons':'Assets/Prefabs/Dungeons/Rooms',
'Assets/Prefabs/LowPoly_Prefabs':'Assets/Prefabs/Environment/LowPolyDungeon',
'Assets/Prefabs/LowPolyW Prefabs':'Assets/Prefabs/Models/Weapons/LowPoly',
'Assets/Prefabs/Metalon Prefabs':'Assets/Prefabs/Models/Characters/Metalon',
'Assets/Prefabs/Player/PlayerBackUp':'Assets/Prefabs/Player/Legacy/Backups',
'Assets/Prefabs/Props/Stones_Prefabs':'Assets/Prefabs/Props/RocksAndCrystals'}
for p,q in folders.items():add(p,q,'Preserve cohesive collection and folder GUID; clarify role')
direct={
'UI_SkillTree':'UI/Progression/UI_SkillTree','GAMECONTROLLER':'Core/GAMECONTROLLER','DungeonGenerator':'Dungeons/Generation/DungeonGenerator',
'Health ui':'UI/HUD/Health ui','HealthTextDisplay':'UI/HUD/HealthTextDisplay','InventorySlot':'UI/Inventory/InventorySlot','Canvas':'UI/Inventory/Canvas',
'Canv_Main':'UI/Menus/Canv_Main','Canv_Options':'UI/Menus/Canv_Options','Item':'Items/Templates/Item','Stage':'Environment/Stage','SkyBounds':'Environment/SkyBounds'}
for p,q in direct.items():add('Assets/Prefabs/'+p+'.prefab','Assets/Prefabs/'+q+'.prefab','Component/hierarchy role inspected; organize without content rename')
for r in a:
 p=r['path']; q=None; reason='Component/model role inspected'
 if p.startswith('Assets/Meshes/'):
  category=p.split('/')[2]; sub={'ArmorMeshes':'Armor','WeaponMeshes':'Weapons','ConsumableMeshes':'Consumables'}[category]
  if Path(p).stem in ['Hair','Pants','Shirt','Shoes']:sub='Clothing'
  q='Assets/Prefabs/Models/Equipment/'+sub+'/'+Path(p).name
 elif p=='Assets/GamePlay/Character/Scripts/PlayerContent.prefab':q='Assets/Prefabs/Player/Variants/StarterAssets_PlayerContent.prefab';reason='Existing variant stranded in script directory; preserve inheritance'
 elif p.startswith('Assets/GamePlay/ScriptableObjects/Enemies/'):
  if Path(p).name in ['Enemy-Test.prefab','Enemy-Test Old.prefab']:q='Assets/Prefabs/Enemies/Legacy/'+Path(p).name;reason='Legacy enemy test assemblies, retained without replacing Enemy_Main'
  else:q='Assets/Prefabs/Models/Characters/'+('Giant/' if 'Giant/' in p else 'Spider/')+Path(p).name
 elif p=='Assets/Prefabs/Enemies/OldEnemy-Test.prefab':q='Assets/Prefabs/Enemies/Legacy/OldEnemy-Test.prefab';reason='Explicitly old test prefab retained'
 elif p=='Assets/Prefabs/Player/HitDamage.prefab':q='Assets/Prefabs/UI/Feedback/HitDamage.prefab'
 elif p.startswith('Assets/Prefabs/Props/DestructibleObjects_Prefabs/'):
  name=Path(p).name
  if '/Done/' in p:q='Assets/Prefabs/Props/Destructibles/Interactive/'+name;reason='Has DestructibleObject; replaces ambiguous Done folder'
  elif '/BrokenObjects/' in p:q='Assets/Prefabs/Props/Destructibles/Debris/'+name;reason='BrokenObject debris output, not an obsolete duplicate'
  elif name in ['Box_02.prefab','Jug_02.prefab','Rock_01.prefab']:q='Assets/Prefabs/Props/Destructibles/Legacy/ImportedDuplicates/'+name;reason='Exact byte duplicate of plugin art prefab; retained distinct GUID and all references'
  else:q='Assets/Prefabs/Props/Destructibles/Unconfigured/'+name;reason='No custom DestructibleObject attached; retain as unconfigured source, not gameplay-ready'
 elif p.startswith('Assets/Prefabs/Props/') and p.count('/')==3:
  name=Path(p).name
  sub='Decorations' if Path(p).stem in ['Bone','Skull','Pick'] else 'Valuables' if Path(p).stem in ['Bullion','Crown','Emerald','Ruby','Сoin','Key'] else 'Potions'
  q='Assets/Prefabs/Props/'+sub+'/'+name
 if q:add(p,q,reason)
inventory=[]
for r in a:
 out=r['path']
 for m in moves:
  if out==m['old'] or out.startswith(m['old']+'/'):out=m['new']+out[len(m['old']):];break
 inventory.append({**r,'new':out})
path=Path('Assets/Plan/ARCHIVE/Organization/prefab-organization.json')
path.write_text(json.dumps(dict(date='2026-11-13',moves=moves,inventory=inventory,excluded='Plugins, Starter Assets camera/model prefabs and imported particle-library prefabs retained with owners'),indent=2),encoding='utf-8')
with zipfile.ZipFile('Temp/prefab-organization-before.zip','w',zipfile.ZIP_DEFLATED) as z:
 for r in a:
  z.write(r['path']);z.write(r['path']+'.meta')
 for p in folders:z.write(p+'.meta')
print(json.dumps(dict(operations=len(moves),prefabsMoved=sum(r['path']!=r['new'] for r in inventory),total=len(a),outside=[r['path'] for r in inventory if not r['new'].startswith(('Assets/Prefabs/','Assets/Plugins/','Assets/Particles/','Assets/GamePlay/Character/'))]),indent=2))
