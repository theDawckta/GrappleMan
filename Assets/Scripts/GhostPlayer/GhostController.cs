using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class GhostController : MonoBehaviour 
{
	public GameObject GhostPlayerSprite;
	public GameObject RopeOrigin;
	public GameObject WallHookSprite;
	[HideInInspector]
	public LineRenderer RopeLineRenderer;

    private List<Renderer> _renderers = new List<Renderer>();
    private Color[] _colors = new Color[] {Color.black, Color.blue, Color.cyan, Color.gray, Color.green, Color.magenta, Color.red, Color.white, Color.yellow};

	void Awake()
	{
		RopeLineRenderer = WallHookSprite.GetComponent<LineRenderer>();
        _renderers.AddRange(GhostPlayerSprite.GetComponentsInChildren<Renderer>().ToList());
        _renderers.AddRange(RopeOrigin.GetComponentsInChildren<Renderer>().ToList());
        _renderers.AddRange(WallHookSprite.GetComponentsInChildren<Renderer>().ToList());

        for (int i = 0; i < _renderers.Count; i++)
        {
			_renderers[i].material = new Material (Shader.Find("Legacy Shaders/Transparent/Diffuse"));
			_renderers[i].material.SetFloat("_Mode", 2);
			_renderers[i].material.color = _colors[Random.Range(0, 8)];
            _renderers[i].material.color = new Color(_renderers[i].material.color.r, _renderers[i].material.color.g, _renderers[i].material.color.b, 0.0f); 
        }
    }

    private void Start()
    {
        StartCoroutine(StartFadeIn());
    }

    IEnumerator StartFadeIn()
    {
        yield return new WaitForSeconds(0.5f);
        FadeIn(0.5f);
    }

    public void FadeOut(float time)
    {
        for (int i = 0; i < _renderers.Count; i++)
        {
			Color endColor = new Color(_renderers[i].material.color.r, _renderers[i].material.color.g, _renderers[i].material.color.b, 0.0f);
			_renderers[i].material.DOColor(endColor, time);
        }
    }

    public void FadeIn(float time)
    {
        for (int i = 0; i < _renderers.Count; i++)
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
