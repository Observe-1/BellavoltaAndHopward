#!/usr/bin/env python3
"""Render Bellavolta's app icon set from code (Porto Chiaro palette), no source artwork.

    python3 Tools/testflight/make-icon.py        # writes Tools/testflight/AppIcon.appiconset

Needs Pillow. Drawn at 4096 px and downsampled per size, so small sizes stay clean.
Opaque RGB, as App Store Connect rejects an alpha channel on the 1024 px icon.
"""
import json, math, os
from PIL import Image, ImageDraw, ImageFilter

S = 4096
HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, "AppIcon.appiconset")

def hx(s): return tuple(int(s[i:i + 2], 16) for i in (0, 2, 4))
def mix(a, b, t): return tuple(round(a[i] + (b[i] - a[i]) * t) for i in range(3))

SKY_TOP, SKY_LOW = hx("F7A07E"), hx("FBC9A0")
SUN, SUN_GLOW = hx("FDE7C2"), hx("FBD2A6")
RIDGE_FAR, RIDGE_MID, HILL = hx("A4A4C9"), hx("8F92BE"), hx("7780A3")
SEA, SEA_HORIZON, SEA_STREAK = hx("5F95AA"), hx("78A9B7"), hx("9CC3C2")
TOWN_WALL, TOWN_ROOF, TOWER = hx("F1B08D"), hx("CF6357"), hx("E7A68A")
DECK, DECK_TOP, DECK_EDGE, DECK_SHADE = hx("47374C"), hx("5B475F"), hx("6B5166"), hx("3D2F43")
JACKET, HELMET, HAIR, MOPED, MOPED_LIGHT, TYRE, RIM = (hx("DD5E43"), hx("F7DDB3"), hx("32293E"),
                                                    hx("3F6A66"), hx("6A948E"), hx("2A2331"), hx("8C8595"))
TROUSERS, SHOE, SKIN = hx("2F4953"), hx("F5D5A8"), hx("A8705E")
LEAF, FLOWER = hx("35283E"), hx("E0747A")

def P(*pts):  # icon coordinates 0..1 -> pixels
    return [(x * S, y * S) for x, y in pts]

img = Image.new("RGB", (S, S))
d = ImageDraw.Draw(img)

# Sky: vertical gradient.
for y in range(S):
    d.line([(0, y), (S, y)], fill=mix(SKY_TOP, SKY_LOW, min(1, y / (S * .56))))

# Sun with a soft glow.
glow = Image.new("L", (S, S), 0)
ImageDraw.Draw(glow).ellipse([S * .50, S * .10, S * .92, S * .52], fill=255)
glow = glow.filter(ImageFilter.GaussianBlur(S * .05))
img.paste(Image.new("RGB", (S, S), SUN_GLOW), (0, 0), glow.point(lambda v: int(v * .7)))
d.ellipse([S * .58, S * .18, S * .84, S * .44], fill=SUN)

def ridge(base, amp, freq, phase, colour, bottom=1.0):
    pts = [(0, bottom)]
    for i in range(0, 201):
        x = i / 200
        y = base - amp * (.6 * math.sin(x * freq + phase) + .4 * math.sin(x * freq * 2.3 + phase * 1.7))
        pts.append((x, y))
    pts.append((1, bottom))
    d.polygon(P(*pts), fill=colour)

ridge(.47, .045, 5.0, .6, RIDGE_FAR)
ridge(.52, .035, 7.0, 2.1, RIDGE_MID)
ridge(.565, .03, 4.0, 4.0, HILL)

# Hill town with a bell tower on the right slope.
for (x0, x1, top) in [(.60, .66, .525), (.655, .71, .505), (.705, .76, .515), (.755, .80, .53)]:
    d.rectangle(P((x0, top), (x1, .58)), fill=TOWN_WALL)
    d.polygon(P((x0 - .006, top), ((x0 + x1) / 2, top - .025), (x1 + .006, top)), fill=TOWN_ROOF)
d.rectangle(P((.675, .40), (.705, .52)), fill=TOWER)
d.polygon(P((.670, .40), (.690, .355), (.710, .40)), fill=TOWN_ROOF)
d.rectangle(P((.684, .415), (.696, .445)), fill=DECK_EDGE)

# Sea with light streaks.
d.rectangle(P((0, .575), (1, 1)), fill=SEA)
d.rectangle(P((0, .575), (1, .59)), fill=SEA_HORIZON)
for (x, y, w) in [(.08, .62, .16), (.36, .64, .10), (.70, .615, .14), (.52, .60, .08)]:
    d.rounded_rectangle(P((x, y), (x + w, y + .008)), radius=S * .004, fill=SEA_STREAK)

# Arched stone bridge (the riding line) across the foreground.
deck_y = .745
d.rectangle(P((0, deck_y), (1, 1)), fill=DECK)
for cx in (.16, .50, .84):
    d.ellipse(P((cx - .12, .80), (cx + .12, 1.04)), fill=DECK_SHADE)
    d.rectangle(P((cx - .12, .92), (cx + .12, 1)), fill=DECK_SHADE)
    d.pieslice(P((cx - .105, .815), (cx + .105, 1.025)), 180, 360, fill=SEA)
    d.rectangle(P((cx - .105, .92), (cx + .105, 1)), fill=SEA)
d.rectangle(P((0, deck_y), (1, deck_y + .022)), fill=DECK_TOP)
d.rectangle(P((0, deck_y + .022), (1, deck_y + .03)), fill=DECK_EDGE)

# Foreground foliage with coral flowers, bottom left.
for (x, y, r) in [(.02, .96, .09), (.12, 1.0, .08), (-.02, .88, .07)]:
    d.ellipse(P((x - r, y - r), (x + r, y + r)), fill=LEAF)
for (x, y) in [(.05, .90), (.11, .94), (.02, .83), (.16, .97)]:
    d.ellipse(P((x - .016, y - .016), (x + .016, y + .016)), fill=FLOWER)

# Moped and rider, popping a wheelie on the bridge. Rotated about the rear contact point.
cx, cy, ang = .42, deck_y, math.radians(-13)
def R(*pts):
    out = []
    for x, y in pts:
        dx, dy = x - cx, y - cy
        out.append((cx + dx * math.cos(ang) - dy * math.sin(ang), cy + dx * math.sin(ang) + dy * math.cos(ang)))
    return P(*out)
def wheel(x, y, r):
    (px, py), = R((x, y))
    d.ellipse([px - r * S, py - r * S, px + r * S, py + r * S], fill=TYRE)
    d.ellipse([px - r * .55 * S, py - r * .55 * S, px + r * .55 * S, py + r * .55 * S], fill=RIM)
    d.ellipse([px - r * .2 * S, py - r * .2 * S, px + r * .2 * S, py + r * .2 * S], fill=TYRE)

wr = .058
wheel(cx, cy - wr, wr)                    # rear
wheel(cx + .30, cy - wr, wr)              # front
# Body: rear cowl, floorboard, leg shield, steering column.
d.polygon(R((cx - .035, cy - .085), (cx + .02, cy - .15), (cx + .12, cy - .155), (cx + .13, cy - .095),
            (cx + .21, cy - .085), (cx + .225, cy - .11), (cx + .245, cy - .24), (cx + .275, cy - .245),
            (cx + .265, cy - .10), (cx + .30, cy - .06), (cx + .28, cy - .045), (cx + .05, cy - .05)), fill=MOPED)
d.polygon(R((cx + .01, cy - .135), (cx + .115, cy - .14), (cx + .12, cy - .12), (cx + .015, cy - .115)), fill=MOPED_LIGHT)
d.polygon(R((cx + .235, cy - .245), (cx + .30, cy - .262), (cx + .305, cy - .248), (cx + .24, cy - .232)), fill=TYRE)  # bars
d.ellipse([p - .012 * S for p in R((cx + .288, cy - .215))[0]] + [p + .012 * S for p in R((cx + .288, cy - .215))[0]], fill=SUN)
# Seat.
d.polygon(R((cx + .01, cy - .155), (cx + .115, cy - .165), (cx + .12, cy - .148), (cx + .015, cy - .142)), fill=TYRE)
# Rider: legs, torso, arm, head.
d.polygon(R((cx + .05, cy - .165), (cx + .11, cy - .17), (cx + .185, cy - .12), (cx + .20, cy - .09),
            (cx + .175, cy - .082), (cx + .15, cy - .11), (cx + .09, cy - .14), (cx + .055, cy - .135)), fill=TROUSERS)
d.polygon(R((cx + .175, cy - .092), (cx + .225, cy - .09), (cx + .225, cy - .075), (cx + .17, cy - .078)), fill=SHOE)
d.polygon(R((cx + .045, cy - .16), (cx + .105, cy - .17), (cx + .165, cy - .28), (cx + .105, cy - .305)), fill=JACKET)
d.polygon(R((cx + .12, cy - .275), (cx + .14, cy - .26), (cx + .245, cy - .24), (cx + .24, cy - .225),
            (cx + .13, cy - .245)), fill=JACKET)
d.polygon(R((cx + .085, cy - .31), (cx + .03, cy - .25), (cx + .01, cy - .20), (cx + .04, cy - .21),
            (cx + .10, cy - .27)), fill=HAIR)  # long hair streaming back
(hx_, hy_), = R((cx + .135, cy - .335))
hr = .045 * S
d.ellipse([hx_ - hr * .8, hy_ - hr * .5, hx_ + hr * 1.05, hy_ + hr * 1.1], fill=SKIN)   # face
d.chord([hx_ - hr, hy_ - hr, hx_ + hr, hy_ + hr], 150, 360, fill=HELMET)               # open-face helmet
d.polygon([(hx_ + hr * .55, hy_ - hr * .15), (hx_ + hr * 1.35, hy_ - hr * .05), (hx_ + hr * .9, hy_ + hr * .12)], fill=HELMET)  # brim

# Sizes: every file referenced by the asset catalog.
sizes = {"Icon-20.png": 20, "Icon-29.png": 29, "Icon-40.png": 40, "Icon-58.png": 58, "Icon-60.png": 60,
         "Icon-76.png": 76, "Icon-80.png": 80, "Icon-87.png": 87, "Icon-120.png": 120, "Icon-152.png": 152,
         "Icon-167.png": 167, "Icon-180.png": 180, "Icon-1024.png": 1024}
os.makedirs(OUT, exist_ok=True)
for name, px in sizes.items():
    img.resize((px, px), Image.LANCZOS).save(os.path.join(OUT, name), optimize=True)

entries = [("iphone", "2x", "20x20", 40), ("iphone", "3x", "20x20", 60), ("iphone", "2x", "29x29", 58),
           ("iphone", "3x", "29x29", 87), ("iphone", "2x", "40x40", 80), ("iphone", "3x", "40x40", 120),
           ("iphone", "2x", "60x60", 120), ("iphone", "3x", "60x60", 180), ("ipad", "1x", "20x20", 20),
           ("ipad", "2x", "20x20", 40), ("ipad", "1x", "29x29", 29), ("ipad", "2x", "29x29", 58),
           ("ipad", "1x", "40x40", 40), ("ipad", "2x", "40x40", 80), ("ipad", "1x", "76x76", 76),
           ("ipad", "2x", "76x76", 152), ("ipad", "2x", "83.5x83.5", 167), ("ios-marketing", "1x", "1024x1024", 1024)]
with open(os.path.join(OUT, "Contents.json"), "w") as f:
    json.dump({"images": [{"filename": "Icon-%d.png" % px, "idiom": i, "scale": sc, "size": sz}
                          for i, sc, sz, px in entries], "info": {"author": "xcode", "version": 1}}, f, indent=2)
print("Wrote", OUT)
