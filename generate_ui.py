"""
NekoCollect UI素材生成 - 背景、ボタン、アイコン
"""
from PIL import Image, ImageDraw, ImageFilter
import os, math, random

BG_DIR = "D:/workd/NekoCollect/NekoCollect/Assets/Sprites/Backgrounds"
UI_DIR = "D:/workd/NekoCollect/NekoCollect/Assets/Sprites/UI"

def gradient_bg(w, h, top_color, bot_color):
    """縦グラデーション背景"""
    img = Image.new("RGBA", (w, h))
    for y in range(h):
        t = y / h
        r = int(top_color[0]*(1-t) + bot_color[0]*t)
        g = int(top_color[1]*(1-t) + bot_color[1]*t)
        b = int(top_color[2]*(1-t) + bot_color[2]*t)
        for x in range(w):
            img.putpixel((x, y), (r, g, b, 255))
    return img

def add_stars(img, count=30):
    """星模様を追加"""
    random.seed(42)
    for _ in range(count):
        x = random.randint(0, img.width-1)
        y = random.randint(0, img.height//2)
        brightness = random.randint(150, 255)
        size = random.choice([1, 1, 1, 2])
        for dy in range(-size+1, size):
            for dx in range(-size+1, size):
                if 0 <= x+dx < img.width and 0 <= y+dy < img.height:
                    a = max(0, brightness - abs(dx)*40 - abs(dy)*40)
                    img.putpixel((x+dx, y+dy), (brightness, brightness, brightness+20, a))

def add_paw_prints(img, count=8):
    """肉球模様を散りばめる"""
    random.seed(123)
    draw = ImageDraw.Draw(img)
    for _ in range(count):
        cx = random.randint(20, img.width-20)
        cy = random.randint(20, img.height-20)
        alpha = random.randint(15, 35)
        color = (255, 255, 255, alpha)
        s = random.randint(4, 8)
        # メインパッド
        draw.ellipse([cx-s, cy-s//2, cx+s, cy+s], fill=color)
        # 指パッド
        for angle in [-0.6, -0.2, 0.2, 0.6]:
            px = cx + int(math.cos(angle-1.57)*s*1.5)
            py = cy + int(math.sin(angle-1.57)*s*1.5)
            ps = s//2
            draw.ellipse([px-ps, py-ps, px+ps, py+ps], fill=color)

def rounded_rect(draw, xy, radius, fill):
    """角丸矩形"""
    x1, y1, x2, y2 = xy
    draw.rectangle([x1+radius, y1, x2-radius, y2], fill=fill)
    draw.rectangle([x1, y1+radius, x2, y2-radius], fill=fill)
    draw.pieslice([x1, y1, x1+2*radius, y1+2*radius], 180, 270, fill=fill)
    draw.pieslice([x2-2*radius, y1, x2, y1+2*radius], 270, 360, fill=fill)
    draw.pieslice([x1, y2-2*radius, x1+2*radius, y2], 90, 180, fill=fill)
    draw.pieslice([x2-2*radius, y2-2*radius, x2, y2], 0, 90, fill=fill)

print("=== 背景画像生成 ===")

# ホーム画面背景（暖かいオレンジ〜パープル）
bg = gradient_bg(540, 960, (60, 30, 80), (30, 15, 50))
add_stars(bg, 40)
add_paw_prints(bg, 12)
bg.save(os.path.join(BG_DIR, "bg_home.png"))
print("  bg_home.png")

# ガチャ画面背景（華やかなゴールド〜パープル）
bg = gradient_bg(540, 960, (80, 40, 100), (40, 20, 60))
add_stars(bg, 60)
# キラキラエフェクト
random.seed(77)
for _ in range(20):
    x, y = random.randint(0, 539), random.randint(0, 959)
    for dd in range(3):
        for d in [(-1,0),(1,0),(0,-1),(0,1)]:
            px, py = x+d[0]*(dd+1), y+d[1]*(dd+1)
            if 0<=px<540 and 0<=py<960:
                a = max(0, 200-dd*60)
                bg.putpixel((px,py), (255, 230, 100, a))
bg.save(os.path.join(BG_DIR, "bg_gacha.png"))
print("  bg_gacha.png")

# 猫一覧背景（落ち着いたブルー）
bg = gradient_bg(540, 960, (30, 50, 80), (20, 30, 50))
add_paw_prints(bg, 15)
bg.save(os.path.join(BG_DIR, "bg_catlist.png"))
print("  bg_catlist.png")

# 猫詳細背景（ダークティール）
bg = gradient_bg(540, 960, (20, 50, 60), (15, 25, 35))
add_paw_prints(bg, 8)
bg.save(os.path.join(BG_DIR, "bg_detail.png"))
print("  bg_detail.png")

# 図鑑背景（ダークネイビー）
bg = gradient_bg(540, 960, (25, 25, 55), (15, 15, 35))
add_stars(bg, 50)
add_paw_prints(bg, 10)
bg.save(os.path.join(BG_DIR, "bg_catalog.png"))
print("  bg_catalog.png")


print("\n=== UIパーツ生成 ===")

# ボタン素材（4種類）
button_configs = [
    ("btn_primary", (80, 160, 255), (60, 130, 220), 256, 80),   # 青（メイン）
    ("btn_secondary", (100, 200, 120), (70, 170, 90), 256, 80), # 緑（餌）
    ("btn_gacha", (255, 180, 50), (230, 150, 30), 256, 80),     # 金（ガチャ）
    ("btn_danger", (255, 100, 100), (220, 70, 70), 256, 80),    # 赤（進化）
    ("btn_close", (120, 120, 140), (90, 90, 110), 180, 60),     # グレー（閉じる）
]

for name, top, bot, w, h in button_configs:
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    # ボタン影
    rounded_rect(draw, (2, 4, w-2, h-1), 12, (0, 0, 0, 80))
    # ボタン本体グラデ
    for y in range(4, h-4):
        t = (y-4) / (h-8)
        r = int(top[0]*(1-t) + bot[0]*t)
        g = int(top[1]*(1-t) + bot[1]*t)
        b = int(top[2]*(1-t) + bot[2]*t)
        for x in range(4, w-4):
            # 角丸チェック
            radius = 10
            in_corner = False
            for cx, cy in [(4+radius, 4+radius), (w-4-radius, 4+radius),
                           (4+radius, h-4-radius), (w-4-radius, h-4-radius)]:
                dx, dy = x-cx, y-cy
                if ((x < 4+radius or x > w-4-radius) and
                    (y < 4+radius or y > h-4-radius)):
                    if dx*dx + dy*dy > radius*radius:
                        in_corner = True
            if not in_corner:
                img.putpixel((x, y), (r, g, b, 255))
    # ハイライト（上部）
    for y in range(6, 6+h//4):
        for x in range(8, w-8):
            px = img.getpixel((x, y))
            if px[3] > 0:
                highlight = min(255, px[0]+30), min(255, px[1]+30), min(255, px[2]+30)
                img.putpixel((x, y), (*highlight, 255))
    img.save(os.path.join(UI_DIR, f"{name}.png"))
    print(f"  {name}.png")


# コインアイコン（32x32ピクセルアート）
coin = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
d = ImageDraw.Draw(coin)
# 外枠
d.ellipse([2, 2, 29, 29], fill=(255, 200, 50))
d.ellipse([4, 4, 27, 27], fill=(255, 220, 80))
# ¢マーク
d.ellipse([10, 8, 22, 24], outline=(200, 150, 30), width=2)
d.line([16, 6, 16, 26], fill=(200, 150, 30), width=2)
coin.save(os.path.join(UI_DIR, "icon_coin.png"))
print("  icon_coin.png")


# カード背景（猫一覧用）
card = Image.new("RGBA", (200, 260), (0, 0, 0, 0))
d = ImageDraw.Draw(card)
rounded_rect(d, (0, 0, 199, 259), 16, (40, 45, 60, 230))
# 枠線
rounded_rect(d, (2, 2, 197, 257), 14, (60, 70, 90, 200))
rounded_rect(d, (4, 4, 195, 255), 12, (35, 40, 55, 240))
card.save(os.path.join(UI_DIR, "card_bg.png"))
print("  card_bg.png")

# レアリティ別カード枠
rarity_colors = [
    ("card_frame_common", (150, 150, 160)),
    ("card_frame_rare", (80, 180, 255)),
    ("card_frame_sr", (255, 200, 50)),
    ("card_frame_ur", (255, 100, 255)),
]
for name, color in rarity_colors:
    frame = Image.new("RGBA", (200, 260), (0, 0, 0, 0))
    d = ImageDraw.Draw(frame)
    rounded_rect(d, (0, 0, 199, 259), 16, (*color, 200))
    rounded_rect(d, (3, 3, 196, 256), 13, (0, 0, 0, 0))
    frame.save(os.path.join(UI_DIR, f"{name}.png"))
    print(f"  {name}.png")


# 経験値バー（背景と前景）
exp_bg = Image.new("RGBA", (400, 24), (0, 0, 0, 0))
d = ImageDraw.Draw(exp_bg)
rounded_rect(d, (0, 0, 399, 23), 8, (30, 30, 40, 200))
exp_bg.save(os.path.join(UI_DIR, "exp_bar_bg.png"))
print("  exp_bar_bg.png")

exp_fill = Image.new("RGBA", (400, 24), (0, 0, 0, 0))
d = ImageDraw.Draw(exp_fill)
rounded_rect(d, (0, 0, 399, 23), 8, (100, 220, 100, 255))
# グラデーションハイライト
for x in range(400):
    for y in range(8):
        px = exp_fill.getpixel((x, y+2))
        if px[3] > 0:
            exp_fill.putpixel((x, y+2), (min(255,px[0]+40), min(255,px[1]+40), px[2], 255))
exp_fill.save(os.path.join(UI_DIR, "exp_bar_fill.png"))
print("  exp_bar_fill.png")


# ガチャカプセル（演出用）
capsule = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
d = ImageDraw.Draw(capsule)
# 下半分
d.pieslice([16, 32, 112, 120], 0, 180, fill=(240, 240, 250))
# 上半分
d.pieslice([16, 8, 112, 96], 180, 360, fill=(255, 80, 80))
# 分割線
d.rectangle([16, 60, 112, 68], fill=(220, 220, 230))
# 中央ボタン
d.ellipse([50, 52, 78, 76], fill=(240, 240, 250))
d.ellipse([54, 56, 74, 72], fill=(255, 255, 255))
# ハイライト
d.ellipse([30, 24, 50, 44], fill=(255, 255, 255, 80))
capsule.save(os.path.join(UI_DIR, "gacha_capsule.png"))
print("  gacha_capsule.png")

print("\n=== UI素材生成完了 ===")
