using System.Collections;
using System.Collections.Generic;
using System.IO;
using Grappler.DataModel;
using UnityEngine;

public class PlayerRecorderController : MonoBehaviour
{
    private PlayerController _player;
    private PlayerPlayback _playerPlayback;
    private float _pollRate = 0.05f; 
    private bool _recording = false;
    private float _timePassed = 0.0f;

    void Awake()
    {
    	_player = transform.parent.GetComponent<PlayerController>();
    }

    public void StartRecording()
    {
		PlayerState tempPlayerState = new PlayerState(_player.gameObject.transform.position, 
													  _player.PlayerSprite.transform.rotation, 
													  _player.RopeOrigin.transform.rotation,
													  _player.WallHookSprite.transform.position,
													  _player.RopeLineRenderer, 
													  0.0f);

		_playerPlayback = new PlayerPlayback(tempPlayerState);
        _recording = true;
        StartCoroutine("Record");
    }

    public void PauseRecording()
    {
        _recording = false;
    }

    public void ResumeRecording()
    {
    	_recording = true;
    }

    public void DoneRecording()
    {
		StopCoroutine("Record");
		AddState(_timePassed);
    	_recording = false;

		var bytes = System.Text.Encoding.UTF8.GetBytes (JsonUtility.ToJson(_playerPlayback));
		var path = Path.Combine(Application.persistentDataPath, "PlayerData/playerGhostData.json");

		// create directory if it doesn't exist
		(new FileInfo(path)).Directory.Create();

        File.WriteAllBytes (path, bytes);
    }

    IEnumerator Record()
    {
        _timePassed = 0.0f;

        while(_recording)
        {
            if(_timePassed > _pollRate)
            {
				AddState(_timePassed);
				_timePassed = 0.0f;
            }
            else
				_timePassed = _timePassed + Time.deltaTime;

            yield return null;
        }
        yield return null;
    }

    void AddState(float deltaTime)
    {
		PlayerState tempPlayerState = new PlayerState(_player.gameObject.transform.position, 
													  _player.PlayerSprite.transform.rotation, 
													  _player.RopeOrigin.transform.rotation,
													  _player.WallHookSprite.transform.position,
													  _player.RopeLineRenderer, 
													  deltaTime);

		_playerPlayback.AddPlayerState(tempPlayerState);
    }
}
