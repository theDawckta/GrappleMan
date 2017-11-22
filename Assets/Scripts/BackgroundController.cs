﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour 
{
    public List<GameObject> Backgrounds = new List<GameObject>();
    public List<float> BackgroundSpeeds = new List<float>();
    public int Scale;

    private Material[] _backgroundMaterials;

    void Start () 
    {
        float height = 2f * Camera.main.orthographicSize;
        float width = height * Camera.main.aspect;
        float size = (height > width) ? height * 2 : width * 2;

        _backgroundMaterials = new Material[Backgrounds.Count];
        for (int i = 0; i < Backgrounds.Count; i++)
        {
            Backgrounds[i].transform.localScale = new Vector3(size * Scale, size * Scale, 0.0f);
            _backgroundMaterials[i] = Backgrounds[i].GetComponent<Renderer>().material;
        }
    }
    
    void Update () 
    {
        for (int i = 0; i < _backgroundMaterials.Length; i++ )
        {
            Vector2 offset = new Vector2(transform.position.x / Backgrounds[i].transform.localScale.x, transform.position.y / Backgrounds[i].transform.localScale.y);
            _backgroundMaterials[i].mainTextureOffset = new Vector2((offset.x % 1) * BackgroundSpeeds[i], (offset.y % 1) * BackgroundSpeeds[i]);
        }   
    }
}