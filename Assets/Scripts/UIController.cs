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
    public InputField EnemyMin;
    public InputField EnemyMax;
    public GameObject ParentCanvas;
    public SideScrollerGenerator _SideScrollerGenerator;
    public PlayerController _PlayerController;
    public LevelController _LevelController;
    public int SeedLength;

    private String seed;

	void Start () 
    {
        seed = RandomString(SeedLength);
        SeedInputField.text = seed;
        _SideScrollerGenerator.Init(seed);
        DistanceTraveled.text = "0 m";
        ShowPanel("StartCanvas");
	}
	
	void Update () 
    {
        if(_LevelController._PlayerController.playerStarted)
            DistanceTraveled.text = _PlayerController.DistanceTraveled.ToString() + " m";
	}

    public void NewGame()
    {
        DistanceTraveled.text = "0 m";
        ShowPanel("StartCanvas");
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

    public void SetSeed()
    {
        if(SeedInputField.text != "")
        {
            seed = SeedInputField.text;

            _SideScrollerGenerator.Init(seed);

            _SideScrollerGenerator.MakeLevel(Int32.Parse(WidthMin.text),
                                            Int32.Parse(WidthMax.text),
                                            Int32.Parse(HeightMin.text),
                                            Int32.Parse(HeightMax.text),
                                            Int32.Parse(DepthMin.text),
                                            Int32.Parse(DepthMax.text),
                                            Int32.Parse(EnemyMin.text),
                                            Int32.Parse(EnemyMax.text),
                                            new Vector3(0.0f, 0.0f, 0.0f));
        }
    }

    public void RandomSeed()
    {
        seed = RandomString(SeedLength);
        SeedInputField.text = seed;

        _SideScrollerGenerator.Init(seed);

        _SideScrollerGenerator.MakeLevel(Int32.Parse(WidthMin.text),
                                        Int32.Parse(WidthMax.text),
                                        Int32.Parse(HeightMin.text),
                                        Int32.Parse(HeightMax.text),
                                        Int32.Parse(DepthMin.text),
                                        Int32.Parse(DepthMax.text),
                                        Int32.Parse(EnemyMin.text),
                                        Int32.Parse(EnemyMax.text),
                                        new Vector3(0.0f, 0.0f, 0.0f));
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
