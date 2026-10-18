using UnityEngine;
using UnityEngine.UI;

public enum BannerCardRole
{
    None,
    Hand,
    Backup
}

public class BannerCardVisual : MonoBehaviour
{
    public BannerCardRole Role { get; private set; }

    public CardInfo? CurrentCard { get; private set; }

    public void SetCard(CardInfo card)
    {
        CurrentCard = card;
    }

    public void ClearCard()
    {
        CurrentCard = null;
        Role = BannerCardRole.None;
    }

    public void SetRole(BannerCardRole role)
    {
        Role = role;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
