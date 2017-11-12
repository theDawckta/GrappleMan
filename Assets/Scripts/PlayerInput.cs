using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour {

	public bool Mobile;
    public bool InputActive = false;

	public float Move()
	{
        if (InputActive)
        {
            if (Mobile)
            {
                Debug.Log("implement Move in PlayerInput for mobile");
                return 0;
            }
            else
                return Input.GetAxis("Horizontal");
        }
        else
            return 0;
	}

	public bool RopeReleasePressed()
	{
        if (InputActive)
        {
		    if (Mobile) 
		    {
			    Debug.Log("implement HookReleasePressed in PlayerInput for mobile");
			    return false;
		    } 
		    else
			    return Input.GetButton("Fire2") ? true:false;
        }
        else
            return false;
	}

	public bool HookPressed()
	{
        if (InputActive)
        {
		    if (Mobile) 
			    return (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began);
		    else 
			    return (Input.GetButton("Fire1"));
        }
        else
            return false;
	}

    public bool HookReleased()
    {
        if (InputActive)
        {
            if (Mobile)
                return (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended);
            else
                return (Input.GetButtonUp("Fire1"));
        }
        else
            return false;
    }

    public Vector3 GetPlayerTouchPosition()
	{
        if (InputActive)
        {
		    if (Mobile) 
			    return Input.GetTouch(0).position;
		    else 
			    return Input.mousePosition;
        }
        else
            return new Vector3();
	}
}
