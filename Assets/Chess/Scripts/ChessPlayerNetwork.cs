using Mirror;
using UnityEngine;

public class ChessPlayerNetwork : NetworkBehaviour
{
    private PlayerNetworkObject player;

    [SyncVar]
    public ChessColour chessColour;

    private void Awake()
    {
        player = GetComponent<PlayerNetworkObject>();
    }

    public override void OnStartLocalPlayer()
    {
        ChessAnimator.OnIntroComplete += CmdFinishedBoardAnimation;
    }

    public override void OnStopClient()
    {
        ChessAnimator.OnIntroComplete -= CmdFinishedBoardAnimation;
    }

    // ========================================================= 
    // TARGET RPCs //
    // =========================================================

    [TargetRpc]
    public void TargetShowPromotionUI(ChessPieceInfo pieceInfo, ChessColour colour)
    {
        PawnPromotionUI.Instance.Show(pieceInfo, colour);
    }

    [Command]
    public void CmdFinishedBoardAnimation()
    {
        ChessGameManager.Instance.PlayerFinishedBoardAnimation(connectionToClient);
    }

    [Command]
    public void CmdMovePiece(int startX, int startZ, int targetX, int targetZ)
    {
        ChessGameManager.Instance.TryMovePiece(player, startX, startZ, targetX, targetZ);
    }

    [Command]
    public void CmdPromotePiece(ChessPieceInfo pieceInfo, ChessPieceType newType)
    {
        ChessGameManager.Instance.TryPromotePiece(player, pieceInfo, newType);
    }
}
