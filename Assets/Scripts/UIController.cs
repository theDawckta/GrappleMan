using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;

public class UIController : MonoBehaviour
{
    public InputField SeedInputField;
    public Slider Volume;
    public GameObject ParentCanvas;
    public PlayerController _PlayerController;
    public SceneController _SceneController;
    public int SeedLength;

    private String seed;

	void Start () 
    {
        seed = RandomString(SeedLength);
        SeedInputField.text = seed;
        // init levelgenerator here when ready
        // LevelGenerator.Init(seed);
	}

    public void NewGame()
    {
        _SceneController.Init();
        ToggleUIController();
    }

    public void RestartGame()
    {
        ToggleUIController();
        _SceneController.Init();
    }

    public void EndGame()
    {
        ToggleUIController();
    }

    public void SetSeed()
    {
        if(SeedInputField.text != "")
        {
            seed = SeedInputField.text;

            // init levelgenerator here when ready
            // LevelGenerator.Init(seed);
        }
    }

    public void RandomSeed()
    {
        seed = RandomString(SeedLength);
        SeedInputField.text = seed;

        // init levelgenerator here when ready
        // LevelGenerator.Init(seed);
    }

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new System.Random();
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public void ToggleUIController()
    {
        ParentCanvas.gameObject.SetActive(!ParentCanvas.gameObject.activeSelf);
    }

    public void OnVolumeChanged()
    {
        _SceneController.SetVolume(Volume.value);
    }
}
