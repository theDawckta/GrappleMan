using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Grappler;
using Grappler.Data;
using DG.Tweening;
using TMPro;

public class UIController : MonoBehaviour
{
    public delegate void StartButtonClicked();
    public event StartButtonClicked OnStartButtonClicked;

    public delegate void DoneButtonClicked();
    public event DoneButtonClicked OnDoneButtonClicked;

    public delegate void ResetDataButtonClicked();
    public event ResetDataButtonClicked OnResetDataButtonClicked;

	public delegate void ResetButtonClicked();
    public event ResetButtonClicked OnResetButtonClicked;

    public delegate void GhostValueChanged(int value);
    public event GhostValueChanged OnGhostsValueChanged;

    public delegate void GhostRecordsValueChanged(int value);
    public event GhostRecordsValueChanged OnGhostRecordsValueChanged;

	public delegate void UserSaveButtonClicked(string value);
	public event UserSaveButtonClicked OnUserSaveButtonClicked;

    public SmoothFollow Follow;
    public RectTransform UIPanel;
    public InputField SeedInputField;
    public Text FPSText;
    public Text TimerText;
    public GameObject Countdown;
    public TextMeshProUGUI CountdownText;
	public GameObject NoUsernameScreen;
    public GameObject StartScreen;
	public GameObject ConfigScreen;
    public GameObject PlayerRanksScreen;
    public PlayerRowController PlayerRow;
    public GameObject WaitScreen;
    public GameObject Spinner;
    public InputField GhostsInput;
    public InputField GhostRecordsInput;
	public InputField NewUserInput;
    public InputField UserInput;
	public GameObject UserEdit;
    public GameObject UserCancel;
    public GameObject UserSave;
	public Text UserName;
	public Text ErrorText;
	public Text TotalGhostRecordsLocal;
    
    private float _timer;
    private bool _timeStarted;
    private GraphicRaycaster _uiCanvasRaycaster;
    private Tweener _spinnerTweener;
    private Vector3 _startSpinnerRotation;
    private float _showHideTime = 1.0f;

    void Awake()
    {
        UserInput.gameObject.SetActive(false);
        UserSave.SetActive(false);
        UserCancel.SetActive(false);
        ErrorText.text = "";
        UIPanel.DOAnchorPosX((-UIPanel.rect.width) - UIPanel.offsetMin.x / 2, 0.0f);
        _uiCanvasRaycaster = gameObject.GetComponentInChildren<GraphicRaycaster>();
        _uiCanvasRaycaster.enabled = false;
        _startSpinnerRotation = Spinner.transform.localEulerAngles;
    }

    void Start()
    {
        StartCoroutine(FPS());
    }

    void Update()
    {
    	if (_timeStarted == true)
    	{
            _timer = _timer + Time.deltaTime;
            TimerText.text = GetTimeText(_timer);
		}
    }

    public void Show()
    {
        if (Follow != null)
            Follow.MenuOpen();

        UIPanel.DOAnchorPosX(35.0f, _showHideTime).OnComplete(() => {
            _uiCanvasRaycaster.enabled = true;
        });
    }
    
    public void Hide()
    {
        if (Follow != null)
            Follow.MenuClosed();

        UIPanel.DOAnchorPosX((-UIPanel.rect.width) - UIPanel.offsetMin.x / 2, _showHideTime).OnComplete(() => {
            WaitScreen.SetActive(true);
            DOTween.Kill(_spinnerTweener.target);
            Spinner.transform.localEulerAngles = _startSpinnerRotation;
        });
    }

    public void ShowCountdown()
    {
        CountdownText.text = "3";
        Countdown.SetActive(true);
    }

    public void UpdateCountdown(int time)
    {
        CountdownText.text = time.ToString();
    }

    public void HideCountdown()
    {
        Countdown.SetActive(false);
        _timeStarted = true;
    }

    public void InitPlayerRanksScreen(List<PlayerReplayModel> playerReplays, PlayerReplayModel currentPlayerReplay)
	{
        foreach (Transform playerRow in PlayerRanksScreen.transform.Find("Players"))
            Destroy(playerRow.gameObject);

        playerReplays.Add(currentPlayerReplay);
        playerReplays.Sort((x, y) => y.ReplayTime.CompareTo(x.ReplayTime));

        for (int i = 0; i < playerReplays.Count; i++)
		{
			PlayerRowController playerRow = (PlayerRowController)Instantiate(PlayerRow);
            playerRow.SetPlayerRow((i + 1) + ". " + playerReplays[i].UserName, GetTimeText(playerReplays[i].ReplayTime));

            if (currentPlayerReplay == playerReplays[i])
                playerRow.MarkRowAsPlayer();

            playerRow.transform.SetParent(PlayerRanksScreen.transform.Find("Players"), false);
		}

        WaitScreen.SetActive(false);
        PlayerRanksScreen.SetActive(true);
	}

	public void UIStartButtonClicked(string levelName)
	{
        StartScreen.SetActive(false);
        WaitScreen.SetActive(true);
        _spinnerTweener = Spinner.transform.DOLocalRotate(new Vector3(0.0f, 0.0f, -359.0f), 2.0f).SetLoops(-1).SetEase(Ease.Linear);
        OnStartButtonClicked();
	}

    public void UIDoneButtonClicked()
    {
        PlayerRanksScreen.SetActive(false);
        StartScreen.SetActive(true);
        _timer = 0.0f;
        TimerText.text = GetTimeText(_timer);
        OnDoneButtonClicked();
    }

    public void UIUserEditButtonClicked()
    {
        UserEdit.SetActive(false);
        UserInput.gameObject.SetActive(true);
		UserSave.SetActive(true);
		UserCancel.SetActive(true);
    }

	public void UIUserCancelButtonClicked()
    {
		UserEdit.SetActive(true);
        UserInput.gameObject.SetActive(false);
		UserSave.SetActive(false);
		UserCancel.SetActive(false);
    }

	public void UINewUserSaveButtonClicked()
    {
        OnUserSaveButtonClicked(NewUserInput.text.ToUpper());
    }

	public void UIUserSaveButtonClicked()
    {
		if(UserInput.text.Length > 0)
		{
			if(UserInput.text.ToUpper() == UserName.text)
			{
				UserEdit.SetActive(true);
        		UserInput.gameObject.SetActive(false);
				UserSave.SetActive(false);
				UserCancel.SetActive(false);
			}
			else
			{
				if (OnUserSaveButtonClicked != null)
					OnUserSaveButtonClicked(UserInput.text);
			}
		}
		else
		{
			UserEdit.SetActive(true);
        	UserInput.gameObject.SetActive(false);
			UserSave.SetActive(false);
			UserCancel.SetActive(false);
		}
    }

    public void UIResetDataButtonClicked()
    {
        if (OnResetDataButtonClicked != null)
            OnResetDataButtonClicked();
    }

	public void UIResetButtonClicked()
    {
        if (OnResetButtonClicked != null)
            OnResetButtonClicked();
    }

    public void UIGhostsValueChanged(string value)
    {
        if (OnGhostsValueChanged != null)
        {
            OnGhostsValueChanged(Int32.Parse(GhostsInput.text));
            PlayerPrefs.SetInt(Constants.GHOSTS, Int32.Parse(GhostsInput.text));
        }
    }

    public void UIGhostRecordsValueChanged(string value)
    {
        if (OnGhostRecordsValueChanged != null)
        {
            OnGhostRecordsValueChanged(Int32.Parse(GhostRecordsInput.text));
            PlayerPrefs.SetInt(Constants.NUM_OF_LOCAL_GHOST_RECORDS, Int32.Parse(GhostRecordsInput.text));
        }
    }

	public void UIDeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        ConfigScreen.SetActive(false);
        StartScreen.SetActive(false);
        NoUsernameScreen.SetActive(true);
    }

    public void EndGame()
    {
        Show();
        _timeStarted = false;
    }

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new System.Random();
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

	public void OpenConfigScreen()
    {
        StartScreen.gameObject.SetActive(false);
		ConfigScreen.gameObject.SetActive(true);
    }

    public void CloseConfigScreen()
    {
        StartScreen.gameObject.SetActive(true);
        ConfigScreen.gameObject.SetActive(false);
    }

    string GetTimeText (float time)
	{
		return string.Format("{0:00}:{1:00}.{2:00}",
						     Mathf.Floor(time / 60),
							 Mathf.Floor(time) % 60,
							 Mathf.Floor((time * 100) % 100));
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

    public void SetErrorText(string errorText)
    {
		ErrorText.text = errorText;
		ErrorText.DOFade(1.0f, 0.5f);
		ErrorText.DOFade(0.0f, 0.5f).SetDelay(3.0f).OnComplete(() => {
            ErrorText.text = "";
         });
    }
}
