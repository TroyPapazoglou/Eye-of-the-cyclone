using UnityEngine;

public class PawnPromotionUI : MonoBehaviour
{
    public static PawnPromotionUI Instance { get; private set; }

    [SerializeField] private GameObject pawnPromotionUI;
    [SerializeField] private RectTransform pawnPromotionUICanvas;

    private ChessPieceInfo pendingPiece;

    private void Awake()
    {
        Instance = this;       
    }

    public void Show(ChessPieceInfo piece, ChessColour colour)
    {      
        pendingPiece = piece;
        pawnPromotionUI.SetActive(true);
    }

   
    public void SelectQueen()
    {
        SelectPromotion(ChessPieceType.Queen);
    }

    public void SelectRook()
    {
        SelectPromotion(ChessPieceType.Rook);
    }

    public void SelectBishop()
    {
        SelectPromotion(ChessPieceType.Bishop);
    }

    public void SelectKnight()
    {
        SelectPromotion(ChessPieceType.Knight);
    }

    private void SelectPromotion(ChessPieceType type)
    {
        pawnPromotionUI.SetActive(false);

        PlayerNetworkObject.LocalPlayer.Chess.CmdPromotePiece(pendingPiece,type);
    }
}
