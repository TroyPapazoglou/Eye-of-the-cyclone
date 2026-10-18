using PrimeTween;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class ChessAnimator : MonoBehaviour
{
    [Header("Pivot")]
    [SerializeField] private GameObject chessboardPivot;

    [Header("Board")]
    [SerializeField] private Transform chessBoardTransform;
    [SerializeField] private ChessBoard chessBoard;


    [Header("Pieces")]
    [SerializeField] private List<ChessPiece> pieces = new();

    [Header("Board Intro")]
    [SerializeField] private float boardAnimationDuration = 1f;

    [Header("Piece Intro")] 
    [SerializeField] private float pieceFallDuration = 0.3f;
    [SerializeField] private float pieceDelay = 0.08f;

    [Header("Movement")]
    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private Ease moveEase = Ease.InOutQuad;

    [Header("Captured Piece Positions")]
    [SerializeField] private List<Transform> blackOutPositions = new();
    [SerializeField] private List<Transform> whiteOutPositions = new();

    private readonly ChessPiece[] blackOutPieces = new ChessPiece[16];
    private readonly ChessPiece[] whiteOutPieces = new ChessPiece[16];

    private readonly List<ChessSquare> moveIndicators = new();

    private Dictionary<ChessPieceInfo, ChessPiece> pieceLookup = new();

    private Sequence boardSequence;
    private Sequence pieceSequence;

    private const float BoardSize = 0.155162f;
    private const int BoardDimension = 8;
    private const float PieceY = 0.01f;

    public static event Action OnIntroComplete;

    public static ChessAnimator Instance;

    private void Awake()
    {
        Instance = this;

        foreach (ChessPiece piece in pieces)
        {
            if (piece == null)
                continue;

            ChessPieceInfo info =
                new ChessPieceInfo(
                    piece.BoardX,
                    piece.BoardZ
                );

            piece.pieceInfo = info;

            pieceLookup[info] = piece;
        }
    }   

    private void ShowChessBoard()
    {
        chessboardPivot.SetActive(true);
    }

    public void HideChessBoard()
    {
        chessboardPivot.SetActive(false);
    }

    public void PlayIntroAnimation(bool flipped)
    {
        if(flipped)
        {
            chessboardPivot.transform.rotation = new Quaternion(0, -180, 0, 1);
        }      

        ShowChessBoard();

        boardSequence.Stop();
        pieceSequence.Stop();

        chessBoardTransform.localScale = Vector3.zero;
        chessBoardTransform.localRotation = Quaternion.identity;

        SetupPiecePositions();

        PlayBoardIntro();      
    }

    private void PlayBoardIntro()
    {
        boardSequence = Sequence.Create();

        boardSequence
            .Group(
                Tween.Scale(
                    chessBoardTransform,
                    Vector3.one,
                    boardAnimationDuration,
                    ease: Ease.OutBack
                )
            )
            .Group(
                Tween.LocalRotation(
                    chessBoardTransform,
                    Quaternion.Euler(0f, 360f, 0f),
                    boardAnimationDuration,
                    ease: Ease.OutCubic
                )
            ).OnComplete( PlayPieceIntro );
    }

    private void PlayPieceIntro()
    {
        pieceSequence = Sequence.Create();

        float delay = 0f;

        foreach (ChessPiece piece in pieces)
        {
            pieceSequence.Insert(
                delay,
                Tween.LocalPosition(
                    piece.transform,
                    GetBoardPosition(piece.BoardX, piece.BoardZ),
                    pieceFallDuration,
                    ease: Ease.OutBounce
                )
            );

            delay += pieceDelay;
        }

        pieceSequence.OnComplete(OnIntroAnimationComplete);

    }

    private void OnIntroAnimationComplete()
    {
        OnIntroComplete?.Invoke();
    }

    private void SetupPiecePositions()
    {
        Camera camera = Camera.main;

        foreach (ChessPiece piece in pieces)
        {
            Vector3 target = GetBoardPosition(piece.BoardX, piece.BoardZ);

            Vector3 boardWorldPosition = ChessBoard.Instance.transform.TransformPoint(target);

            Vector3 screenPosition = camera.WorldToScreenPoint(boardWorldPosition);

            screenPosition.y = Screen.height;

            Vector3 topWorldPosition = camera.ScreenToWorldPoint(screenPosition);

            Vector3 topLocalPosition = ChessBoard.Instance.transform.InverseTransformPoint(topWorldPosition);

            piece.transform.localPosition = new Vector3(
                target.x,
                topLocalPosition.y + 0.5f,
                target.z
            );
        }    
}

    public void MovePieceOut(ChessPiece piece)
    {
        if (piece == null)
            return;

        List<Transform> positions;
        ChessPiece[] occupied;

        if (piece.Colour == ChessColour.Black)
        {
            positions = blackOutPositions;
            occupied = blackOutPieces;
        }
        else
        {
            positions = whiteOutPositions;
            occupied = whiteOutPieces;
        }

        for (int i = 0; i < positions.Count && i < occupied.Length; i++)
        {
            if (positions[i] == null)
                continue;

            if (occupied[i] != null)
                continue;

            occupied[i] = piece;

            piece.transform.SetPositionAndRotation(positions[i].position, positions[i].rotation);
            return;
        }
    }

    public Vector3 GetBoardPosition(int x, int z)
    {
        float squareSize =
            BoardSize / BoardDimension;

        float boardMin =
            -BoardSize * 0.5f;

        return new Vector3(
            boardMin +
            squareSize * 0.5f +
            x * squareSize,

            PieceY,

            boardMin +
            squareSize * 0.5f +
            z * squareSize
        );
    }



    public ChessPiece GetPiece(ChessPieceInfo info)
    {
        pieceLookup.TryGetValue(
            info,
            out ChessPiece piece
        );

        return piece;
    }  
    // =========================================================
    // POSSIBLE MOVE INDICATORS
    // =========================================================

    public void ShowPossibleMoves(List<ChessSquare> squares)
    {
        ClearMoveIndicators();

        if (squares == null)
            return;

        foreach (ChessSquare square in squares)
        {
            if (square == null)
                continue;

            ChessPiece piece = ChessGameManager.Instance.GetPieceOnSquare(square);

            square.HighlightSquare(piece != null);

            moveIndicators.Add(square);
        }
    }

    // =========================================================
    // PIECE PREVIEW
    // =========================================================

    public void PreviewPiece(ChessPiece piece, Vector3 worldPosition)
    {
        if (piece == null)
            return;

        Transform parent = piece.transform.parent;

        if (parent == null)
            return;

        Vector3 localPosition = parent.InverseTransformPoint(worldPosition);

        localPosition.y = 0.01f;

        piece.transform.localPosition = localPosition;
    }

    public void ResetPreview(ChessPiece piece)
    {
        if (piece == null || piece.CurrentSquare == null)
            return;

        ChessPosition position = new ChessPosition(
            piece.CurrentSquare.X,
            piece.CurrentSquare.Z
        );

        piece.transform.position =
            chessBoard.GetWorldPosition(position);
    }

    // =========================================================
    // RESET PIECE
    // =========================================================

    public void ResetPiece(ChessPiece piece)
    {
        if (piece == null)
            return;

        ChessSquare currentSquare =
            piece.CurrentSquare;

        if (currentSquare == null)
            return;

        piece.transform.position = currentSquare.transform.position;
    }

    public void PlayMove(ChessPiece piece, ChessSquare start, ChessSquare end)
    {
        if (piece == null || start == null || end == null)
            return;

        Vector3 startPosition = chessBoard.GetWorldPosition(new ChessPosition(start.X, start.Z));

        Vector3 endPosition = chessBoard.GetWorldPosition(new ChessPosition(end.X, end.Z));

        piece.transform.position = startPosition;

        Tween.Position(
            piece.transform,
            endPosition,
            moveDuration,
            moveEase
        );
    }
    public void ClearSelection()
    {
        ClearMoveIndicators();
    } 

    public void ClearMoveIndicators()
    {
        foreach (ChessSquare indicator in moveIndicators)
        {
            if (indicator != null)
                indicator.ResetSquare();
        }

        moveIndicators.Clear();
    }

    public void CapturePiece(ChessPiece piece)
    {
        MovePieceOut(piece);
    }

    public void ResetPieces()
    {      
        ChessPiece[] pieces =
            FindObjectsByType<ChessPiece>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (ChessPiece piece in pieces)
        {
            if (piece == null)
                continue;

            ChessPosition position =
                new ChessPosition(
                    piece.BoardX,
                    piece.BoardZ
                );

            piece.transform.position =
                chessBoard.GetWorldPosition(position);

            piece.gameObject.SetActive(true);

            piece.ResetPiece();
        }

        ResetCapturedPieces();

        HideChessBoard();
    }

    public void ResetCapturedPieces()
    {
        for (int i = 0; i < blackOutPieces.Length; i++)
            blackOutPieces[i] = null;

        for (int i = 0; i < whiteOutPieces.Length; i++)
            whiteOutPieces[i] = null;
    }

    public void SetPieceDragging(ChessPiece piece, bool dragging)
    {
        if (piece == null)
            return;

        MeshCollider collider =
            piece.GetComponent<MeshCollider>();

        if (collider != null)
            collider.enabled = !dragging;
    }

    public void SetAllPieceColliders(bool enabled)
    {
        ChessPiece[] pieces =
            FindObjectsByType<ChessPiece>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (ChessPiece piece in pieces)
        {
            if (piece == null)
                continue;

            MeshCollider collider =
                piece.GetComponentInChildren<MeshCollider>();

            if (collider != null)
                collider.enabled = enabled;
        }
    }

    private void OnDestroy()
    {
        boardSequence.Stop();
        pieceSequence.Stop();
    }
}
