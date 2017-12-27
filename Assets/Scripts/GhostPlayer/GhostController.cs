using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class GhostController : MonoBehaviour 
{
    public GameObject GhostPlayerSprite;
    public GameObject RopeOrigin;
    public GameObject Foot;
    public GameObject Body;
    public GameObject GrappleArm;
    public GameObject WallHookSprite;

    public Material GhostHook;
    public Material GhostHookArm;
    public Material GhostPlayerBody;
    public Material GhostPlayerFoot;
	[HideInInspector]
	public LineRenderer RopeLineRenderer;

    private Renderer[] _renderers;
    private Color[] _colors = new Color[] {Color.black, Color.blue, Color.cyan, Color.gray, Color.green, Color.magenta, Color.red, Color.white, Color.yellow};

	void Awake()
	{
        Color primary = _colors[Random.Range(0, 8)];
        Color secondary = _colors[Random.Range(0, 8)];
        Color highlight = _colors[Random.Range(0, 8)];
        Color hookLine = _colors[Random.Range(0, 8)];

        RopeLineRenderer = WallHookSprite.GetComponent<LineRenderer>();
        RopeLineRenderer.material.color = hookLine;
        RopeOrigin.GetComponent<Renderer>().material.color = secondary;
        Foot.GetComponent<Renderer>().material.color = secondary;
        Body.GetComponent<Renderer>().material.color = primary;
        GrappleArm.GetComponent<Renderer>().material.color = highlight;
        WallHookSprite.GetComponent<Renderer>().material.color = highlight;

        _renderers = gameObject.GetComponentsInChildren<Renderer>();

        FadeOut(0.0f);
    }

    public void FadeOut(float time)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
			Color endColor = new Color(_renderers[i].material.color.r, _renderers[i].material.color.g, _renderers[i].material.color.b, 0.0f);
			_renderers[i].material.DOColor(endColor, time);
        }
    }

    public void FadeIn(float time)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
			Color endColor = new Color(_renderers[i].material.color.r, _renderers[i].material.color.g, _renderers[i].material.color.b, 1.0f);
			_renderers[i].material.DOColor(endColor, time);
        }
    }

	void OnTriggerEnter(Collider other) 
	{
		if(other.name == "Player")
        {
            StopAllCoroutines();
            FadeOut(0.3f);
        }	
    }

	void OnTriggerExit(Collider other) 
	{
		if(other.name == "Player")
			FadeIn(0.3f);
    }
}
