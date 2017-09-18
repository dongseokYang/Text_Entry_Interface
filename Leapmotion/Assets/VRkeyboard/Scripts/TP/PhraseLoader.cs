using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PhraseLoader
{
    private static Dictionary<int, string> Phrases;
    private static int[] list;

    private static void LoadPhrases()
    {
        TextAsset asset = Resources.Load<TextAsset>("phrases");
        string[] phraseArray = asset.text.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);

        Phrases = new Dictionary<int, string>();
        list = new int[phraseArray.Length];
        for (int i = 0; i < phraseArray.Length; i++)
        {
            Phrases.Add(i, phraseArray[i]);
            list[i] = i;
        }

        System.Random r = new System.Random();
        list = list.OrderBy(x => r.Next()).ToArray();
    }

    public static string GetPhrase(int i)
    {
        if (Phrases == null)
            LoadPhrases();

        return Phrases[list[i]];
    }

    public static bool IsEnd(int i)
    {
        if (Phrases == null || list == null) return false;

        if (list.Length <= i) return true;
        else return false;
    }
}