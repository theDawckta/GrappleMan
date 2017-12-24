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

	void Awake()
	{
		RopeLineRenderer = WallHookSprite.GetComponent<LineRenderer>();
        _renderers.AddRange(GhostPlayerSprite.GetComponentsInChildren<Renderer>().ToList());
        _renderers.AddRange(RopeOrigin.GetComponentsInChildren<Renderer>().ToList());
        _renderers.AddRange(WallHookSprite.GetComponentsInChildren<Renderer>().ToList());

        for (int i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }

        FadeOut(0.0f);
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
            _renderers[i].material.DOFade(0.0f, time);
        }
    }

    public void FadeIn(float time)
    {
        for (int i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].material.DOFade(1.0f, time);
        }
    }
}
