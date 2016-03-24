using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour {

	public bool Mobile;
    [HideInInspector]
    public bool InputActive = false;
    public float gunButtonDownTime = 0.3f;
    private bool firing = false;

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

    public bool RopeClimbPressed()
	{
        if (InputActive)
        {
            if (Mobile)
            {
                Debug.Log("implement RopeMove in PlayerInput for mobile");
                return false;
            }
            else if (Input.GetKey("w"))
                return true;
            else
                return false;
        }
        else
            return false;
	}
    public bool RopeClimbReleased()
    {
        if (InputActive)
        {
            if (Mobile)
            {
                Debug.Log("implement RopeMove in PlayerInput for mobile");
                return false;
            }
            else if (Input.GetKeyUp("w"))
                return true;
            else
                return false;
        }
        else
            return false;
    }
	
	public bool JumpPressed()
	{
        if (InputActive)
        {
		    if (Mobile) 
		    {
			    Debug.Log("implement JumpPressed in PlayerInput for mobile");
			    return false;
		    } 
		    else
			    return Input.GetButtonDown ("Jump") ? true:false;
        }
        else
            return false;
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
			    return Input.GetKey ("s") ? true:false;
        }
        else
            return false;
	}
    public bool RopeReleaseReleased()
    {
        if (InputActive)
        {
            if (Mobile)
            {
                Debug.Log("implement HookReleaseReleased in PlayerInput for mobile");
                return false;
            }
            else
                return Input.GetKeyUp("e") ? true : false;
        }
        else
            return false;
    }

    public bool GunPressed()
    {
        if (InputActive)
        {
            if (Mobile)
                return (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began);
            else if (Input.GetButtonDown("Fire1"))
            {
                Debug.Log("StartCoroutine");
                StartCoroutine("ActivateGunTimer");
                return false;
            }
            else if (firing && Input.GetButton("Fire1"))
            {
                Debug.Log("firing");
                return true;
            }
            else if (Input.GetButtonUp("Fire1"))
            {
                StopCoroutine("ActivateGunTimer");
                firing = false;
                return false;
            }
            else
                return false;
        }
        else
            return false;
    }


	public bool RopePressed()
	{
        if (InputActive)
        {
		    if (Mobile) 
			    return (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began);
		    else 
			    return (!firing && Input.GetButtonUp("Fire1"));
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
                return Input.GetButtonDown("Fire2");
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

    IEnumerator ActivateGunTimer()
    {
        float timePassed = 0.0f;
        while(timePassed < gunButtonDownTime)
        {
            timePassed = timePassed + Time.deltaTime;
            yield return null;
        }
        Debug.Log("start firing");
        firing = true;
    }
}
