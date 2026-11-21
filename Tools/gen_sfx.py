import math, struct, wave, sys, os
SR = 44100
out = sys.argv[1]
os.makedirs(out, exist_ok=True)

def tone(freq_start, freq_end, dur, wave_type="square", vol=0.35, attack=0.005):
    n = int(SR * dur); s = []; phase = 0.0
    for i in range(n):
        t = i / n
        f = freq_start + (freq_end - freq_start) * t
        phase += f / SR
        p = phase % 1.0
        if wave_type == "square": v = 1.0 if p < 0.5 else -1.0
        elif wave_type == "triangle": v = 4 * abs(p - 0.5) - 1
        else: v = math.sin(2 * math.pi * p)
        env = min(1.0, i / (SR * attack)) * (1 - t) ** 1.5   # 빠른 어택, 점점 감쇠
        s.append(v * env * vol)
    return s

def glass_break(dur=0.5, seed=7, peak=0.35):
    """유리 깨지는 소리: 맑은 '챙'(비정수배 고음 배음) + 짧은 타격 노이즈 + 부서지는 잔향."""
    import random
    rnd = random.Random(seed)
    n = int(SR * dur)
    s = [0.0] * n
    # 1) '챙' — 유리 특유의 비정수배 배음, 고음일수록 빨리 감쇠
    partials = [(2350, 1.00, 0.16), (2372, 0.55, 0.18), (3610, 0.65, 0.11),
                (5130, 0.40, 0.08), (6890, 0.28, 0.06), (8420, 0.18, 0.04)]
    for f, a, tau in partials:
        for i in range(n):
            t = i / SR
            s[i] += a * math.sin(2 * math.pi * f * t) * math.exp(-t / tau)
    # 2) 타격 순간의 짧은 고역 노이즈
    prev = 0.0
    for i in range(int(SR * 0.012)):
        w = rnd.uniform(-1, 1)
        s[i] += (w - prev) * 0.9 * math.exp(-i / (SR * 0.003))
        prev = w
    # 3) 부서지는 잔향 — 작은 파편들이 짤랑거리는 소리
    for k in range(18):
        start = 0.03 + (k / 18) ** 1.4 * 0.38 + rnd.uniform(0, 0.02)
        f = rnd.uniform(3200, 9000)
        a = 0.35 * (1 - start / dur) * rnd.uniform(0.4, 1.0)
        tau = rnd.uniform(0.008, 0.025)
        i0 = int(start * SR)
        for i in range(i0, min(n, i0 + int(tau * 6 * SR))):
            t = (i - i0) / SR
            s[i] += a * math.sin(2 * math.pi * f * t) * math.exp(-t / tau)
    # 4) 은은한 고역 노이즈 꼬리
    prev = 0.0
    for i in range(n):
        w = rnd.uniform(-1, 1)
        t = i / SR
        s[i] += (w - prev) * 0.08 * math.exp(-t / 0.09)
        prev = w
    # 끝부분 페이드아웃 후 정규화
    fade = int(SR * 0.05)
    for i in range(fade):
        s[n - 1 - i] *= i / fade
    m = max(abs(x) for x in s)
    return [x / m * peak for x in s]

def metal_tink(dur=0.18, peak=0.3):
    """단단한 블럭 타격: 짧고 둔탁한 금속성 '틱'."""
    n = int(SR * dur)
    parts = [(1180, 1.0, 0.045), (1730, 0.6, 0.035), (2650, 0.4, 0.025), (590, 0.5, 0.05)]
    s = [sum(a * math.sin(2 * math.pi * f * i / SR) * math.exp(-(i / SR) / tau) for f, a, tau in parts)
         for i in range(n)]
    m = max(abs(x) for x in s)
    return [x / m * peak for x in s]

def silence(dur): return [0.0] * int(SR * dur)

def save(name, samples):
    with wave.open(os.path.join(out, name), "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, x)) * 32767)) for x in samples))

save("PaddleHit.wav", tone(520, 520, 0.08, "square", 0.30))
save("WallHit.wav",   tone(260, 240, 0.06, "triangle", 0.45))
save("BrickHit.wav",  glass_break())
save("HardHit.wav",  metal_tink())
save("BallLost.wav",  tone(600, 120, 0.6, "triangle", 0.5))
go = []
for f in (440, 349, 262):
    go += tone(f, f * 0.97, 0.22, "square", 0.3) + silence(0.03)
go += tone(196, 140, 0.6, "square", 0.3)
save("GameOver.wav", go)
clr = []
for f in (523, 659, 784):
    clr += tone(f, f, 0.12, "square", 0.28)
clr += tone(1047, 1047, 0.5, "square", 0.28)
save("Clear.wav", clr)
print(sorted(os.listdir(out)))
