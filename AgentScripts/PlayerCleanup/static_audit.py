import json
import re
from pathlib import Path

names = {'PlayerAnimator', 'EnemyAnimator', 'PlayerCameraController', 'PlayerAim', 'PlayerSword', 'Player', 'Testing', 'PlayerPickUpDrop', 'CharacterAnimationEventReceiver', 'FPC', 'StarterAssetsInputs'}
assets = list(Path('Assets').rglob('*'))
scripts = {}
for path in Path('Assets/Scripts').rglob('*.cs'):
    if path.stem in names:
        scripts[path] = re.search(r'guid: (\w+)', path.with_suffix('.cs.meta').read_text()).group(1)
files = [p for p in assets if p.is_file() and p.suffix in ('.prefab', '.unity', '.asset', '.controller', '.anim')]
for path, guid in scripts.items():
    hits = [str(asset) for asset in files if guid.encode() in asset.read_bytes()]
    print(str(path), json.dumps(hits))
