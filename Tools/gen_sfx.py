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

def silence(dur): return [0.0] * int(SR * dur)

def save(name, samples):
    with wave.open(os.path.join(out, name), "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, x)) * 32767)) for x in samples))

save("PaddleHit.wav", tone(520, 520, 0.08, "square", 0.30))
save("WallHit.wav",   tone(260, 240, 0.06, "triangle", 0.45))
save("BrickHit.wav",  tone(900, 1300, 0.09, "square", 0.25))
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
