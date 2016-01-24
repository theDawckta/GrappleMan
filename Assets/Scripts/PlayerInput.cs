using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour {

	public bool Mobile;

	public float Move()
	{
		if (Mobile) 
		{
			Debug.Log("implement Move in PlayerInput for mobile");
			return 0;
		} 
		else 
			return Input.GetAxis ("Horizontal");
	}

    public bool RopeClimbPressed()
	{
		if (Mobile) 
		{
			Debug.Log("implement RopeMove in PlayerInput for mobile");
			return false;
		} 
		else if (Input.GetKeyDown("w"))
                return true;
        else
            return false;
	}
	
	public bool JumpPressed()
	{
		if (Mobile) 
		{
			Debug.Log("implement JumpPressed in PlayerInput for mobile");
			return false;
		} 
		else
			return Input.GetButtonDown ("Jump") ? true:false;
	}

	public bool HookReleasePressed()
	{
		if (Mobile) 
		{
			Debug.Log("implement HookReleasePressed in PlayerInput for mobile");
			return false;
		} 
		else
			return Input.GetKeyDown ("e") ? true:false;
	}

	public bool HookPressed()
	{
		if (Mobile) 
			return (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began);
		else 
			return Input.GetButtonDown("Fire1");
	}

	public Vector3 GetPlayerTouchPosition()
	{
		if (Mobile) 
			return Input.GetTouch(0).position;
		else 
			return Input.mousePosition;
	}
}
