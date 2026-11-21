using System.Collections.Generic;
using UnityEngine;

/// <summary>최고 점수 TOP N을 PlayerPrefs에 저장/조회.</summary>
public static class HighScores
{
    public const int MaxEntries = 5;
    const string KeyPrefix = "Breakout.HighScore.";

    public static List<int> Load()
    {
        var list = new List<int>(MaxEntries);
        for (int i = 0; i < MaxEntries; i++)
        {
            string key = KeyPrefix + i;
            if (PlayerPrefs.HasKey(key)) list.Add(PlayerPrefs.GetInt(key));
        }
        return list;
    }

    /// <summary>점수를 기록하고 순위(1부터)를 반환. 순위권 밖이면 0.</summary>
    public static int Submit(int score)
    {
        if (score <= 0) return 0;

        var list = Load();
        int index = list.FindIndex(s => score > s);
        if (index < 0) index = list.Count;
        if (index >= MaxEntries) return 0;

        list.Insert(index, score);
        if (list.Count > MaxEntries) list.RemoveAt(list.Count - 1);

        for (int i = 0; i < list.Count; i++)
            PlayerPrefs.SetInt(KeyPrefix + i, list[i]);
        PlayerPrefs.Save();
        return index + 1;
    }
}
