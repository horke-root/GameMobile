using System.Collections.Generic;
using UnityEngine;

public class StickersManager : MonoBehaviour
{
    public StickersCatalogSO stickersCatalog;
    public List<GameObject> stickerObjects = new List<GameObject>();
    public GameObject StickerPrefab;
    public Transform stickersParental;

    [HideInInspector] public bool isFleaMarketPreview = false;

    void Start()
    {
    }

    public void LoadStickers()
    {
    }

    public void Save()
    {
    }

    public void ClearStickers()
    {
    }

    public void RemoveAllStickers()
    {
    }

    public void EnableEditing()
    {
    }

    public void DisableEditing()
    {
    }
}
