using Mirror;
using UnityEngine;

public class PlayerStatsNetwork : NetworkBehaviour
{
    public static PlayerStatsNetwork Instance;

    private void Awake()
    {
        Instance = this;
    }

    [TargetRpc]
    public void TargetRecordCardPlayed(NetworkConnection target)
    {
        PlayerStatsManager.Instance.AddCardPlayed();
    }

    [TargetRpc]
    public void TargetRecordBackupPlayed(NetworkConnection target)
    {
        PlayerStatsManager.Instance.AddBackupPlayed();
    }

    [TargetRpc]
    public void TargetRecordCycloneIncremented(NetworkConnection target, int amount)
    {
        PlayerStatsManager.Instance.AddCycloneIncremented(amount);
    }

    [TargetRpc]
    public void TargetRecordCycloneDecremented(NetworkConnection target, int amount)
    {
        PlayerStatsManager.Instance.AddCycloneDecremented(amount);
    }

    [TargetRpc]
    public void TargetRecordCycloneWin(NetworkConnection target)
    {
        PlayerStatsManager.Instance.AddCycloneWin();
    }

    /// <summary>
    /// CHESS FUNCTIONS
    /// </summary>

    [TargetRpc]
    public void TargetAddPieceCaptured(NetworkConnection target)
    {
        PlayerStatsManager.Instance.AddChessPieceCaptured();
    }

    [TargetRpc]
    public void TargetAddPawnPromotion(NetworkConnection target)
    {
        PlayerStatsManager.Instance.AddChessPawnPromotion();
    }

    [TargetRpc]
    public void TargetAddStalemate(NetworkConnection target)
    {
        PlayerStatsManager.Instance.AddChessStalemate();
    }

    [TargetRpc]
    public void TargetAddCheckmate(NetworkConnection target)
    {
        PlayerStatsManager.Instance.AddChessCheckmate();
    }

    [TargetRpc]
    public void TargetAddChessWin(NetworkConnection target)
    {
        PlayerStatsManager.Instance.AddChessWin();
    }
}
