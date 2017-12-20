using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerInput : MonoBehaviour
{
    public bool InputActive = false;
    public float MovementRampUpTime = 0.3f;
    public Slider XAxis;
    public UIController UI;

    private bool _leftTouchStarted = false;
    private Vector2 _leftStart = new Vector2();
    private Vector2 _leftSwipe = new Vector2();
    private bool _leftTouchDone = false;
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
			if (touch.position.x < Screen.width / 2)
			{	
				if (touch.phase == TouchPhase.Began)
                {
                    _leftTouchStarted = true;
                    _leftStart = touch.position;
                }
                if (touch.phase == TouchPhase.Moved)
                    _leftSwipe = touch.position - _leftStart;
                if (touch.phase == TouchPhase.Ended)
                {
                    _leftTouchDone = true;
                    _leftTouchStarted = false;
                    _leftSwipe = Vector2.zero;
                }
			}
            else
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
                        return (Input.GetKey(KeyCode.Q));
            #elif (UNITY_IOS || UNITY_ANDROID)
            if (Mathf.Abs(_leftSwipe.y) > _minDPadDistance)
            {
                if (_leftSwipe.y > 0.0f)
                {
                    //Debug.Log("SWIPE LENGTH:" + _leftSwipe);
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
#endif
        }
        else
            return false;
    }

	public bool RopeReleasePressed()
	{
        if (InputActive)
        {
            #if (UNITY_STANDALONE || UNITY_EDITOR)
                        return Input.GetKey(KeyCode.E) ? true : false;
            #elif (UNITY_IOS || UNITY_ANDROID)
            if (Mathf.Abs(_leftSwipe.y) > _minDPadDistance)
            {
                if (_leftSwipe.y < 0.0f)
                {
                    return true;
                }
                else
                    return false;
            }
            else
                return false;   
#endif
        }
        else
            return false;
	}

    public bool RopeReleaseUp()
    {
        if (InputActive)
        {
#if (UNITY_IOS || UNITY_ANDROID)
            if (_leftTouchDone)
            {
                _leftTouchDone = false;
                return true;
            }
            else
                return false;
#endif
            return false;
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

    public float Move()
    {
        if (InputActive)
        {
#if (UNITY_STANDALONE || UNITY_EDITOR)
            {
                return Input.GetAxis("Horizontal");
            }

#elif (UNITY_IOS || UNITY_ANDROID)
            if (Mathf.Abs(_leftSwipe.x) > _minDPadDistance)
            {   
                if (_leftSwipe.x > 0.0f)
                    return 1.0f;
                else if (_leftSwipe.x < 0.0f)
                    return -1.0f;
                else
                    return 0.0f;
            }
            else
                return 0.0f;
#endif
        }
        else
            return 0.0f;
    }
}
