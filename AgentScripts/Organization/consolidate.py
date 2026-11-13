import pathlib,json,hashlib,os,re,shutil,urllib.parse
R=pathlib.Path.cwd(); P=R/'Assets/Plan'; M=P/'ARCHIVE/Organization/organization-manifest.json';m=json.loads(M.read_text())
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(path,text):
 p=R/path;p.parent.mkdir(parents=True,exist_ok=True);p.write_text(text.strip()+'\n',encoding='utf-8')
# Finish external document moves without changing source-code snapshots.
for e in m['externalDocuments']:
 src=R/e['old'];dst=R/e['new'];assert sha(src)==e['sha256'],src;assert not dst.exists(),dst;dst.parent.mkdir(parents=True,exist_ok=True);shutil.move(str(src),str(dst))
for e in m['duplicates']:
 src=R/e['old'];dst=R/e['new'];assert sha(src)==sha(dst)==e['sha256'];src.unlink();meta=src.with_name(src.name+'.meta')
 if meta.exists():meta.unlink()
# Ephemeral builders/tests stay outside Assets, hence outside runtime compilation.
agentmoves=[]
for src in sorted((R/'AgentScripts').glob('*.cs')):
 dst=R/'AgentScripts/Dialogue'/src.name;dst.parent.mkdir(parents=True,exist_ok=True);assert not dst.exists();shutil.move(str(src),str(dst));agentmoves.append({'old':src.relative_to(R).as_posix(),'new':dst.relative_to(R).as_posix()})
m['agentMoves']=agentmoves
# Only literal path/output migrations; no gameplay algorithms or class/API changes.
pathmap={e['old']:e['new'] for e in m['scriptInventory']+m['assetDocuments']+m['externalDocuments']+m['duplicates']+agentmoves};pathmap.update(m['sceneAliases']);edits=[]
for p in [*(R/'Assets/Scripts').rglob('*.cs'),*(R/'Assets/Editor').rglob('*.cs'),*(R/'AgentScripts/Dialogue').glob('*.cs')]:
 before=p.read_bytes();after=before
 for old,new in sorted(pathmap.items(),key=lambda e:-len(e[0])):after=after.replace(old.encode(),new.encode())
 if p.name=='SettingsMenu.cs':after=after.replace(b'LoadScene("Main")',b'LoadScene("Start")')
 if p.name=='GameplayModernizationValidation.cs':
  after=after.replace(b'Directory.CreateDirectory("docs")',b'Directory.CreateDirectory("Assets/Plan/ARCHIVE/Evidence/Current")').replace(b'"docs/GAMEPLAY_MODERNIZATION_VALIDATION.txt"',b'"Assets/Plan/ARCHIVE/Evidence/Current/GAMEPLAY_MODERNIZATION_VALIDATION.txt"')
 if p.name=='CombatCheckpointRunner.cs':
  after=after.replace(b'Directory.CreateDirectory("GameplayCheckpoints")',b'Directory.CreateDirectory("Assets/Plan/ARCHIVE/Evidence/Current")').replace(b'"GameplayCheckpoints/CombatTest.txt"',b'"Assets/Plan/ARCHIVE/Evidence/Current/CombatTest.txt"')
 if after!=before:p.write_bytes(after);edits.append(p.relative_to(R).as_posix())
m['pathOnlySourceEdits']=edits
# Rebase links in archived Markdown; raw original bytes remain in restore.zip.
missing_history=[]
for e in m['assetDocuments']+m['externalDocuments']:
 dst=R/e['new']
 if dst.suffix!='.md':continue
 text=dst.read_text(encoding='utf-8-sig');oldparent=(R/e['old']).parent
 def fix(match):
  label,target=match.groups();target=target.strip('<>')
  if re.match(r'^[a-z]+:',target) or target.startswith('#'):return match.group(0)
  pathpart,sep,fragment=target.partition('#');candidate=pathlib.Path(os.path.normpath(str(oldparent/urllib.parse.unquote(pathpart))))
  try:old=candidate.relative_to(R).as_posix()
  except ValueError:return label+' (historical external path)'
  new=pathmap.get(old,old)
  # The old audit referred to the pre-extraction generator and removed examples.
  if old=='Assets/Plugins/DungeonGenerator/DungeonGenerator_Scripts/DungeonGenerator.cs':new='Assets/Scripts/World/Dungeon/DungeonGenerator.cs'
  exists=(R/new).exists()
  if not exists:missing_history.append({'document':e['old'],'target':target});return label+' (`'+target+'`; historical target no longer exists)'
  link=os.path.relpath(R/new,dst.parent).replace('\\','/')
  return '['+label+'](<'+link+(' #'+fragment if False else '#'+fragment if sep else '')+'>)'
 text=re.sub(r'\[([^\]\n]+)\]\(([^)\n]+)\)',fix,text)
 banner='> Historical snapshot. Superseded by [current project context](../../PROJECT_CONTEXT.md) and the subsystem references. Original requirements, file names, counts, and test limits below describe the earlier task, not current instructions. Original bytes are preserved in the organization restore ZIP.\n\n'
 dst.write_text(banner+text,encoding='utf-8')
m['historicalMissingTargetsConvertedToText']=missing_history
M.write_text(json.dumps(m,indent=2),encoding='utf-8')
print(json.dumps({'externalDocsArchived':len(m['externalDocuments']),'exactDuplicateCopiesRemoved':len(m['duplicates']),'ephemeralToolsMoved':len(agentmoves),'pathOnlySourceEdits':edits,'historicalDeadLinksAnnotated':len(missing_history)},indent=2))
