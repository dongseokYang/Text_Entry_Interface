using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class Manager : MonoBehaviour
{
    public static Manager Instance;

    private TextMeshPro tm_main;
    private TextMeshPro tm_log;
    private int currentPhrase = 0;
    private int currentWord = 0;
    private string inputPrev = "";
    private string input = "";

    private List<KeyCode> KeyList;
    private DateTime timer;
    private DateTime endTime;
    private bool isTyping = false;
    private int totalCharacters = 0;
    private int totalWords = 0;
    private int correctWords = 0;
    private int characterErrors = 0;

    private bool isEnd = false;
    private GameObject set_end;
    private Text txt_finishLog;
    private InputField input_fileName;
    private Button btn_save;

    void Start()
    {
        Instance = this;
        Screen.SetResolution(1280, 720, false);
        
        tm_main = GameObject.Find("tm_main").GetComponent<TextMeshPro>();
        tm_log = GameObject.Find("tm_log").GetComponent<TextMeshPro>();
        timer = DateTime.MinValue;

        set_end = GameObject.Find("set_end");
        txt_finishLog = GameObject.Find("txt_finishLog").GetComponent<Text>();
        input_fileName = GameObject.Find("input_fileName").GetComponent<InputField>();
        btn_save = GameObject.Find("btn_save").GetComponent<Button>();
        GameObject.Find("btn_save").GetComponent<Button>().onClick.AddListener(() => Save(false));
        GameObject.Find("btn_autosave").GetComponent<Button>().onClick.AddListener(() => Save(true));
        set_end.SetActive(false);

        KeyList = new List<KeyCode>();
        for (int i = 'a'; i <= 'z'; i++)
        {
            KeyList.Add((KeyCode)i);
        }
        KeyList.Add(KeyCode.Backspace);
        KeyList.Add(KeyCode.Return);
        KeyList.Add(KeyCode.Space);

        UpdateText();
    }

    private void Save(bool isAutoSave)
    {
        string dir = Application.dataPath + "/data";
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string file = dir + "/";

        if (isAutoSave)
        {
            file += DateTime.Now.ToString("yyyyMMddHHmmss");
        }
        else
        {
            if (input_fileName.text == "") return;
            file += input_fileName.text;
        }

        while (File.Exists(file + ".txt"))
        {
            file += "1";
        }

        File.WriteAllLines(file + ".txt", GetContents().ToArray());

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    private List<string> GetContents()
    {
        int WPM = (int)(correctWords / (endTime - timer).TotalMinutes);
        float CER = (float)characterErrors / totalCharacters;
        double timeElapse = (endTime - timer).TotalSeconds;
        if (CER == float.NaN) CER = 0;
        if (timer == DateTime.MinValue) timeElapse = 0;

        List<string> contents = new List<string>();
        contents.Add("Total Phrases: " + currentPhrase);
        contents.Add("Total Words: " + totalWords);
        contents.Add("Total Characters: " + totalCharacters);
        contents.Add("");
        contents.Add("Correct Words: " + correctWords);
        contents.Add("Incorrect Words: " + (totalWords - correctWords));
        contents.Add("");
        contents.Add("Time Elapsed: " + timeElapse.ToString("F2") + " s");
        contents.Add("");
        contents.Add("CER (Character Error Rate): " + CER.ToString("P2"));
        contents.Add("WPM (Word per Minute): " + WPM);
        contents.Add("");
        contents.Add("Entered Phrases:");
        for(int i = 0; i < currentPhrase; i++)
        {
            contents.Add((i + 1).ToString("00") + ". " +  PhraseLoader.GetPhrase(i));
        }

        return contents;
    }

    void Update()
    {
        GetStop();
        InputKey();
        UpdateLog();
    }

    private void GetStop()
    {
        if (isEnd) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Stop();
        }
    }

    private void Stop()
    {
        isEnd = true;
        set_end.SetActive(true);

        endTime = DateTime.Now;
        int WPM = (int)(correctWords / (endTime - timer).TotalMinutes);
        float CER = (float)characterErrors / totalCharacters;
        double timeElapse = (endTime - timer).TotalSeconds;
        if (CER == float.NaN) CER = 0;
        if (timer == DateTime.MinValue) timeElapse = 0;

        string log1 = "Correct/Total Words: " + correctWords + " / " + totalWords + ", CER: " + CER.ToString("P2") +
            "\nTime Elapsed: " + timeElapse.ToString("F2") + "s, WPM: " + WPM;
        tm_log.text = log1;
        Debug.Log("===END==\n" + log1);

        string log2 = currentPhrase + Environment.NewLine;
        log2 += totalWords + Environment.NewLine;
        log2 += totalCharacters + Environment.NewLine;
        log2 += Environment.NewLine;
        log2 += correctWords + Environment.NewLine;
        log2 += (totalWords - correctWords) + Environment.NewLine;
        log2 += Environment.NewLine;
        log2 += timeElapse.ToString("F2") + " s" + Environment.NewLine;
        log2 += Environment.NewLine;
        log2 += CER.ToString("P2") + Environment.NewLine;
        log2 += WPM + Environment.NewLine;
        txt_finishLog.text = log2;
    }

    private void UpdateLog()
    {
        if (isEnd) return;

        int WPM = (int)(correctWords / (DateTime.Now - timer).TotalMinutes);
        float CER = (float)characterErrors / totalCharacters;
        double timeElapse = (DateTime.Now - timer).TotalSeconds;
        if (CER == float.NaN) CER = 0;
        if (timer == DateTime.MinValue) timeElapse = 0;

        string log = "Correct/Total Words: " + correctWords + " / " + totalWords + ", CER: " + CER.ToString("P2") +
            "\nTime Elapsed: " + timeElapse.ToString("F2") + "s, WPM: " + WPM;
        tm_log.text = log;
    }

    public void InputKey(KeyCode key)
    {
        if (isEnd) return;

        if (!isTyping)
        {
            timer = DateTime.Now;
            isTyping = true;
        }

        if (key >= KeyCode.A && key <= KeyCode.Z)
        {
            input += key.ToString().ToLower();
        }
        else if (key == KeyCode.Backspace)
        {
            if (input.Length != 0)
                input = input.Substring(0, input.Length - 1);
        }
        else if (key == KeyCode.Space)
        {
            if (input.Length == 0)
                return;
            else if (input[input.Length - 1] == ' ')
                return;

            input += " ";
        }
        else if (key == KeyCode.Return)
        {
            string phrase = PhraseLoader.GetPhrase(currentPhrase);
            if (input.Length < (phrase.Length * 2 / 3))
                return;

            string[] words = phrase.Split(' ');
            string[] inputs = input.Split(' ');
            characterErrors += GetCER(phrase, input);
            totalCharacters += phrase.Length;
            totalWords += words.Length;
            for (int i = 0; i < words.Length; i++)
            {
                if (inputs.Length <= i)
                    continue;
                if (words[i].Equals(inputs[i]))
                    correctWords++;
            }

            inputPrev = input;
            input = "";
            currentPhrase++;

            if (currentPhrase == 10)
                Stop();
        }

        UpdateText();
    }
    private void InputKey()
    {
        if (isEnd) return;

        KeyCode? key = GetDownKey();
        if (key == null) return;

        if(!isTyping)
        {
            timer = DateTime.Now;
            isTyping = true;
        }

        if (key.Value == KeyCode.Backspace)
        {
            if (input.Length != 0)
                input = input.Substring(0, input.Length - 1);
        }
        else if (key.Value == KeyCode.Space)
        {
            if (input.Length == 0)
                return;
            else if (input[input.Length - 1] == ' ')
                return;

            input += " ";
        }
        else if (key.Value == KeyCode.Return)
        {
            string phrase = PhraseLoader.GetPhrase(currentPhrase);
            if (input.Length < (phrase.Length * 2 / 3))
                return;

            string[] words = phrase.Split(' ');
            string[] inputs = input.Split(' ');
            characterErrors += GetCER(phrase, input);
            totalCharacters += phrase.Length;
            totalWords += words.Length;
            for (int i = 0; i < words.Length; i++)
            {
                if (inputs.Length <= i)
                    continue;
                if (words[i].Equals(inputs[i]))
                    correctWords++;
            }

            inputPrev = input;
            input = "";
            currentPhrase++;

            if (currentPhrase == 10)
                Stop();
        }
        else
        {
            input += key.Value.ToString().ToLower();
        }

        UpdateText();
    }
    private KeyCode? GetDownKey()
    {
        for (int i = 0; i < KeyList.Count; i++)
        {
            if (Input.GetKeyDown(KeyList[i]))
                return KeyList[i];
        }
        return null;
    }

    // colors   current previous
    // normal   #ffffff #cccccc
    // red      #ff3333 #cc0000
    // green    #33ff33 #00cc00
    private readonly Dictionary<string, string> ColorTable = new Dictionary<string, string>()
    {
        { "nc", "<#ffffff>" },
        { "rc", "<#ff3333>" },
        { "gc", "<#33ff33>" },
        { "np", "<#888888>" },
        { "rp", "<#880000>" },
        { "gp", "<#008800>" },
    };
    private bool isAutoSave;

    private void UpdateText()
    {
        if (PhraseLoader.IsEnd(currentPhrase)) return;

        string phrase = PhraseLoader.GetPhrase(currentPhrase);
        if (currentPhrase == 0)
        {
            tm_main.text = Environment.NewLine + Environment.NewLine;
        }
        else
        {
            tm_main.text = GetTypo(PhraseLoader.GetPhrase(currentPhrase - 1), inputPrev, true);
            tm_main.text += Environment.NewLine;
            tm_main.text += ColorTable["np"] + inputPrev + "</color>";
            tm_main.text += Environment.NewLine;
        }

        tm_main.text += GetTypo(PhraseLoader.GetPhrase(currentPhrase), input, false);
        tm_main.text += Environment.NewLine;
        tm_main.text += input;
        tm_main.text += Environment.NewLine + ColorTable["np"];

        for (int i = currentPhrase + 1; i < currentPhrase + 3; i++)
        {
            if (PhraseLoader.IsEnd(i)) break;
            tm_main.text += PhraseLoader.GetPhrase(i).ToLower();
            tm_main.text += Environment.NewLine + Environment.NewLine;
        }
    }

    private string GetTypo(string r, string h, bool isPrev)
    {
        r = r.ToLower();
        string[] words = r.Split(' ');
        string[] inputs = h.Split(' ');

        string typo = "";

        for(int i = 0; i < words.Length; i++)
        {
            if (inputs.Length > i)
            {
                if (words[i].Length != inputs[i].Length && inputs.Length - 1 != i)
                {
                    typo += (isPrev? ColorTable["rp"] : ColorTable["rc"])  + words[i] + "</color> ";
                }
                else
                {
                    int wordTypo = GetWordTypo(words[i], inputs[i]);
                    switch (wordTypo)
                    {
                        case 0:
                            typo += (isPrev ? ColorTable["np"] : ColorTable["nc"]) + words[i] + "</color> ";
                            break;
                        case 1:
                            typo += (isPrev ? ColorTable["gp"] : ColorTable["gc"]) + words[i] + "</color> ";
                            break;
                        case 2:
                            typo += (isPrev ? ColorTable["rp"] : ColorTable["rc"]) + words[i] + "</color> ";
                            break;
                    }
                }
            }
            else
            {
                if(isPrev)
                    typo += (isPrev ? ColorTable["rp"] : ColorTable["rc"]) + words[i] + "</color> ";
                else
                    typo += (isPrev ? ColorTable["np"] : ColorTable["nc"]) + words[i] + "</color> ";
            }
        }

        return typo;
    }
    // 0: none, 1: true, 2: false
    private int GetWordTypo(string word, string inWord)
    {
        if (inWord.Length > word.Length)
            return 2;

        for(int i = 0; i < word.Length; i++)
        {
            if (inWord.Length == i)
                return 0;
            if (inWord[i] != word[i])
                return 2;
        }
        return 1;
    }

    // Ref: https://martin-thoma.com/word-error-rate-calculation/
    private int GetCER(string r, string h)
    {
        // Installation
        int[,] d = new int[r.Length + 1, h.Length + 1];
        for(int i = 0; i <= r.Length; i++)
        {
            for (int j = 0; j <= h.Length; j++)
            {
                if (i == 0)
                    d[0, j] = j;
                else if (j == 0)
                    d[i, 0] = i;
            }
        }

        // Calculation
        for (int i = 1; i <= r.Length; i++)
        {
            for (int j = 1; j <= h.Length; j++)
            {
                if (r[i - 1] == h[j - 1])
                {
                    d[i, j] = d[i - 1, j - 1];
                }
                else
                {
                    int sub = d[i - 1, j - 1] + 1;
                    int ins = d[i, j - 1] + 1;
                    int del = d[i - 1, j] + 1;
                    d[i, j] = Mathf.Min(sub, ins, del);
                }
            }
        }

        return d[r.Length, h.Length];
    }
}