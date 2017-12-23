using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;

public class UIController : MonoBehaviour
{
	public delegate void StartButtonClicked();
	public event StartButtonClicked OnStartButtonClicked;

    public InputField SeedInputField;
    public Text FPSText;
    public Text TimerText;
    public GameObject StartScreen;
    public int SeedLength;
    public Button StartButton;

    private String seed;
    private float _timer;
    private bool _timeStarted;

    void Start()
    {
        StartCoroutine(FPS());
        seed = RandomString(SeedLength);
        // SeedInputField.text = seed;
        // init levelgenerator here when ready
        // LevelGenerator.Init(seed);
    }

    void Update()
    {
    	if (_timeStarted == true)
    	{
    		UpdateTimer();
			_timer += Time.deltaTime;
		}
    }

	public void UIStartButtonClicked()
    {
		if(OnStartButtonClicked != null)
			OnStartButtonClicked();
		ToggleStartScreen();
        _timer = 0.0f;
		_timeStarted = true;
    }

    public void EndGame()
    {
        ToggleStartScreen();
        _timeStarted = false;
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

    void ToggleStartScreen()
    {
        StartScreen.gameObject.SetActive(!StartScreen.gameObject.activeSelf);
    }

	void UpdateTimer ()
	{
		TimerText.text = string.Format("{0:00}:{1:00}.{2:00}",
						     Mathf.Floor(_timer / 60),
							 Mathf.Floor(_timer) % 60,
							 Mathf.Floor((_timer * 100) % 100));
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
			FPSText.text = "FPS: " + Mathf.RoundToInt(frameCount / timeSpan);
        }
    }
}
