"""
NekoCollect SE・BGM生成
チップチューン風サウンドをnumpyで合成してWAV出力
"""
import numpy as np
import struct, wave, os

SE_DIR = "D:/workd/NekoCollect/NekoCollect/Assets/Audio/SE"
BGM_DIR = "D:/workd/NekoCollect/NekoCollect/Assets/Audio/BGM"
RATE = 44100

def save_wav(path, samples, rate=RATE):
    """float配列をWAVファイルに保存"""
    samples = np.clip(samples, -1.0, 1.0)
    data = (samples * 32767).astype(np.int16)
    with wave.open(path, 'w') as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(rate)
        w.writeframes(data.tobytes())

def sine(freq, duration, volume=0.5):
    t = np.linspace(0, duration, int(RATE * duration), False)
    return np.sin(2 * np.pi * freq * t) * volume

def square(freq, duration, volume=0.3):
    t = np.linspace(0, duration, int(RATE * duration), False)
    return np.sign(np.sin(2 * np.pi * freq * t)) * volume

def triangle(freq, duration, volume=0.4):
    t = np.linspace(0, duration, int(RATE * duration), False)
    return (2 * np.abs(2 * (t * freq - np.floor(t * freq + 0.5))) - 1) * volume

def noise(duration, volume=0.2):
    return np.random.uniform(-volume, volume, int(RATE * duration))

def envelope(samples, attack=0.01, decay=0.05, sustain=0.7, release=0.1):
    """ADSR エンベロープ"""
    n = len(samples)
    env = np.ones(n)
    a = int(attack * RATE)
    d = int(decay * RATE)
    r = int(release * RATE)
    # Attack
    if a > 0:
        env[:a] = np.linspace(0, 1, a)
    # Decay
    if d > 0 and a+d < n:
        env[a:a+d] = np.linspace(1, sustain, d)
    # Sustain
    if a+d < n-r:
        env[a+d:n-r] = sustain
    # Release
    if r > 0:
        env[-r:] = np.linspace(sustain, 0, r)
    return samples * env

def fade_out(samples, duration=0.05):
    n = int(RATE * duration)
    if n > len(samples):
        n = len(samples)
    samples[-n:] *= np.linspace(1, 0, n)
    return samples

print("=== SE生成 ===")

# 1. コインクリック音
s = envelope(sine(800, 0.08, 0.4) + sine(1200, 0.08, 0.2), 0.005, 0.02, 0.3, 0.03)
save_wav(os.path.join(SE_DIR, "se_click.wav"), s)
print("  se_click.wav")

# 2. コイン獲得音（チャリン）
s1 = envelope(sine(1200, 0.1, 0.3), 0.005, 0.03, 0.2, 0.04)
s2 = envelope(sine(1600, 0.1, 0.2), 0.005, 0.03, 0.15, 0.04)
pad = np.zeros(int(RATE * 0.04))
s = np.concatenate([s1, pad, s2])
save_wav(os.path.join(SE_DIR, "se_coin.wav"), fade_out(s))
print("  se_coin.wav")

# 3. ガチャ回転音
dur = 1.0
t = np.linspace(0, dur, int(RATE * dur), False)
freq = 200 + 400 * t/dur  # 周波数が上がっていく
s = np.sin(2 * np.pi * freq * t) * 0.3
s += noise(dur, 0.05)
s = envelope(s, 0.05, 0.1, 0.8, 0.3)
save_wav(os.path.join(SE_DIR, "se_gacha_roll.wav"), s)
print("  se_gacha_roll.wav")

# 4. ガチャ結果表示音（ジャジャーン）
notes = [523, 659, 784, 1047]  # C5, E5, G5, C6
parts = []
for i, note in enumerate(notes):
    delay = np.zeros(int(RATE * i * 0.08))
    tone = envelope(sine(note, 0.4, 0.35) + triangle(note, 0.4, 0.15),
                    0.01, 0.05, 0.6, 0.2)
    parts.append(np.concatenate([delay, tone]))
max_len = max(len(p) for p in parts)
s = sum(np.pad(p, (0, max_len - len(p))) for p in parts)
s = np.clip(s, -1, 1)
save_wav(os.path.join(SE_DIR, "se_gacha_result.wav"), s)
print("  se_gacha_result.wav")

# 5. レアガチャ結果音（もっと派手）
notes = [523, 659, 784, 1047, 1319, 1568]  # C5-G6
parts = []
for i, note in enumerate(notes):
    delay = np.zeros(int(RATE * i * 0.06))
    tone = envelope(sine(note, 0.6, 0.3) + square(note*2, 0.6, 0.1),
                    0.01, 0.1, 0.5, 0.3)
    parts.append(np.concatenate([delay, tone]))
max_len = max(len(p) for p in parts)
s = sum(np.pad(p, (0, max_len - len(p))) for p in parts)
# キラキラ追加
sparkle_t = np.linspace(0, 0.8, int(RATE * 0.8), False)
sparkle = np.sin(2*np.pi*3000*sparkle_t) * np.sin(2*np.pi*5*sparkle_t) * 0.1
s[:len(sparkle)] += sparkle
s = np.clip(s, -1, 1)
save_wav(os.path.join(SE_DIR, "se_gacha_rare.wav"), s)
print("  se_gacha_rare.wav")

# 6. 餌やり音（もぐもぐ）
parts = []
for i in range(3):
    delay = np.zeros(int(RATE * i * 0.12))
    munch = envelope(sine(300 + i*50, 0.08, 0.3) + noise(0.08, 0.1),
                     0.01, 0.02, 0.3, 0.03)
    parts.append(np.concatenate([delay, munch]))
max_len = max(len(p) for p in parts)
s = sum(np.pad(p, (0, max_len - len(p))) for p in parts)
save_wav(os.path.join(SE_DIR, "se_feed.wav"), s)
print("  se_feed.wav")

# 7. レベルアップ音
notes = [523, 659, 784, 1047]  # 上昇音階
parts = []
for i, note in enumerate(notes):
    delay = np.zeros(int(RATE * i * 0.1))
    tone = envelope(triangle(note, 0.15, 0.4), 0.01, 0.03, 0.5, 0.05)
    parts.append(np.concatenate([delay, tone]))
max_len = max(len(p) for p in parts)
s = sum(np.pad(p, (0, max_len - len(p))) for p in parts)
# 最後にキラキラ
sparkle = envelope(sine(2000, 0.3, 0.15) + sine(2500, 0.3, 0.1), 0.01, 0.1, 0.3, 0.15)
s = np.concatenate([s, sparkle])
save_wav(os.path.join(SE_DIR, "se_levelup.wav"), s)
print("  se_levelup.wav")

# 8. 進化音（ドラマチック）
# 下降→上昇の劇的な音
t1 = np.linspace(0, 0.5, int(RATE*0.5), False)
freq1 = 600 - 300 * t1
sweep_down = np.sin(2*np.pi * np.cumsum(freq1)/RATE) * 0.3
sweep_down = envelope(sweep_down, 0.02, 0.1, 0.5, 0.1)

t2 = np.linspace(0, 0.8, int(RATE*0.8), False)
freq2 = 400 + 800 * t2
sweep_up = np.sin(2*np.pi * np.cumsum(freq2)/RATE) * 0.35
sweep_up = envelope(sweep_up, 0.05, 0.1, 0.6, 0.3)

# 和音フィナーレ
finale_notes = [523, 659, 784, 1047]
finale_parts = []
for note in finale_notes:
    finale_parts.append(envelope(sine(note, 0.8, 0.2), 0.02, 0.1, 0.5, 0.4))
finale = sum(finale_parts)

pause = np.zeros(int(RATE * 0.1))
s = np.concatenate([sweep_down, pause, sweep_up, pause, finale])
s = np.clip(s, -1, 1)
save_wav(os.path.join(SE_DIR, "se_evolve.wav"), s)
print("  se_evolve.wav")

# 9. ボタンタップ音（軽い）
s = envelope(sine(600, 0.05, 0.25) + sine(900, 0.05, 0.1), 0.003, 0.01, 0.2, 0.02)
save_wav(os.path.join(SE_DIR, "se_tap.wav"), s)
print("  se_tap.wav")

# 10. おるすばんボーナス音
notes = [392, 440, 494, 523, 587, 659, 784]  # G4-G5 上昇
parts = []
for i, note in enumerate(notes):
    delay = np.zeros(int(RATE * i * 0.08))
    tone = envelope(triangle(note, 0.2, 0.3), 0.01, 0.03, 0.4, 0.1)
    parts.append(np.concatenate([delay, tone]))
max_len = max(len(p) for p in parts)
s = sum(np.pad(p, (0, max_len - len(p))) for p in parts)
s = np.clip(s, -1, 1)
save_wav(os.path.join(SE_DIR, "se_bonus.wav"), s)
print("  se_bonus.wav")


print("\n=== BGM生成 ===")

# メインBGM - ゆるかわチップチューン（約30秒ループ）
BPM = 120
beat = 60.0 / BPM  # 0.5秒/ビート
bar = beat * 4       # 2秒/小節

# メロディ（16小節 = 32秒）
melody_notes = [
    # 1-4小節: メインフレーズ
    (392, 1), (440, 1), (523, 1), (494, 0.5), (440, 0.5),
    (392, 1), (330, 1), (349, 2),
    (392, 1), (440, 1), (523, 1), (587, 0.5), (523, 0.5),
    (494, 1), (440, 1), (392, 2),
    # 5-8小節: サビ
    (523, 1), (587, 1), (659, 1), (587, 0.5), (523, 0.5),
    (494, 1), (440, 1), (523, 2),
    (659, 1), (587, 1), (523, 1), (494, 0.5), (440, 0.5),
    (392, 1), (440, 1), (392, 2),
    # 9-12小節: Bメロ
    (330, 1), (349, 1), (392, 1), (440, 1),
    (494, 1), (523, 0.5), (494, 0.5), (440, 2),
    (349, 1), (392, 1), (440, 1), (494, 1),
    (523, 1), (587, 0.5), (523, 0.5), (494, 2),
    # 13-16小節: 締め
    (523, 1), (587, 1), (659, 1), (587, 0.5), (523, 0.5),
    (494, 1), (523, 1), (587, 2),
    (659, 1), (587, 1), (523, 1), (440, 0.5), (392, 0.5),
    (440, 1), (494, 0.5), (440, 0.5), (392, 2),
]

# ベースライン（コード進行: C-Am-F-G のバリエーション）
bass_pattern = [
    # 各小節のルート音（ビート単位）
    (131, 4), (110, 4), (87, 4), (98, 4),  # C-Am-F-G
    (131, 4), (110, 4), (87, 4), (98, 4),
    (131, 4), (87, 4), (110, 4), (98, 4),
    (131, 4), (110, 4), (87, 2), (98, 2),
]

# メロディトラック生成
melody_samples = np.array([], dtype=np.float64)
for freq, beats in melody_notes:
    dur = beat * beats
    tone = triangle(freq, dur, 0.25) + sine(freq*2, dur, 0.05)
    tone = envelope(tone, 0.01, 0.05, 0.6, min(0.1, dur*0.3))
    melody_samples = np.concatenate([melody_samples, tone])

# ベーストラック生成
bass_samples = np.array([], dtype=np.float64)
for freq, beats in bass_pattern:
    dur = beat * beats
    tone = square(freq, dur, 0.15)
    tone = envelope(tone, 0.01, 0.05, 0.7, 0.05)
    bass_samples = np.concatenate([bass_samples, tone])

# ベースをメロディの長さに合わせてループ
if len(bass_samples) < len(melody_samples):
    repeats = (len(melody_samples) // len(bass_samples)) + 1
    bass_samples = np.tile(bass_samples, repeats)[:len(melody_samples)]
else:
    bass_samples = bass_samples[:len(melody_samples)]

# ドラムトラック（キック+ハイハット）
total_dur = len(melody_samples) / RATE
total_beats = int(total_dur / beat)
drum_samples = np.zeros(len(melody_samples))

for b in range(total_beats):
    pos = int(b * beat * RATE)
    # キック（1,3拍目）
    if b % 4 == 0 or b % 4 == 2:
        kick_dur = int(0.1 * RATE)
        kick_t = np.linspace(0, 0.1, kick_dur, False)
        kick_freq = 150 * np.exp(-30 * kick_t)
        kick = np.sin(2*np.pi * np.cumsum(kick_freq)/RATE) * 0.2
        kick *= np.exp(-20 * kick_t)
        end = min(pos + kick_dur, len(drum_samples))
        drum_samples[pos:end] += kick[:end-pos]
    # ハイハット（毎拍）
    hh_dur = int(0.03 * RATE)
    hh = noise(0.03, 0.08) * np.exp(-np.linspace(0, 5, hh_dur))
    end = min(pos + hh_dur, len(drum_samples))
    drum_samples[pos:end] += hh[:end-pos]
    # オフビートハイハット（裏拍）
    off_pos = pos + int(beat * 0.5 * RATE)
    if off_pos + hh_dur < len(drum_samples):
        hh2 = noise(0.03, 0.05) * np.exp(-np.linspace(0, 8, hh_dur))
        end2 = min(off_pos + hh_dur, len(drum_samples))
        drum_samples[off_pos:end2] += hh2[:end2-off_pos]

# ミックス
bgm = melody_samples * 0.5 + bass_samples * 0.6 + drum_samples * 0.8
bgm = np.clip(bgm, -1, 1)

# フェードイン/アウト
fade_in_n = int(RATE * 1.0)
fade_out_n = int(RATE * 2.0)
bgm[:fade_in_n] *= np.linspace(0, 1, fade_in_n)
bgm[-fade_out_n:] *= np.linspace(1, 0, fade_out_n)

save_wav(os.path.join(BGM_DIR, "bgm_main.wav"), bgm)
print(f"  bgm_main.wav ({len(bgm)/RATE:.1f}秒)")

print("\n=== 全オーディオ生成完了 ===")
