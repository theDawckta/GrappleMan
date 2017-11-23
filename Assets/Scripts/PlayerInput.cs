using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerInput : MonoBehaviour 
{
    public bool InputActive = false;
    public float MovementRampUpTime = 0.3f;
    public Slider XAxis;

    private bool _leftTouchStarted = false;
	private Vector2 _leftStart = new Vector2();
	private Vector2 _leftSwipe = new Vector2();
	private bool _leftTouchDone = false;
    private float _leftSwipeAxis = 0.0f;
    private bool _rightTouchStarted = false;
	private Vector2 _rightStart = new Vector2();
	private Vector2 _rightSwipe = new Vector2();
	private bool _rightTouchDone = false;
    private float _rightSwipeAxis = 0.0f;
    private float xVelocity = 0.0F;

	void Update() 
	{
        //Debug.Log("RIGHT SWIPE AXIS: " + _rightSwipeAxis);
        //if (Input.GetKeyDown(KeyCode.F))
        //    _rightTouchStarted = true;
        //if (Input.GetKeyUp(KeyCode.F))
        //{
        //    _rightTouchStarted = false;
        //    XAxis.value = 0.0f;
        //    _rightSwipeAxis = 0.0f;
        //}
        
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
                    _leftTouchStarted = false;
                    _leftTouchDone = true;
                    _leftSwipe = touch.position - _leftStart;
                    _leftSwipeAxis = 0.0f;
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
                    _rightTouchStarted = false;
					_rightTouchDone = true;
                    _rightSwipe = touch.position - _rightStart;
                    _rightSwipeAxis = 0.0f;
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
            if (_leftTouchDone)
            {
                _leftTouchDone = false;
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
            if (_rightSwipe.y > 0 && _rightTouchStarted)
                return true;
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
			return Input.GetKey(KeyCode.E) ? true:false;

			#elif (UNITY_IOS || UNITY_ANDROID)
            if (_rightSwipe.y < 0 && _rightTouchStarted)
				return true;
			else
				return false;
			#endif
		}
        else
            return false;
	}

	public bool ClimbButtonUp()
	{
        if (InputActive)
        {
			#if (UNITY_STANDALONE || UNITY_EDITOR)
			return (Input.GetKeyUp(KeyCode.Q));

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
            return _leftSwipe;
			#endif
		}
        else
            return new Vector3();
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
            {
                if (_rightTouchStarted)
                {
                    float targetValue = 0.0f;
                    if (_rightSwipe.x > 0.0f)
                        targetValue = 1.0f;
                    else if (_rightSwipe.x < 0.0f)
                        targetValue = -1.0f;
                    _rightSwipeAxis = Mathf.SmoothDamp(_rightSwipeAxis, targetValue, ref xVelocity, MovementRampUpTime);
                }
                return _rightSwipeAxis;
            }
            #endif
        }
        else
            return 0;
    }
}
