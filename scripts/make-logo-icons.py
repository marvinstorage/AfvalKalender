import struct, io, sys
from PIL import Image, ImageDraw
src = sys.argv[1] if len(sys.argv) > 1 else "assets/logo-source.jpg"  # run from the repo root
im = Image.open(src).convert('RGB')

# 1. Rounded-square logo (the card itself)
card = im.crop((78, 83, 946, 951)).convert('RGBA')
m = Image.new('L', card.size, 0)
ImageDraw.Draw(m).rounded_rectangle((0, 0, card.size[0]-1, card.size[1]-1), radius=165, fill=255)
card.putalpha(m)
logo = card.resize((512, 512), Image.LANCZOS)
logo.save('assets/logo.png')
logo.resize((256, 256), Image.LANCZOS).save('assets/logo-256.png')

# ICO with PNG entries
sizes = [16, 32, 48, 256]
blobs = []
for s in sizes:
    b = io.BytesIO(); card.resize((s, s), Image.LANCZOS).save(b, 'PNG'); blobs.append(b.getvalue())
out = struct.pack('<HHH', 0, 1, len(sizes))
off = 6 + 16*len(sizes)
for s, d in zip(sizes, blobs):
    out += struct.pack('<BBBBHHII', s % 256, s % 256, 0, 0, 1, 32, len(d), off); off += len(d)
open('AfvalKalender.DesktopUI/Assets/app-logo.ico', 'wb').write(out + b''.join(blobs))

# 2. Adaptive-icon foreground: artwork without the card background
fg = im.crop((112, 100, 912, 900)).convert('RGBA')
px = fg.load(); w, h = fg.size
light = Image.new('L', fg.size, 0); lp = light.load()
for y in range(h):
    for x in range(w):
        r, g, b, _ = px[x, y]
        if min(r, g, b) >= 190 and max(r, g, b) - min(r, g, b) <= 16: lp[x, y] = 255
for seed in [(0,0),(w-1,0),(0,h-1),(w-1,h-1),(w//2,0),(w//2,h-1),(0,h//2),(w-1,h//2)]:
    if lp[seed] == 255: ImageDraw.floodfill(light, seed, 128)
for y in range(h):
    for x in range(w):
        if lp[x, y] == 128: px[x, y] = (255, 255, 255, 0)
fg.save('assets/logo-foreground.png')
