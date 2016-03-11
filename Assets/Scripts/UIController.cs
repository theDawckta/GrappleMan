using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;

public class UIController : MonoBehaviour {

    public Text EndText;
    public Text DistanceTraveled;
    public InputField SeedInputField;
    public InputField WidthMin;
    public InputField WidthMax;
    public InputField HeightMin;
    public InputField HeightMax;
    public InputField DepthMin;
    public InputField DepthMax;
    public GameObject ParentCanvas;
    public LevelGenerator _LevelGenerator;
    public LevelController _LevelController;
    public int SeedLength;

    private bool GameOn = false;

    private String seed;

	void Start () 
    {
        seed = RandomString(SeedLength);
        SeedInputField.text = seed;
        _LevelGenerator.Init(seed);
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
        EndText.text = "You traveled " + DistanceTraveled.text + " m";
        ToggleUIController();
        ShowPanel("EndPanel");
    }

    public void SetSeed()
    {
        if(SeedInputField.text != "")
        {
            seed = SeedInputField.text;

            _LevelGenerator.Init(seed);

            _LevelGenerator.MakeLevel(Int32.Parse(WidthMin.text),
                                      Int32.Parse(WidthMax.text),
                                      Int32.Parse(HeightMin.text),
                                      Int32.Parse(HeightMax.text),
                                      Int32.Parse(DepthMin.text),
                                      Int32.Parse(DepthMax.text),
                                      new Vector3(_LevelGenerator.TotalLength / 2, 0.0f, 0.0f));
        }
    }

    public void RandomSeed()
    {
        seed = RandomString(SeedLength);
        SeedInputField.text = seed;

        _LevelGenerator.Init(seed);

        _LevelGenerator.MakeLevel(Int32.Parse(WidthMin.text),
                                  Int32.Parse(WidthMax.text),
                                  Int32.Parse(HeightMin.text),
                                  Int32.Parse(HeightMax.text),
                                  Int32.Parse(DepthMin.text),
                                  Int32.Parse(DepthMax.text),
                                  new Vector3(_LevelGenerator.TotalLength / 2, 0.0f, 0.0f));
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

    public void CheckInputs()
    {
        if(WidthMin.text == "0")
        {
            WidthMin.text = "1";
        }

        if (WidthMax.text == "0")
        {
            WidthMax.text = "1";
        }

        if (HeightMin.text == "0")
        {
            HeightMin.text = "1";
        }

        if (HeightMax.text == "0")
        {
            HeightMax.text = "1";
        }

        if (DepthMin.text == "0")
        {
            DepthMin.text = "1";
        }

        if (DepthMax.text == "0")
        {
            DepthMax.text = "1";
        }
    }
}
