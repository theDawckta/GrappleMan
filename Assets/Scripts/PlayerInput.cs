using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour 
{
    public bool InputActive = false;
	public delegate void OnSwipeLeftEvent(Vector2 direction);
	public event OnSwipeLeftEvent OnSwipeLeft;
	public delegate void OnSwipeRightHeldEvent(Vector2 direction);
	public event OnSwipeRightHeldEvent OnSwipeRightHeld;
	public delegate void OnSwipeRightEndedEvent(Vector2 direction);
	public event OnSwipeRightEndedEvent OnSwipeRightEnded;

	private Vector2 firstPressPos;
	private Vector2 secondPressPos;
	private Vector2 currentSwipe;

	void Update() 
	{
		if (InputActive)
			CheckSwipe();
    }

	public float Move()
	{
        if (InputActive)
           	return Input.GetAxis("Horizontal");
        else
            return 0;
	}

	public bool RopeReleasePressed()
	{
        if (InputActive)
			return Input.GetKey(KeyCode.E) ? true:false;
        else
            return false;
	}

	public bool HookButtonDown()
	{
        if (InputActive)
			return (Input.GetButtonDown("Fire1"));
        else
            return false;
	}

	public bool ClimbButtonUp()
	{
        if (InputActive)
			return (Input.GetKeyUp(KeyCode.Q));
        else
            return false;
	}

	public bool ClimbButtonPressed()
    {
        if (InputActive)
          	return (Input.GetKey(KeyCode.Q));
        else
            return false;
    }

    public Vector3 GetPlayerTouchPosition()
	{
        if (InputActive)
			return Input.mousePosition;
        else
            return new Vector3();
	}

	void CheckSwipe()
	{
		if(Input.touches.Length > 0)
		{
			Debug.Log("TOUCH LENGTH GREATER THAN ZERO");
			Touch t = Input.GetTouch(0);

			if(t.phase == TouchPhase.Moved)
			{
				Debug.Log("TOUCH HAS MOVED");
				firstPressPos = new Vector2(t.position.x,t.position.y);
				if (t.position.x > Screen.width / 2) 
					OnSwipeRightHeld(t.deltaPosition);
			}

			if(t.phase == TouchPhase.Ended)
			{
				secondPressPos = new Vector2(t.position.x,t.position.y);
			           
				currentSwipe = new Vector3(secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);

				if (t.position.x > Screen.width / 2)
				{
					Debug.Log("SWIPE RIGHT ENDED");
					OnSwipeRightEnded(t.deltaPosition);
				}
				if (t.position.x < Screen.width / 2) 
				{
					Debug.Log("SWIPE LEFT");
					OnSwipeLeft(currentSwipe);
				}

				Debug.Log(currentSwipe);
			}
		}		
	}
}
