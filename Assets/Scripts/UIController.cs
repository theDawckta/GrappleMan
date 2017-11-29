using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;

public class UIController : MonoBehaviour
{
    public InputField SeedInputField;
    public Slider Volume;
    public Text TestText;
    public Text FPSText;
    public InputField ClimbSpeedText;
    public GameObject ParentCanvas;
    public PlayerController _PlayerController;
    public SceneController _SceneController;
    public int SeedLength;

    private String seed;

    void Start()
    {
        StartCoroutine(FPS());
        seed = RandomString(SeedLength);
        SeedInputField.text = seed;
        // init levelgenerator here when ready
        // LevelGenerator.Init(seed);
        ClimbSpeedText.text = _PlayerController.ClimbSpeed.ToString();

    }

    public void NewGame()
    {
        _PlayerController.Init();
        //ToggleUIController();
    }

    public void EndGame()
    {
        ToggleUIController();
    }

    public void SetSeed()
    {
        if (SeedInputField.text != "")
        {
            seed = SeedInputField.text;

            // init levelgenerator here when ready
            // LevelGenerator.Init(seed);
        }
    }

    public void SetTestText(string newText)
    {
        TestText.text = newText;
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

    public void OnVolumeChanged()
    {
        _SceneController.SetVolume(Volume.value);
    }

    public void OnClimbSpeedChanged()
    {
        _PlayerController.ClimbSpeed = float.Parse(ClimbSpeedText.text);
    }

    void ToggleUIController()
    {
        ParentCanvas.gameObject.SetActive(!ParentCanvas.gameObject.activeSelf);
    }

    IEnumerator FPS()
    {
        for (;;)
        {
            int lastFrameCount = Time.frameCount;
            float lastTime = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(0.5f);
            float timeSpan = Time.realtimeSinceStartup - lastTime;
            int frameCount = Time.frameCount - lastFrameCount;
            FPSText.text = Mathf.RoundToInt(frameCount / timeSpan) + " fps";
        }
    }
}
