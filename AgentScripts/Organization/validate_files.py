import pathlib,json,re,hashlib,urllib.parse,collections,os
R=pathlib.Path.cwd();P=R/'Assets/Plan';m=json.loads((P/'ARCHIVE/Organization/organization-manifest.json').read_text());errors=[];changes=[]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
allowed=set(m['pathOnlySourceEdits']);mapping={e['old']:e['new'] for e in m['scriptInventory']}
for e in m['scriptInventory']:
 p=R/e['new'];meta=p.with_name(p.name+'.meta')
 if not p.exists() or not meta.exists():errors.append('Missing script/meta: '+e['new']);continue
 if sha(meta)!=e['meta_sha256']:errors.append('Metadata bytes changed: '+e['new'])
 if re.search(r'^guid: (\w+)',meta.read_text(),re.M).group(1)!=e['guid']:errors.append('GUID changed: '+e['new'])
 if sha(p)!=e['sha256'] and e['new'] not in allowed:errors.append('Unexpected code change: '+e['new'])
for old,digest in m['protected'].items():
 new=mapping.get(old,old);p=R/new
 if not p.exists():errors.append('Protected file missing: '+old)
 elif sha(p)!=digest:
  changes.append(new)
  if new not in allowed:errors.append('Protected bytes changed: '+new)
guidmap=collections.defaultdict(list)
for p in (R/'Assets').rglob('*.meta'):
 match=re.search(r'^guid: (\w+)',p.read_text(errors='replace'),re.M)
 if match:guidmap[match.group(1)].append(p.relative_to(R).as_posix())
for e in m['scriptInventory']:
 if len(guidmap[e['guid']])!=1:errors.append('Duplicate/missing moved GUID: '+e['guid'])
links=0
for p in [R/'AGENTS.md',*P.rglob('*.md')]:
 for match in re.finditer(r'\[[^\]\n]+\]\((<[^>]+>|[^)\n]+)\)',p.read_text(encoding='utf-8-sig')):
  target=match.group(1).strip('<>');target=target.split('#',1)[0]
  if not target or re.match(r'^[a-zA-Z]+:',target):continue
  links+=1;q=(p.parent/urllib.parse.unquote(target))
  if not q.exists():errors.append('Broken link: '+p.relative_to(R).as_posix()+' -> '+target)
strays=[]
for base in ['docs','Assets/Plans','Assets/Doc']:
 d=R/base
 if d.exists():strays.extend(p.relative_to(R).as_posix() for p in d.rglob('*') if p.is_file() and p.suffix in ['.md','.txt','.json'])
if strays:errors.append('Unconsolidated project docs: '+str(strays))
report={'passed':not errors,'scriptsChecked':len(m['scriptInventory']),'scriptMoves':len(m['scriptMoves']),'guidAndMetadataPreserved':not any('GUID' in e or 'Metadata' in e for e in errors),'protectedFilesChecked':len(m['protected']),'expectedPathOnlySourceChanges':changes,'markdownLinksChecked':links,'errors':errors}
(P/'ARCHIVE/Organization/filesystem-validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8');print(json.dumps(report,indent=2));raise SystemExit(bool(errors))
