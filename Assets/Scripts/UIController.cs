using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;

public class UIController : MonoBehaviour {

    public Text EndText;
    public Text DistanceTraveled;
    public InputField SeedInputField;
    public GameObject ParentCanvas;
    public LevelGenerator _LevelGenerator;
    public LevelController _LevelController;
    public int SeedLength;

    private bool GameOn = false;

    private String seed;

	void Start () 
    {
        DistanceTraveled.text = "0 m";
        ShowPanel("StartPanel");
	}
	
	void Update () 
    {
        if (_LevelController.GameOn)
            DistanceTraveled.text = _LevelController.DistanceTraveled.ToString() + " m";
	}

    public void NewGame()
    {
        DistanceTraveled.text = "0 m";
        ShowPanel("StartPanel");
        _LevelController.Init();
    }

    public void RestartGame()
    {
        DistanceTraveled.text = "0 m";
        ToggleUIController();
        _LevelController.Init();
    }

    public void EndGame()
    {
        EndText.text = "You traveled " + DistanceTraveled.text;
        ToggleUIController();
        ShowPanel("EndPanel");
    }

    public void RandomSeed()
    {
        seed = RandomString(SeedLength);
        SeedInputField.text = seed;

        _LevelGenerator.MakeLevel(seed);
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

    void ShowPanel(string panelName)
    {
        foreach (Transform child in ParentCanvas.transform)
        {
            child.gameObject.SetActive(false);
        }

        ParentCanvas.SetActive(true);
        ParentCanvas.transform.Find(panelName).gameObject.SetActive(true);
    }
}
