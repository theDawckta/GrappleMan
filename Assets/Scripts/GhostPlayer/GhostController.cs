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
    public Text Username;
    public List<ParticleSystem> ExplosionEffects = new List<ParticleSystem>();
    
	[HideInInspector]
	public LineRenderer RopeLineRenderer;
    
    private Renderer[] _renderers;
    private Rigidbody _playerPiece1Rigidbody;
    private Rigidbody _playerPiece2Rigidbody;

    void Awake()
    {
        RopeLineRenderer = WallHookSprite.GetComponent<LineRenderer>();
        _playerPiece1Rigidbody = PlayerPiece1.GetComponent<Rigidbody>();
        _playerPiece2Rigidbody = PlayerPiece2.GetComponent<Rigidbody>();

        _renderers = gameObject.GetComponentsInChildren<Renderer>(true);
    }

    public void Caught()
    {
        RopeLineRenderer.enabled = false;
        GrappleArmEndPS.Stop();
        WallHookSpritePS.Stop();
        ElectrodeFrontPS.Stop();
        ElectrodeBackPS.Stop();
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

	void OnTriggerEnter(Collider other) 
	{
		// place holder
    }

	void OnTriggerExit(Collider other) 
	{
		// place holder
    }
}
