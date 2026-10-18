using Mirror;
using UnityEngine;

public enum ChessPieceType
{
    King,
    Queen,
    Rook,
    Bishop,
    Knight,
    Pawn
}

public struct ChessPieceInfo
{
    public int boardX;
    public int boardZ;

    public ChessPieceInfo(int boardX, int boardZ)
    {
        this.boardX = boardX;
        this.boardZ = boardZ;
    }
}

public class ChessPiece : MonoBehaviour
{
    [Header("Piece")]
    [SerializeField] private ChessPieceType pieceType;
    [SerializeField] private ChessColour colour;   


    [Header("Board Position")]
    [SerializeField] private int boardX;
    [SerializeField] private int boardZ;

    private ChessPieceType originalPieceType;
    private Mesh originalMesh;

    private ChessSquare currentSquare;
    public ChessPieceType PieceType => pieceType;
    public ChessColour Colour => colour;

    public ChessPieceInfo pieceInfo;

    public int BoardX => boardX;
    public int BoardZ => boardZ;

    public ChessSquare CurrentSquare => currentSquare;
        
    private bool isOut;

    public bool IsOut => isOut;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private PawnVisuals pawnVisuals;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        pawnVisuals = GetComponent<PawnVisuals>();

        originalPieceType = pieceType;
        originalMesh = meshFilter.mesh;
    }

    public bool IsPieceOut()
    {
        return isOut;
    }

    public void SetIsOut(bool value)
    {
        isOut = value;
    }

    public void SetBoardPosition(int x, int z)
    {
        boardX = x;
        boardZ = z;
    }  

    public void SetCurrentSquare(ChessSquare square)
    {
        currentSquare = square;
    }

    public void SetPieceType(ChessPieceType newType)
    {
        pieceType = newType;
    }

    public void SetPieceVisual(ChessPiece piece, ChessPieceType type)
    {
        if (piece == null)
            return;

        piece.SetPieceType(type);

        piece.SetVisual(type);
    }

    public void SetVisual(ChessPieceType type)
    {
        if (pawnVisuals == null)
            return;

        switch (type)
        {
            case ChessPieceType.Bishop:
                meshFilter.mesh = pawnVisuals.BishopMesh;
                meshCollider.sharedMesh = pawnVisuals.BishopMesh;
                break;
            case ChessPieceType.Rook:
                meshFilter.mesh = pawnVisuals.RookMesh;
                meshCollider.sharedMesh = pawnVisuals.RookMesh;
                break;
            case ChessPieceType.Knight:
                meshFilter.mesh = pawnVisuals.KnightMesh;
                meshCollider.sharedMesh = pawnVisuals.KnightMesh;
                break;
            case ChessPieceType.Queen:
                meshFilter.mesh = pawnVisuals.QueenMesh;
                meshCollider.sharedMesh = pawnVisuals.QueenMesh;
                break;



        }
    }

    public void ResetPiece()
    {
        pieceType = originalPieceType;

        if (meshFilter == null || meshCollider == null)
            return;

        meshFilter.mesh = originalMesh;
        meshCollider.sharedMesh = originalMesh;
    }

    public ChessPieceInfo GetPieceInfo()
    {
        return pieceInfo;
    }
}
