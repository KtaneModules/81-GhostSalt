using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class EightyOneScript : MonoBehaviour {

    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] Buttons;
    public TextMesh Text;
    public Material BG;

    private int CorrectButton;
    private int PressesLeft = 6;
    private bool[] Grid = new bool[9];
    private bool Proceed = true;
    private bool Solved;

    void Awake()
    {
        _moduleID = _moduleIdCounter++;
        for (int i = 0; i < 9; i++)
        {
            int x = i;
            Buttons[i].OnInteract += delegate { StartCoroutine(ButtonPress(x)); return false; };
        }
        Text.text = "??";
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void CalcGrid(int pos)
    {
        PressesLeft--;
        Text.text = PressesLeft.ToString();
        for (int i = 0; i < 9; i++)
        {
            if (Rnd.Range(0, 2) == 0)
            {
                Grid[i] = false;
                Buttons[i].GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
            }
            else
            {
                Grid[i] = true;
                Buttons[i].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1);
            }
        }
        bool[] Cache = new bool[9];
        Cache[pos] = true;
        for (int i = 0; i < 9; i++)
        {
            bool[] Cache2 = new bool[9];
            for (int j = 0; j < 9; j++)
                Cache2[j] = Cache[j];
            if (!Grid[i])
            {
                Cache[0] = Cache2[6];
                Cache[1] = Cache2[3];
                Cache[2] = Cache2[0];
                Cache[3] = Cache2[7];
                Cache[5] = Cache2[1];
                Cache[6] = Cache2[8];
                Cache[7] = Cache2[5];
                Cache[8] = Cache2[2];
            }
            else
            {
                Cache[0] = Cache2[2];
                Cache[1] = Cache2[0];
                Cache[2] = Cache2[1];
                Cache[3] = Cache2[5];
                Cache[4] = Cache2[3];
                Cache[5] = Cache2[4];
                Cache[6] = Cache2[8];
                Cache[7] = Cache2[6];
                Cache[8] = Cache2[7];
            }
        }
        for (int i = 0; i < 9; i++)
        {
            if (Cache[i])
                CorrectButton = i;
        }
    }

    void Reset()
    {
        Module.HandleStrike();
        for (int i = 0; i < 9; i++)
        {
            Buttons[i].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1);
        }
        Proceed = true;
        PressesLeft++;
        Text.text = "??";
    }

    private IEnumerator ButtonPress(int pos)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, Buttons[pos].transform);
        Buttons[pos].AddInteractionPunch();
        for (int i = 0; i < 3; i++)
        {
            Buttons[pos].transform.localPosition -= new Vector3(0, 0.002f, 0);
            yield return null;
        }
        if (!Solved)
        {
            if (CorrectButton == pos || Proceed)
            {
                if (Proceed)
                    Debug.LogFormat("[81 #{0}] Button {1} was pressed to start the module.", _moduleID, (pos + 1).ToString());
                else
                    Debug.LogFormat("[81 #{0}] Button {1} was correctly pressed. Presses remaining: {2}.", _moduleID, (pos + 1).ToString(), PressesLeft - 1);
                Proceed = false;
                CalcGrid(pos);
                if (PressesLeft == 0)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        Buttons[i].GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0);
                    }
                    Text.color = new Color(0, 1, 0);
                    Text.text = "GG";
                    Audio.PlaySoundAtTransform("solve", Buttons[pos].transform);
                    Module.HandlePass();
                    Debug.LogFormat("[81 #{0}] Module solved. Poggers!", _moduleID);
                    Solved = true;
                }
                else
                    Audio.PlaySoundAtTransform("press", Buttons[pos].transform);
            }
            else
            {
                Reset();
                Debug.LogFormat("[81 #{0}] Button {1} was incorrectly pressed, expected button {2}. Presses remaining: {3}.", _moduleID, (pos + 1).ToString(), CorrectButton, PressesLeft - 1);
            }
        }
        for (int i = 0; i < 3; i++)
        {
            Buttons[pos].transform.localPosition += new Vector3(0, 0.002f, 0);
            yield return null;
        }
    }
#pragma warning disable 414
    private string TwitchHelpMessage = "Use '!{0} 2' to press the second button in reading order.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string validcmds = "123456789";
        if (!validcmds.Contains(command) || command.Length != 1)
        {
            yield return "sendtochaterror Invalid command.";
            yield break;
        }
        yield return null;
        Buttons[int.Parse(command) - 1].OnInteract();
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        yield return true;
        Buttons[0].OnInteract();
        yield return new WaitForSeconds(5 * Time.deltaTime);
        for (int i = 0; i < 30; i++)
        {
            Buttons[CorrectButton].OnInteract();
            yield return new WaitForSeconds(5 * Time.deltaTime);
        }
    }
}