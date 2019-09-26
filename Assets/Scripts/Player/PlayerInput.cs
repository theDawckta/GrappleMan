using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public bool InputActive = false;
    public int MinSwipeDistance = 15;
    public bool TouchStarted 
    {
        get
        {
#if (UNITY_STANDALONE || UNITY_EDITOR)
            if (_disableLeftScreen && Input.mousePosition.x < Screen.width / 2)
            {
                return false;
            }
            else
                return (Input.GetButtonDown("Fire1"));
#elif (UNITY_IOS || UNITY_ANDROID)
            if (_disableLeftScreen && Input.mousePosition.x < Screen.width / 2)
            {
                return false;
            }
            else if(_touchStarted)
                return true;
            else
                return false;
#endif
        }
        private set{} 
    }

    private bool _touchStarted = false;
    private Vector2 _touchStartPosition = new Vector2();
    private Vector2 _swipeDirection = new Vector2();
    private bool _touchDone = false;

    private bool _disableLeftScreen = true;

    void Update()
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                _touchStarted = true;
                _touchStartPosition = touch.position;
            }
            if (touch.phase == TouchPhase.Moved)
                _swipeDirection = touch.position - _touchStartPosition;
            if (touch.phase == TouchPhase.Ended)
            {
                _touchDone = true;
                _touchStarted = false;
                _swipeDirection = touch.position - _touchStartPosition;
            }
        }
    }

    public bool HookButtonDown()
    {
        if (InputActive)
        {
#if (UNITY_STANDALONE || UNITY_EDITOR)
            if (_disableLeftScreen && Input.mousePosition.x < Screen.width / 2)
            {
                return false;
            }
            else
                return (Input.GetButtonDown("Fire1"));
#elif (UNITY_IOS || UNITY_ANDROID)
            if (_disableLeftScreen && Input.mousePosition.x < Screen.width / 2)
            {
                return false;
            }
            else if (_touchDone)
            {
                _touchDone = false;
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
            return Input.GetKey(KeyCode.E) ? true : false;
#elif (UNITY_IOS || UNITY_ANDROID)
            if (_disableLeftScreen && Input.mousePosition.x < Screen.width / 2)
            {
                return false;
            }
            else if(_touchStarted)
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
            if(Mathf.Abs(_swipeDirection.x) > MinSwipeDistance || Mathf.Abs(_swipeDirection.y) > MinSwipeDistance)
                return _swipeDirection;
            else
            return Vector3.zero;
#endif
        }
        else
            return Vector3.zero;
    }

    public void EnableFullScreenTouch()
    {
        _disableLeftScreen = false;
    }

    public void DisableLeftScreenInput()
    {
        _disableLeftScreen = true;
    }
}