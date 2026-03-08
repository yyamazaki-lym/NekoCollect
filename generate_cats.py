"""
NekoCollect 猫スプライト生成 - 高品質ピクセルアート (128x128)
7種の猫をそれぞれ特徴的なデザインで生成
"""
from PIL import Image, ImageDraw
import os

OUT = "D:/workd/NekoCollect/NekoCollect/Assets/Sprites/Cats"

def put(img, x, y, color):
    """ピクセルを安全に配置"""
    if 0 <= x < img.width and 0 <= y < img.height:
        img.putpixel((x, y), color)

def fill_rect(img, x1, y1, x2, y2, color):
    """矩形を塗りつぶし"""
    for yy in range(y1, y2+1):
        for xx in range(x1, x2+1):
            put(img, xx, yy, color)

def draw_ellipse_filled(img, cx, cy, rx, ry, color):
    """塗りつぶし楕円"""
    for yy in range(-ry, ry+1):
        for xx in range(-rx, rx+1):
            if (xx*xx)/(rx*rx+0.01) + (yy*yy)/(ry*ry+0.01) <= 1.0:
                put(img, cx+xx, cy+yy, color)

def draw_cat(name, body_color, belly_color, eye_color, pattern_func=None,
             ear_inner=(255,150,150), nose_color=(255,130,130),
             has_stripes=False, stripe_color=None, rarity_glow=None):
    """汎用猫描画関数"""
    img = Image.new("RGBA", (128, 128), (0, 0, 0, 0))

    # レアリティの光彩エフェクト
    if rarity_glow:
        draw_ellipse_filled(img, 64, 68, 40, 38, (*rarity_glow, 30))
        draw_ellipse_filled(img, 64, 68, 36, 34, (*rarity_glow, 20))

    # しっぽ（右後ろ）
    tail = body_color
    for i in range(12):
        tx = 90 + i
        ty = 90 - i//2 + (i*i)//20
        fill_rect(img, tx, ty, tx+3, ty+3, tail)

    # 体（楕円）
    draw_ellipse_filled(img, 64, 80, 28, 22, body_color)
    # お腹
    draw_ellipse_filled(img, 64, 84, 16, 14, belly_color)

    # 前足
    fill_rect(img, 42, 90, 48, 105, body_color)
    fill_rect(img, 78, 90, 84, 105, body_color)
    # 足先（肉球）
    fill_rect(img, 40, 103, 50, 108, body_color)
    fill_rect(img, 76, 103, 86, 108, body_color)
    # 肉球
    draw_ellipse_filled(img, 45, 106, 3, 2, (255, 180, 180))
    draw_ellipse_filled(img, 81, 106, 3, 2, (255, 180, 180))

    # 頭
    draw_ellipse_filled(img, 64, 48, 24, 20, body_color)

    # 耳（三角）
    for i in range(12):
        # 左耳
        fill_rect(img, 44-i//2, 32+i, 44+i//2, 32+i, body_color)
        # 右耳
        fill_rect(img, 84-i//2, 32+i, 84+i//2, 32+i, body_color)
    # 耳の内側
    for i in range(8):
        fill_rect(img, 44-i//3, 35+i, 44+i//3, 35+i, ear_inner)
        fill_rect(img, 84-i//3, 35+i, 84+i//3, 35+i, ear_inner)

    # 縞模様
    if has_stripes and stripe_color:
        # 額の縞
        fill_rect(img, 62, 36, 66, 40, stripe_color)
        fill_rect(img, 56, 38, 59, 40, stripe_color)
        fill_rect(img, 69, 38, 72, 40, stripe_color)
        # 体の縞
        for i in range(3):
            y = 72 + i * 8
            fill_rect(img, 48, y, 52, y+2, stripe_color)
            fill_rect(img, 76, y, 80, y+2, stripe_color)

    # カスタムパターン
    if pattern_func:
        pattern_func(img)

    # 目（大きめで可愛く）
    # 白目
    draw_ellipse_filled(img, 54, 48, 7, 6, (255, 255, 255))
    draw_ellipse_filled(img, 74, 48, 7, 6, (255, 255, 255))
    # 瞳
    draw_ellipse_filled(img, 55, 48, 5, 5, eye_color)
    draw_ellipse_filled(img, 75, 48, 5, 5, eye_color)
    # ハイライト
    fill_rect(img, 53, 46, 54, 47, (255, 255, 255))
    fill_rect(img, 73, 46, 74, 47, (255, 255, 255))
    # 瞳孔
    draw_ellipse_filled(img, 56, 49, 2, 3, (20, 20, 20))
    draw_ellipse_filled(img, 76, 49, 2, 3, (20, 20, 20))

    # 鼻
    fill_rect(img, 62, 54, 66, 56, nose_color)
    # 口
    put(img, 63, 57, (60, 60, 60))
    put(img, 65, 57, (60, 60, 60))
    put(img, 62, 58, (60, 60, 60))
    put(img, 66, 58, (60, 60, 60))

    # ひげ
    whisker = (180, 180, 180)
    for i in range(8):
        put(img, 38+i, 52, whisker)
        put(img, 38+i, 55, whisker)
        put(img, 88-i, 52, whisker)
        put(img, 88-i, 55, whisker)

    # 最終リサイズ（NN法でドット感を保持）
    path = os.path.join(OUT, f"{name}.png")
    img.save(path)
    print(f"  生成: {path}")
    return img


def pattern_calico(img):
    """三毛猫のパッチ模様"""
    orange = (230, 140, 50)
    black = (40, 40, 40)
    # オレンジパッチ
    draw_ellipse_filled(img, 50, 42, 6, 4, orange)
    draw_ellipse_filled(img, 72, 76, 8, 6, orange)
    draw_ellipse_filled(img, 48, 86, 6, 5, orange)
    # 黒パッチ
    draw_ellipse_filled(img, 78, 42, 5, 4, black)
    draw_ellipse_filled(img, 56, 80, 7, 5, black)


def pattern_scottish(img):
    """スコティッシュフォールドの折れ耳"""
    body = (190, 180, 160)
    # 耳を折り曲げる（元の耳先を体色で上書き）
    for i in range(6):
        fill_rect(img, 42-i//2, 32+i, 46+i//2, 32+i, (0,0,0,0))
        fill_rect(img, 82-i//2, 32+i, 86+i//2, 32+i, (0,0,0,0))
    # 折れた耳を描画
    for i in range(6):
        fill_rect(img, 42, 36+i, 48, 36+i, body)
        fill_rect(img, 80, 36+i, 86, 36+i, body)


def pattern_munchkin(img):
    """マンチカンの短い足"""
    body = (230, 190, 130)
    # 短い足に修正（元の足を消して短く）
    fill_rect(img, 42, 90, 48, 98, body)
    fill_rect(img, 78, 90, 84, 98, body)
    # 短い足先
    fill_rect(img, 40, 98, 50, 103, body)
    fill_rect(img, 76, 98, 86, 103, body)
    # 肉球
    draw_ellipse_filled(img, 45, 101, 3, 2, (255, 180, 180))
    draw_ellipse_filled(img, 81, 101, 3, 2, (255, 180, 180))
    # 王冠（URレア感）
    crown = (255, 215, 0)
    gem = (255, 50, 50)
    fill_rect(img, 54, 24, 74, 30, crown)
    fill_rect(img, 56, 20, 58, 24, crown)
    fill_rect(img, 63, 18, 65, 24, crown)
    fill_rect(img, 70, 20, 72, 24, crown)
    fill_rect(img, 63, 26, 65, 28, gem)


print("=== 猫スプライト生成開始 ===")

# 1. 茶トラ (Common)
draw_cat("cat_chatora",
    body_color=(200, 150, 80), belly_color=(240, 220, 180),
    eye_color=(100, 180, 80), has_stripes=True,
    stripe_color=(160, 100, 40))

# 2. 黒猫 (Common)
draw_cat("cat_kuro",
    body_color=(50, 50, 55), belly_color=(80, 80, 85),
    eye_color=(220, 200, 50), ear_inner=(120, 80, 80),
    nose_color=(60, 60, 65))

# 3. 白猫 (Common)
draw_cat("cat_shiro",
    body_color=(240, 240, 245), belly_color=(255, 255, 255),
    eye_color=(100, 160, 230), ear_inner=(255, 180, 180),
    nose_color=(255, 160, 160))

# 4. 三毛猫 (Rare)
draw_cat("cat_mike",
    body_color=(245, 240, 230), belly_color=(255, 250, 245),
    eye_color=(80, 180, 120), pattern_func=pattern_calico,
    rarity_glow=(100, 200, 255))

# 5. ロシアンブルー (Rare)
draw_cat("cat_russian_blue",
    body_color=(140, 155, 175), belly_color=(180, 195, 210),
    eye_color=(80, 200, 120), ear_inner=(170, 140, 150),
    nose_color=(160, 140, 150), rarity_glow=(100, 200, 255))

# 6. スコティッシュフォールド (SR)
draw_cat("cat_scottish",
    body_color=(190, 180, 160), belly_color=(220, 215, 200),
    eye_color=(200, 160, 60), ear_inner=(210, 180, 170),
    nose_color=(200, 160, 150), pattern_func=pattern_scottish,
    rarity_glow=(255, 200, 50))

# 7. マンチカン (UR)
draw_cat("cat_munchkin",
    body_color=(230, 190, 130), belly_color=(250, 230, 200),
    eye_color=(60, 140, 220), ear_inner=(255, 180, 160),
    nose_color=(255, 150, 140), pattern_func=pattern_munchkin,
    rarity_glow=(255, 100, 255))

print("=== 猫スプライト生成完了 ===")
