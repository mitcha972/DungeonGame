import pathlib,json
moves=json.loads(pathlib.Path('Assets/Plan/Validation/animation-moves.json').read_text())
for name in ['AgentScripts/Resources/BuildResourceNodes.cs','AgentScripts/Resources/InspectResources.cs']:
 p=pathlib.Path(name); s=p.read_text(encoding='utf-8-sig')
 for m in moves:s=s.replace(m['old'],m['new'])
 p.write_text(s,encoding='utf-8')
p=pathlib.Path('AgentScripts/Animations/AnimationAudit.cs');s=p.read_text();s=s.replace('new[]{"Assets/Animations","Assets/Animators"}','new[]{"Assets/Animations"}');p.write_text(s)
