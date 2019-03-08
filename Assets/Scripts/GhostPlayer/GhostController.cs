using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GhostController : MonoBehaviour 
{
    public GameObject GhostPlayerSprite;
    public GameObject RopeOrigin;
    //public GameObject Foot;
    //public GameObject Body;
    //public GameObject GrappleArm;
    public GameObject WallHookSprite;
    public CanvasGroup UserNameCanvasGroup;
    public Text Username;

    //public Material GhostHook;
    //public Material GhostHookArm;
    //public Material GhostPlayerBody;
    //public Material GhostPlayerFoot;
	[HideInInspector]
	public LineRenderer RopeLineRenderer;

    private Renderer[] _renderers;
    private Color[] _colors = new Color[] {Color.black, Color.blue, Color.cyan, Color.gray, Color.green, Color.magenta, Color.red, Color.white, Color.yellow};

    void Awake()
    {
        Color primary = _colors[Random.Range(0, 8)];
        Color secondary = _colors[Random.Range(0, 8)];
        Color highlight = _colors[Random.Range(0, 8)];

        UserNameCanvasGroup.alpha = 0.0f;
        RopeLineRenderer = WallHookSprite.GetComponent<LineRenderer>();
        RopeLineRenderer.material.color = highlight;
        //RopeOrigin.GetComponent<Renderer>().material.color = secondary;
        //Foot.GetComponent<Renderer>().material.color = secondary;
        //Body.GetComponent<Renderer>().material.color = primary;
        //GrappleArm.GetComponent<Renderer>().material.color = highlight;
        WallHookSprite.GetComponent<Renderer>().material.color = highlight;

        _renderers = gameObject.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < _renderers.Length; i++)
        {
            Color endColor = new Color(_renderers[i].material.color.r, _renderers[i].material.color.g, _renderers[i].material.color.b, 0.0f);
            _renderers[i].material.color = endColor;
        }
    }

    public void FadeOut(float time, bool kill = false)
    {
        UserNameCanvasGroup.DOFade(0.0f, time);

        for (int i = 0; i < _renderers.Length; i++)
        {
			Color endColor = new Color(_renderers[i].material.color.r, _renderers[i].material.color.g, _renderers[i].material.color.b, 0.0f);
			_renderers[i].material.DOColor(endColor, time);


            if (i < _renderers.Length - 1)
                _renderers[i].material.DOColor(endColor, time);
            else
            {
                _renderers[i].material.DOColor(endColor, time).OnComplete(() => {
                    if(kill)
                        Destroy(gameObject);
                });
            }
        }
    }

    public void FadeIn(float time, float delay)
    {
        gameObject.SetActive(true);
        UserNameCanvasGroup.DOFade(1.0f, time).SetDelay(delay);

        for (int i = 0; i < _renderers.Length; i++)
        {
			Color endColor = new Color(_renderers[i].material.color.r, _renderers[i].material.color.g, _renderers[i].material.color.b, 1.0f);
            _renderers[i].material.DOColor(endColor, time).SetDelay(delay);
        }
    }

	void OnTriggerEnter(Collider other) 
	{
		// place holder
    }

	void OnTriggerExit(Collider other) 
	{
		// place holder
    }
}
