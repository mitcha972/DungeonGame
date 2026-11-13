from pathlib import Path
import re,json,hashlib
root=Path('Assets')
scripts={}
for p in root.rglob('*.cs.meta'):
 m=re.search(r'^guid: (\w+)',p.read_text(encoding='utf-8-sig'),re.M)
 if m:scripts[m[1]]=str(p)[:-5].replace('\\','/')
rows=[]
for p in root.rglob('*.prefab'):
 s=p.read_text(encoding='utf-8-sig',errors='replace'); meta=Path(str(p)+'.meta'); guid=re.search(r'^guid: (\w+)',meta.read_text(),re.M)[1]
 rows.append(dict(path=p.as_posix(),guid=guid,hash=hashlib.sha256(p.read_bytes()).hexdigest(),metaHash=hashlib.sha256(meta.read_bytes()).hexdigest(),scripts=sorted(set(scripts.get(g,g) for g in re.findall(r'm_Script:.*guid: (\w+)',s))),bases=sorted(set(re.findall(r'm_SourcePrefab:.*guid: (\w+)',s))),names=re.findall(r'^  m_Name: (.+)',s,re.M)[:5]))
Path('Temp/prefab-inventory.json').write_text(json.dumps(rows,indent=2))
for r in rows:
 if not r['path'].startswith('Assets/Plugins/'):
  print(r['path']+' | '+','.join(Path(s).stem for s in r['scripts'])+' | '+','.join(r['names'][:2]))
groups={}
for r in rows:groups.setdefault(r['hash'],[]).append(r['path'])
print('EXACT DUPLICATES',json.dumps([v for v in groups.values() if len(v)>1]))

