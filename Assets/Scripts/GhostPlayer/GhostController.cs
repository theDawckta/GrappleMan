using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour 
{
	public GameObject GhostPlayerSprite;
	public GameObject RopeOrigin;
	public GameObject WallHookSprite;
	[HideInInspector]
	public LineRenderer RopeLineRenderer;

	void Awake()
	{
		RopeLineRenderer = WallHookSprite.GetComponent<LineRenderer>();
	}
}
