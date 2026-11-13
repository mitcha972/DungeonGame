import json,hashlib,re
from pathlib import Path
m=json.loads(Path('Assets/Plan/ARCHIVE/Organization/prefab-organization.json').read_text(encoding='utf-8'))
errors=[]
for r in m['inventory']:
 p=Path(r['new']);meta=Path(str(p)+'.meta')
 if not p.exists() or not meta.exists():errors.append('Missing '+str(p));continue
 if hashlib.sha256(p.read_bytes()).hexdigest()!=r['hash']:errors.append('Content changed '+str(p))
 if hashlib.sha256(meta.read_bytes()).hexdigest()!=r['metaHash']:errors.append('Metadata changed '+str(p))
 if re.search(r'^guid: (\w+)',meta.read_text(),re.M)[1]!=r['guid']:errors.append('GUID changed '+str(p))
assert not errors,errors
report=dict(passed=True,prefabsChecked=len(m['inventory']),prefabsMoved=sum(r['path']!=r['new'] for r in m['inventory']),unchangedPrefabBytes=True,unchangedMetaBytes=True,errors=errors)
Path('Assets/Plan/ARCHIVE/Organization/prefab-filesystem-validation.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report))
# Maintain live tool paths only; historical evidence remains unchanged.
for p in Path('AgentScripts').rglob('*.cs'):
 s=p.read_text(encoding='utf-8-sig');out=s
 for r in m['inventory']:
  if r['path']!=r['new']:out=out.replace('"'+r['path']+'"','"'+r['new']+'"')
 if out!=s:p.write_text(out,encoding='utf-8')
