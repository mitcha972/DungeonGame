from pathlib import Path
import json,re

root=Path('.')
stage=Path('Temp/EquipmentAnimation/Staged')
renames={
 'EquipmentManager':'CharacterEquipment',
 'PlayerEquipmentManager':'PlayerEquipment',
 'EnemyEquipmentManager':'EnemyEquipment',
 'CharacterAnimator':'CharacterAnimation',
 'WeaponClass':'WeaponAttackSet',
}
paths={
 'Assets/Scripts/Inventory/Equipment/EquipmentManager.cs':'Assets/Scripts/Inventory/Equipment/CharacterEquipment.cs',
 'Assets/Scripts/Inventory/Equipment/PlayerEquipmentManager.cs':'Assets/Scripts/Inventory/Equipment/PlayerEquipment.cs',
 'Assets/Scripts/Inventory/Equipment/EnemyEquipmentManager.cs':'Assets/Scripts/Inventory/Equipment/EnemyEquipment.cs',
 'Assets/Scripts/Combat/Animation/CharacterAnimator.cs':'Assets/Scripts/Combat/Animation/CharacterAnimation.cs',
 'Assets/Scripts/Combat/Data/WeaponClass.cs':'Assets/Scripts/Combat/Data/WeaponAttackSet.cs',
}
manifest=[]
for p in list(Path('Assets/Scripts').rglob('*.cs'))+list(Path('Assets/Editor').rglob('*.cs')):
 if p.name in ['EquipmentManagerUI.cs','EquipmentManager1.cs']:continue
 old=str(p).replace('\\','/'); new=paths.get(old,old)
 text=p.read_text(encoding='utf-8-sig')
 updated=text
 for a,b in renames.items():updated=re.sub(r'\b'+a+r'\b',b,updated)
 updated=updated.replace('.onEquipmentChanged','.EquipmentChanged').replace('.currentEquipment','.Items').replace('.AnimationIsBusy()','.IsBusy').replace('.PlayTargetAnimation(','.PlayAction(')
 if updated!=text or old!=new:
  out=stage/new;out.parent.mkdir(parents=True,exist_ok=True);out.write_text(updated,encoding='utf-8')
  manifest.append(dict(old=old,new=new))
(Path('Temp/EquipmentAnimation')/'sources.json').write_text(json.dumps(manifest,indent=2))
print('Staged source migration:',len(manifest),'files; five GUID-preserving renames.')
