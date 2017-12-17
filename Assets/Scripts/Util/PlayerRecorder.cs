using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRecorder : MonoBehaviour
{
    public PlayerController Player;
    public float PollRate = 2.0f; 

    private List<Vector3> _playerPosition = new List<Vector3>();
    private List<Vector3> _playerRotation = new List<Vector3>();
    private bool _recording = false;
    private Coroutine _recordingCoroutine;

    public void StartRecording()
    {
        _recording = true;
        _recordingCoroutine = StartCoroutine("Record");
    }

    public void PauseRecording()
    {
        _recording = false;
    }

    public void DoneRecording()
    {

    }

    IEnumerator Record()
    {
        float timePassed = 0.0f;

        while(_recording)
        {
            if(timePassed > PollRate)
            {
                _playerPosition.Add(Player.PlayerSprite.transform.position);
                _playerRotation.Add(Player.PlayerSprite.transform.rotation);
            }

            timePassed = timePassed + Time.deltaTime;
        }
        yield return null;
    }
}
