using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TurnLogAction
{
    PlayedCard,
    PlayedBackup,
    DiscardedCard,
    DiscardedBackup
}

public class TurnLogEntry : MonoBehaviour
{
    [SerializeField] private Image avatarImage;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text actionText;
    [SerializeField] private Image cardImage;

    public void Setup(string playerName, CSteamID steamId, TurnLogAction action, CardInfo card)
    {
        //playerNameText.text = playerName;
        actionText.text = GetActionText(action);

        cardImage.sprite = CardVisualLoader.LoadSprite(card);

        SteamAvatarCache.Instance.RequestAvatar(
            steamId,
            sprite =>
            {
                if (avatarImage != null)
                    avatarImage.sprite = sprite;
            });
    }

    private string GetActionText(TurnLogAction action)
    {
        return action switch
        {
            TurnLogAction.PlayedCard => "Played a card",
            TurnLogAction.PlayedBackup => "Played a backup",
            TurnLogAction.DiscardedCard => "Discarded a card",
            TurnLogAction.DiscardedBackup => "Discarded a backup",
            _ => ""
        };
    }  
}
