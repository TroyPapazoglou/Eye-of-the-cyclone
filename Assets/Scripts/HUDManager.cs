using Mirror;
using PrimeTween;
using Steamworks;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{     
    public TextMeshProUGUI CycloneCountText;

    private Vector2 originalAnchoredPosition;
    public static HUDManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        originalAnchoredPosition = CycloneCountText.rectTransform.anchoredPosition;
    }

    
   
}
