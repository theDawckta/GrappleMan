using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerInput : MonoBehaviour
{
    public bool InputActive = false;
    public UIController UI;

    private bool _rightTouchStarted = false;
    private Vector2 _rightStart = new Vector2();
    private Vector2 _rightSwipe = new Vector2();
    private bool _rightTouchDone = false;
    private int _minSwipeDistance;
    private int _minDPadDistance;

    void Start()
    {
        _minSwipeDistance = Screen.height * 10 / 100;
        _minDPadDistance = Screen.height * 3 / 100;
    }
    void Update()
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                _rightTouchStarted = true;
                _rightStart = touch.position;
            }
            if (touch.phase == TouchPhase.Moved)
                _rightSwipe = touch.position - _rightStart;
            if (touch.phase == TouchPhase.Ended)
            {
                _rightTouchDone = true;
                _rightTouchStarted = false;
                _rightSwipe = touch.position - _rightStart;
            }
        }
    }

    public bool HookButtonDown()
    {
        if (InputActive)
        {
#if (UNITY_STANDALONE || UNITY_EDITOR)
            return (Input.GetButtonDown("Fire1"));
#elif (UNITY_IOS || UNITY_ANDROID)
            if (_rightTouchDone)
            {
                _rightTouchDone = false;
                return true;
            }
            else
                return false;
#endif
        }
        else
            return false;
    }

    public bool ClimbButtonPressed()
    {
        if (InputActive)
        {
#if (UNITY_STANDALONE || UNITY_EDITOR)
            return Input.GetKey(KeyCode.Q) ? true : false;
#elif (UNITY_IOS || UNITY_ANDROID)
            if(_rightTouchStarted)
                return true;
            else
                return false;
#endif
        }
        else
            return false;
    }

    public Vector3 GetDirection()
    {
        if (InputActive)
        {
#if (UNITY_STANDALONE || UNITY_EDITOR)
            Vector3 wallHookPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                                                                                  Input.mousePosition.y,
                                                                                  -(Camera.main.transform.position.z + transform.position.z)));
            Vector3 direction = wallHookPosition - transform.position;
            return direction;

#elif (UNITY_IOS || UNITY_ANDROID)
            if(Mathf.Abs(_rightSwipe.x) > _minSwipeDistance || Mathf.Abs(_rightSwipe.y) > _minSwipeDistance)
                return _rightSwipe;
            else
            return Vector3.zero;
#endif
        }
        else
            return Vector3.zero;
    }
}