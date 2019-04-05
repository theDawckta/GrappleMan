using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GhostController : MonoBehaviour 
{
    public GameObject GhostPlayerSprite;
    public GameObject PlayerPiece1;
    public GameObject PlayerPiece2;
    public Transform GrabPointTop;
    public Transform GrabPointBottom;
    public GameObject GrappleArmEnd;
    public GameObject RopeOrigin;
    public GameObject LowerLightningPlanes;
    public GameObject UpperLightningPlanes;
    public ParticleSystem GrappleArmEndPS;
    public ParticleSystem WallHookSpritePS;
    public ParticleSystem ElectrodeFrontPS;
    public ParticleSystem ElectrodeBackPS;
    public GameObject WallHookSprite;
    public CanvasGroup UserNameCanvasGroup;
    public Text Username;
    public List<ParticleSystem> ExplosionEffects = new List<ParticleSystem>();
    
	[HideInInspector]
	public LineRenderer RopeLineRenderer;
    
    private Renderer[] _renderers;
    private Rigidbody _playerPiece1Rigidbody;
    private Rigidbody _playerPiece2Rigidbody;
    //private Color[] _colors = new Color[] {Color.black, Color.blue, Color.cyan, Color.gray, Color.green, Color.magenta, Color.red, Color.white, Color.yellow};

    void Awake()
    {
        //Color primary = _colors[Random.Range(0, 8)];
        //Color secondary = _colors[Random.Range(0, 8)];
        //Color highlight = _colors[Random.Range(0, 8)];
        UserNameCanvasGroup.alpha = 0.0f;
        RopeLineRenderer = WallHookSprite.GetComponent<LineRenderer>();
        _playerPiece1Rigidbody = PlayerPiece1.GetComponent<Rigidbody>();
        _playerPiece2Rigidbody = PlayerPiece2.GetComponent<Rigidbody>();
        //RopeLineRenderer.material.color = highlight;
        //WallHookSprite.GetComponent<Renderer>().material.color = highlight;

        _renderers = gameObject.GetComponentsInChildren<Renderer>(true);

        //for (int i = 0; i < _renderers.Length; i++)
        //{
        //    Color endColor = new Color(_renderers[i].material.color.r, _renderers[i].material.color.g, _renderers[i].material.color.b, 0.0f);
        //    _renderers[i].material.color = endColor;
        //}
    }

    public void Caught()
    {
        GhostPlayerSprite.SetActive(false);
        PlayerPiece1.SetActive(true);
        PlayerPiece2.SetActive(true);
        _playerPiece1Rigidbody.AddExplosionForce(8.0f, GhostPlayerSprite.transform.position, 5.0f, 0.0f, ForceMode.Impulse);
        _playerPiece2Rigidbody.AddExplosionForce(8.0f, GhostPlayerSprite.transform.position, 5.0f, 0.0f, ForceMode.Impulse);

        for (int i = 0; i < ExplosionEffects.Count; i++)
        {
            ExplosionEffects[i].Play();
        }
    }

    public void FadeOut(float time, bool kill = false)
    {
   //     UserNameCanvasGroup.DOFade(0.0f, time);

   //     for (int i = 0; i < _renderers.Length; i++)
   //     {
			//Color endColor = new Color(_renderers[i].material.color.r, _renderers[i].material.color.g, _renderers[i].material.color.b, 0.0f);
			//_renderers[i].material.DOColor(endColor, time);


   //         if (i < _renderers.Length - 1)
   //             _renderers[i].material.DOColor(endColor, time);
   //         else
   //         {
   //             _renderers[i].material.DOColor(endColor, time).OnComplete(() => {
   //                 if(kill)
   //                     Destroy(gameObject);
   //             });
   //         }
   //     }
    }

    public void FadeIn(float time, float delay)
    {
   //     gameObject.SetActive(true);
   //     UserNameCanvasGroup.DOFade(1.0f, time).SetDelay(delay);

   //     for (int i = 0; i < _renderers.Length; i++)
   //     {
			//Color endColor = new Color(_renderers[i].material.color.r, _renderers[i].material.color.g, _renderers[i].material.color.b, 1.0f);
   //         _renderers[i].material.DOColor(endColor, time).SetDelay(delay);
   //     }
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
