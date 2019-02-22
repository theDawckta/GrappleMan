using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class LevelSectionController : MonoBehaviour
{
    public SectionType Section;
    public List<SectionType> PossibleNextSections = new List<SectionType>();
    public float OffsetX = 0.0f;
    public float OffsetY = 0.0f;
}