using UnityEngine;


public class CycloneScreenUIManager : MonoBehaviour
{

    public static CycloneScreenUIManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private CycloneUserUI GetPlayerUI(uint netId)
    {
        UserUI genericUI =  FindGenericPlayerUI(netId);

        if (genericUI == null)
            return null;

        return genericUI.GetComponent<CycloneUserUI>();
    }

    private UserUI FindGenericPlayerUI(uint netId)
    {
        return PCScreenUIManager.Instance.GetPlayerUI(netId);       
    }

    public void PlayerDrewCards(uint netId, CardInfo[] cards)
    {
        CycloneUserUI ui =  GetPlayerUI(netId);

        if (ui != null)
        {
            ui.PlayerDrewCards(cards);
        }
    }

    public void PlayerPlacedCard(uint netId, CardInfo card)
    {
        CycloneUserUI ui = GetPlayerUI(netId);

        if (ui != null)
        {
            ui.PlayerPlayedCard(card);
        }
    } 

    public void PlayerDiscardCard(uint netId, CardInfo card)
    {
        CycloneUserUI ui = GetPlayerUI(netId);

        if (ui != null)
        {
            ui.PlayerDiscardCard(card);
        }
    }

    public void PlayerGainedBackup(uint netId, CardInfo backupCard)
    {
        CycloneUserUI ui = GetPlayerUI(netId);

        if (ui != null)
        {
            ui.PlayerGainedBackup(backupCard);
        }
    }

    public void PlayerPlacedBackup(uint netId, CardInfo backupCard)
    {
        CycloneUserUI ui = GetPlayerUI(netId);

        if (ui != null)
        {
            ui.PlayerPlayedBackup(backupCard);
        }
    }

    public void PlayerDiscardedBackup(uint netId, CardInfo card)
    {
        CycloneUserUI ui = GetPlayerUI(netId);

        if (ui != null)
        {
            ui.PlayerDiscardedBackup(card);
        }
    }

}
