using UnityEngine;

/// <summary>아이템 스프라이트(둥근 캡슐 + 픽셀 글자)를 런타임에 생성.</summary>
public static class ItemSprites
{
    const int W = 32, H = 16, GlyphScale = 2;

    // 5x5 픽셀 글자 (위쪽 줄부터)
    static string[] Glyph(char c) => c switch
    {
        '+' => new[] { "..#..", "..#..", "#####", "..#..", "..#.." },
        '-' => new[] { ".....", ".....", "#####", ".....", "....." },
        'W' => new[] { "#...#", "#...#", "#.#.#", "#.#.#", ".#.#." },
        'S' => new[] { ".####", "#....", ".###.", "....#", "####." },
        _   => new[] { ".....", ".....", ".....", ".....", "....." },
    };

    public static Sprite Create(Color body, char letter, float worldWidth)
    {
        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };

        Color border = new Color(body.r * 0.55f, body.g * 0.55f, body.b * 0.55f, 1f);
        float luminance = 0.299f * body.r + 0.587f * body.g + 0.114f * body.b;
        Color glyphColor = luminance > 0.6f ? new Color(0.15f, 0.15f, 0.18f) : Color.white;

        var pixels = new Color[W * H];
        for (int y = 0; y < H; y++)
        for (int x = 0; x < W; x++)
        {
            // 모서리를 깎아 캡슐 느낌
            int dx = Mathf.Min(x, W - 1 - x), dy = Mathf.Min(y, H - 1 - y);
            Color c;
            if (dx + dy < 3) c = Color.clear;
            else if (dx == 0 || dy == 0 || dx + dy == 3) c = border;
            else c = body;
            pixels[y * W + x] = c;
        }

        string[] g = Glyph(letter);
        int gw = 5 * GlyphScale, gh = 5 * GlyphScale;
        int x0 = (W - gw) / 2, y0 = (H - gh) / 2;
        for (int row = 0; row < 5; row++)
        for (int col = 0; col < 5; col++)
        {
            if (g[row][col] != '#') continue;
            for (int sy = 0; sy < GlyphScale; sy++)
            for (int sx = 0; sx < GlyphScale; sx++)
            {
                int px = x0 + col * GlyphScale + sx;
                int py = y0 + (4 - row) * GlyphScale + sy; // 텍스처는 아래쪽이 y=0
                pixels[py * W + px] = glyphColor;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), W / worldWidth);
    }
}
