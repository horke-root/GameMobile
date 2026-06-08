using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SlotField
{
    public string slotId;
    public SpriteRenderer sprite;
}

public class BikeVisual : MonoBehaviour
{   
    public List<SlotField> slotF = new List<SlotField>();
    public SpriteRenderer DirtMask;
    [Range(0f, 1f)] public float dirtness = 0f;

    void Awake()
    {
    }

    public void UpdateDirtness()
    {
        if (DirtMask != null)
        {
            Color oC = DirtMask.color;
            oC.a = dirtness;
            DirtMask.color = oC;
        }
    }
}
