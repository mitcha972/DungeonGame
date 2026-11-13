import pathlib,json
root=pathlib.Path('Assets/Animations')
moves=[]
for p in [*root.rglob('*'),*pathlib.Path('Assets/Animators').rglob('*')]:
 if not p.is_file() or p.suffix=='.meta': continue
 s=p.as_posix(); rel=s.removeprefix('Assets/Animations/'); to=None
 if s.startswith('Assets/Animators/'):
  kind='Masks' if p.suffix=='.mask' else ('Legacy' if p.name.startswith('[Test]') else 'Characters')
  to='Assets/Animations/Controllers/'+kind+'/'+p.name
 elif rel=='StarterAssetsThirdPerson.controller':to='Assets/Animations/Controllers/Legacy/Standalone_StarterAssetsThirdPerson.controller'
 elif rel.startswith('Character/'):to='Assets/Animations/Humanoid/Locomotion/StarterAssets/'+rel[len('Character/'):]
 elif rel.startswith('Movement/Giant/'):to='Assets/Animations/Monsters/Giant/Source/Legacy/'+rel[len('Movement/Giant/'):]
 elif rel.startswith('Movement/'):to='Assets/Animations/Humanoid/Locomotion/'+rel[len('Movement/'):]
 elif rel.startswith('Exploration/'):to='Assets/Animations/Humanoid/Locomotion/Directional/'+rel[len('Exploration/'):]
 elif rel.startswith('Combat/'):to='Assets/Animations/Humanoid/'+rel
 elif rel.startswith('Effects/Hit/') or rel.startswith('Effects/Death/'):to='Assets/Animations/Humanoid/Reactions/'+rel[len('Effects/'):]
 elif rel.startswith('Effects/'):to='Assets/Animations/Humanoid/Abilities/Source/'+p.name
 elif rel.startswith('Magic/'):to='Assets/Animations/Humanoid/Abilities/Clips/'+p.name
 elif rel.startswith('Misc/Door/'):to='Assets/Animations/Environment/Doors/'+p.name
 elif rel.startswith('Misc/'):
  group='Gathering/' if p.name in ['Gathering.anim','MiningLoop.anim'] else ''
  to='Assets/Animations/Humanoid/Actions/'+group+p.name
 elif rel.startswith('Giant/'):to='Assets/Animations/Monsters/Giant/Source/'+rel[len('Giant/'):]
 elif rel.startswith('Monsters/GiantAnimatorControllers/'):to='Assets/Animations/Monsters/Giant/Controllers/'+p.name
 elif rel.startswith('Monsters/Spider Animations/'):
  group='Controllers' if p.suffix in ['.controller','.overrideController'] else ('Rig' if p.suffix=='.asset' else 'Clips')
  to='Assets/Animations/Monsters/Spider/'+group+'/'+p.name
 elif rel.startswith('Monsters/'):
  parts=rel.split('/')
  if len(parts)==2:to='Assets/Animations/Controllers/Legacy/'+p.name
  elif len(parts)==3:
   group='Controllers' if p.suffix in ['.controller','.overrideController'] else 'Clips'
   to='Assets/Animations/'+('/'.join(parts[:2]))+'/'+group+'/'+p.name
 if to and to!=s:
  guid=next(l.split(': ')[1] for l in pathlib.Path(s+'.meta').read_text().splitlines() if l.startswith('guid:'))
  moves.append(dict(old=s,new=to,guid=guid))
assert len({x['new'] for x in moves})==len(moves)
pathlib.Path('Assets/Plan/Validation/animation-moves.json').write_text(json.dumps(moves,indent=2))
print(len(moves),'asset moves planned')
