using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ChessColour
{
    White,
    Black
}

public class ChessGameManager : NetworkBehaviour, IPCApplication
{
    public static ChessGameManager Instance { get; private set; }
    

    [Header("References")]
    [SerializeField] private ChessBoard board;
    [SerializeField] private ChessAnimator chessAnimator;

    [Header("Board")]
    [SerializeField] private ChessSquare[] squares;

    [Header("Game")]
    [SerializeField] private ChessColour startingTurn = ChessColour.White;
       

    private HashSet<NetworkConnectionToClient> playersFinishedBoardAnimation = new();
    private HashSet<int> readyPlayers = new();

    public List<PlayerNetworkObject> players = new();    

    private readonly Dictionary<Vector2Int, ChessSquare> squareLookup = new();
    private readonly List<ChessSquare> possibleMoves = new();   

    private ChessPiece selectedPiece;

    private ChessPiece pendingPromotionPiece;
    private PlayerNetworkObject pendingPromotionPlayer;

    [SyncVar]
    private ChessColour currentTurn;

    private bool gameStarted;
    private bool gameEnded;

    public event Action<bool> OnGameStateChanged;

#pragma warning disable CS0414
    [SyncVar(hook = nameof(OnGameStateChangedSync))]
    private bool isGameActive;
#pragma warning restore CS0414

    public ChessPiece SelectedPiece => selectedPiece;

    public ChessColour CurrentTurn => currentTurn;    

    public bool GameStarted => gameStarted;

    public bool GameEnded => gameEnded;

    //public static event Action<PlayerNetworkObject> OnGameEnd;
    public static event Action OnGameStart;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildSquareLookup();
        InitializePieces();
    }   
    

    private void Start()
    {
        SteamLobbyManager.Instance.OnLobbyClosed += ServerResetGame;
        SteamLobbyManager.Instance.OnLeftLobby += ClientResetGame;
        PCNetworkManager.OnDisconnected += ClientResetGame;
    }

    private void OnDestroy()
    {
        SteamLobbyManager.Instance.OnLobbyClosed -= ServerResetGame;
        SteamLobbyManager.Instance.OnLeftLobby -= ClientResetGame;
        PCNetworkManager.OnDisconnected -= ClientResetGame;
    }

    public override void OnStartServer()
    {
        base.OnStartClient();

        if (PCBehaviour.Instance.ApplicationOpen == PCApplication.Chess)
        {
            PCBehaviour.Instance.RegisterApplication(this);
        }
    }

    private void OnGameStateChangedSync(bool oldValue, bool newValue)
    {
        OnGameStateChanged?.Invoke(newValue);
    }


    private void BuildSquareLookup()
    {
        squareLookup.Clear();

        foreach (ChessSquare square in squares)
        {
            if (square == null)
                continue;

            Vector2Int position = new Vector2Int(square.X, square.Z);

            squareLookup[position] = square;
        }      
    }

    private void InitializePieces()
    {
        ChessPiece[] pieces = FindObjectsByType<ChessPiece>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (ChessPiece piece in pieces)
        {
            if (piece == null)
                continue;

            ChessSquare square = GetSquare(piece.BoardX, piece.BoardZ);

            //if (square == null)
            //{
            //    Debug.LogWarning(
            //        $"Could not find square " +
            //        $"({piece.BoardX}, {piece.BoardZ}) " +
            //        $"for {piece.name}"
            //    );

            //    continue;
            //}

            piece.SetCurrentSquare(square);
        }
    }      

    public bool CanSelectPiece(ChessPiece piece, PlayerNetworkObject player)
    {
        if (gameEnded)
            return false;

        if (!gameStarted)
            return false;

        if (piece == null)
            return false;

        if (piece.CurrentSquare == null)
            return false;

        if (player == null)
            return false;
               
        if (piece.Colour != player.Chess.chessColour)
            return false;
              
        if (currentTurn != player.Chess.chessColour)
            return false;

        return true;
    }

    public void SelectPiece(ChessPiece piece)
    {            

        selectedPiece = piece;

        possibleMoves.Clear();

        possibleMoves.AddRange(GetLegalMoves(piece));       

        chessAnimator.ShowPossibleMoves(possibleMoves);
    }

    private List<ChessSquare> GetPossibleMoves(ChessPiece piece)
    {
        List<ChessSquare> moves = new();

        if (piece == null)
            return moves;

        switch (piece.PieceType)
        {
            case ChessPieceType.Pawn:
                AddPawnMoves(piece, moves);
                break;

            case ChessPieceType.Rook:
                AddRookMoves(piece, moves);
                break;

            case ChessPieceType.Knight:
                AddKnightMoves(piece, moves);
                break;

            case ChessPieceType.Bishop:
                AddBishopMoves(piece, moves);
                break;

            case ChessPieceType.Queen:
                AddQueenMoves(piece, moves);
                break;

            case ChessPieceType.King:
                AddKingMoves(piece, moves);
                break;
        }

        return moves;
    }

    public bool IsPossibleMove(ChessSquare square)
    {
        return possibleMoves.Contains(square);
    }

    public void ClearPossibleMoves()
    {
        possibleMoves.Clear();
        chessAnimator.ClearMoveIndicators();
    }

    private void AddPawnMoves(ChessPiece piece, List<ChessSquare> moves)
    {
        ChessSquare current = piece.CurrentSquare;      

        int direction = piece.Colour == ChessColour.White ? 1 : -1;

        int x = current.X;
        int z = current.Z;

        ChessSquare forward = GetSquare(x, z + direction);       

        if (forward != null && GetPieceOnSquare(forward) == null)
        {
            moves.Add(forward);          

            bool onStartingRank = piece.Colour == ChessColour.White ? z == 1: z == 6;

            if (onStartingRank)
            {
                ChessSquare doubleForward = GetSquare(x, z + direction * 2);

                if (doubleForward != null &&  GetPieceOnSquare(doubleForward) == null)
                {
                    moves.Add(doubleForward);
                }
            }
        }      

        AddPawnCapture(
            piece,
            x - 1,
            z + direction,
            moves
        );
        
        AddPawnCapture(
            piece,
            x + 1,
            z + direction,
            moves
        );
    }

    private void AddPawnCapture(ChessPiece piece, int x, int z, List<ChessSquare> moves)
    {
        ChessSquare square = GetSquare(x, z);

        if (square == null)
            return;

        ChessPiece target = GetPieceOnSquare(square);

        if (target == null)
            return;

        if (target.Colour == piece.Colour)
            return;

        moves.Add(square);
    }

    // =========================================================
    // ROOK
    // =========================================================

    private void AddRookMoves(ChessPiece piece, List<ChessSquare> moves)
    {
        AddSlidingMoves(
            piece,
            moves,
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        );
    }

    // =========================================================
    // BISHOP
    // =========================================================

    private void AddBishopMoves(
        ChessPiece piece,
        List<ChessSquare> moves)
    {
        AddSlidingMoves(
            piece,
            moves,
            new Vector2Int(1, 1),
            new Vector2Int(-1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, -1)
        );
    }

    // =========================================================
    // QUEEN
    // =========================================================

    private void AddQueenMoves(
        ChessPiece piece,
        List<ChessSquare> moves)
    {
        AddSlidingMoves(
            piece,
            moves,
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),

            new Vector2Int(1, 1),
            new Vector2Int(-1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, -1)
        );
    }

    // =========================================================
    // SLIDING PIECES
    // =========================================================

    private void AddSlidingMoves(
        ChessPiece piece,
        List<ChessSquare> moves,
        params Vector2Int[] directions)
    {
        ChessSquare current =
            piece.CurrentSquare;

        foreach (Vector2Int direction in directions)
        {
            int x = current.X;
            int z = current.Z;

            while (true)
            {
                x += direction.x;
                z += direction.y;

                ChessSquare square =
                    GetSquare(x, z);

                if (square == null)
                    break;

                ChessPiece target =
                    GetPieceOnSquare(square);

                // Empty square.
                if (target == null)
                {
                    moves.Add(square);
                    continue;
                }

                // Enemy piece can be captured,
                // but we cannot move beyond it.
                if (target.Colour != piece.Colour)
                {
                    moves.Add(square);
                }

                break;
            }
        }
    }

    // =========================================================
    // KNIGHT
    // =========================================================

    private void AddKnightMoves(
        ChessPiece piece,
        List<ChessSquare> moves)
    {
        ChessSquare current =
            piece.CurrentSquare;

        Vector2Int[] offsets =
        {
            new(1, 2),
            new(2, 1),
            new(-1, 2),
            new(-2, 1),

            new(1, -2),
            new(2, -1),
            new(-1, -2),
            new(-2, -1)
        };

        foreach (Vector2Int offset in offsets)
        {
            ChessSquare square =
                GetSquare(
                    current.X + offset.x,
                    current.Z + offset.y
                );

            if (square == null)
                continue;

            ChessPiece target =
                GetPieceOnSquare(square);

            if (target == null ||
                target.Colour != piece.Colour)
            {
                moves.Add(square);
            }
        }
    }

    // =========================================================
    // KING
    // =========================================================

    private void AddKingMoves(
        ChessPiece piece,
        List<ChessSquare> moves)
    {
        ChessSquare current =
            piece.CurrentSquare;

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0)
                    continue;

                ChessSquare square =
                    GetSquare(
                        current.X + x,
                        current.Z + z
                    );

                if (square == null)
                    continue;

                ChessPiece target =
                    GetPieceOnSquare(square);

                if (target == null ||
                    target.Colour != piece.Colour)
                {
                    moves.Add(square);
                }
            }
        }
    }

    // =========================================================
    // BOARD LOOKUP
    // =========================================================

    private ChessSquare GetSquare(int x, int z)
    {
        squareLookup.TryGetValue(
       new Vector2Int(x, z),
       out ChessSquare square
   );

        return square;
    }

    // =========================================================
    // PIECE LOOKUP
    // =========================================================

    public ChessPiece GetPieceOnSquare(ChessSquare square)
    {
        if (square == null)
            return null;

        ChessPiece[] pieces =
            FindObjectsByType<ChessPiece>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (ChessPiece piece in pieces)
        {
            if (piece == null)
                continue;

            if (piece.IsOut)
                continue;

            if (piece.CurrentSquare == square)
                return piece;
        }

        return null;
    }

    private ChessPiece GetPieceAtPosition(int x, int z)
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

            ChessSquare currentSquare = piece.CurrentSquare;

            if (currentSquare == null)
                continue;

            if (currentSquare.X == x &&
                currentSquare.Z == z)
            {
                return piece;
            }
        }

        return null;
    }  

    // =========================================================
    // PIECE PREVIEW
    // =========================================================

    public void PreviewPiecePosition(ChessPiece piece, Ray ray)
    {
        if (piece == null)
            return;

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        chessAnimator.PreviewPiece(piece, hit.point);
    }


    // =========================================================
    // RELEASE
    // =========================================================

    public void HandlePieceOut(ChessPiece piece)
    {
        if (piece == null)
            return;

        chessAnimator.MovePieceOut(piece);
    }

    public void ReleasePiece(ChessPiece piece, Ray ray)
    {
        if (piece == null)
            return;

        ChessSquare targetSquare = GetSquareFromRay(ray);

        // Released somewhere that isn't a square.
        if (targetSquare == null)
        {
            chessAnimator.ResetPiece(piece);
            ClearSelection();
            return;
        }

        // Released on an illegal square.
        if (!possibleMoves.Contains(targetSquare))
        {
            chessAnimator.ResetPiece(piece);
            ClearSelection();
            return;
        }

        RegisterMove(
            piece,
            targetSquare
        );
    }

    // =========================================================
    // REGISTER MOVE
    // =========================================================

    private void RegisterMove(ChessPiece piece, ChessSquare targetSquare)
    {
        if (piece == null || targetSquare == null)
            return;

        ChessSquare startingSquare = piece.CurrentSquare;

        if (!possibleMoves.Contains(targetSquare))
        {
            chessAnimator.ResetPiece(piece);
            ClearSelection();
            return;
        }

        PlayerNetworkObject localPlayer =
            PlayerNetworkObject.LocalPlayer;

        if (localPlayer == null)
        {
            chessAnimator.ResetPiece(piece);
            ClearSelection();
            return;
        }

        localPlayer.Chess.CmdMovePiece(
            startingSquare.X,
            startingSquare.Z,
            targetSquare.X,
            targetSquare.Z
        );

        ClearSelection();
    }

    [Server]
    public void TryMovePiece(PlayerNetworkObject player, int startX, int startZ, int targetX, int targetZ)
    {
        if (gameEnded)
            return;

        ChessPiece piece = GetPieceAtPosition(startX, startZ);

        if (piece == null)
            return;

        ChessSquare targetSquare = GetSquare(targetX, targetZ);

        if (targetSquare == null)
            return;

        List<ChessSquare> legalMoves = GetLegalMoves(piece);

        if (!legalMoves.Contains(targetSquare))
            return;

        ChessPiece targetPiece = GetPieceAtPosition(targetX, targetZ);
               
        if (targetPiece != null && targetPiece.Colour != piece.Colour)
        {
            CapturePiece(player, targetPiece);           
        }
               
        piece.SetCurrentSquare(targetSquare);
                
        RpcMovePiece(
            piece.BoardX,
            piece.BoardZ,
            startX,
            startZ,
            targetX,
            targetZ
        );

        if (IsPromotionRank(piece, targetSquare))
        {
            BeginPromotion(player, piece);
            return;
        }

        EndTurn();
    }

    [Server]
    public void TryPromotePiece(PlayerNetworkObject player, ChessPieceInfo pieceInfo, ChessPieceType newType)
    {
        if (pendingPromotionPiece == null)
            return;

        if (pendingPromotionPlayer != player)
            return;

        if (newType != ChessPieceType.Queen &&
            newType != ChessPieceType.Rook &&
            newType != ChessPieceType.Bishop &&
            newType != ChessPieceType.Knight)
        {
            return;
        }

        ChessPiece piece = chessAnimator.GetPiece(pieceInfo);

        if (piece == null)
            return;

        if (piece.PieceType != ChessPieceType.Pawn)
            return;

        if (!IsPromotionRank(piece))
            return;

        piece.SetPieceType(newType);

        RpcPromotePiece(pieceInfo, newType);

        pendingPromotionPiece = null;
        pendingPromotionPlayer = null;

        EndTurn();
    }

    private bool IsPromotionRank(ChessPiece piece)
    {
        ChessSquare square = piece.CurrentSquare;

        if (square == null)
            return false;

        if (piece.Colour == ChessColour.White)
            return square.Z == 7;

        return square.Z == 0;
    }

    private ChessPiece GetKing(ChessColour colour)
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

            if (piece.IsOut)
                continue;

            if (piece.PieceType != ChessPieceType.King)
                continue;

            if (piece.Colour != colour)
                continue;

            return piece;
        }

        return null;
    }

    private bool IsKingInCheck(ChessColour colour)
    {
        ChessPiece king = GetKing(colour);

        if (king == null)
            return false;

        ChessSquare kingSquare = king.CurrentSquare;

        if (kingSquare == null)
            return false;

        ChessPiece[] pieces =
            FindObjectsByType<ChessPiece>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (ChessPiece piece in pieces)
        {
            if (piece == null)
                continue;

            if (piece.IsOut)
                continue;

            if (piece.Colour == colour)
                continue;

            List<ChessSquare> moves = GetPossibleMoves(piece);

            if (moves.Contains(kingSquare))
            {
                Debug.Log(
                    $"{colour} king is in check by {piece.PieceType}"
                );

                return true;
            }
        }

        return false;
    }

    private bool HasAnyLegalMoves(ChessColour colour)
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

            if (piece.IsOut)
                continue;

            if (piece.Colour != colour)
                continue;

            List<ChessSquare> moves =
                GetPossibleMoves(piece);

            if (moves.Count > 0)
                return true;
        }

        return false;
    }

    private List<ChessSquare> GetLegalMoves(ChessPiece piece)
    {
        List<ChessSquare> legalMoves = new();

        if (piece == null)
            return legalMoves;

        ChessSquare originalSquare = piece.CurrentSquare;

        if (originalSquare == null)
            return legalMoves;

        List<ChessSquare> possibleMoves = GetPossibleMoves(piece);

        foreach (ChessSquare targetSquare in possibleMoves)
        {
            ChessPiece capturedPiece = GetPieceOnSquare(targetSquare);

            // Temporarily move the piece.
            piece.SetCurrentSquare(targetSquare);

            if (capturedPiece != null)
            {
                capturedPiece.SetIsOut(true);
                capturedPiece.SetCurrentSquare(null);
            }

            bool putsKingInCheck = IsKingInCheck(piece.Colour);

            // Undo captured piece.
            if (capturedPiece != null)
            {
                capturedPiece.SetIsOut(false);
                capturedPiece.SetCurrentSquare(targetSquare);
            }

            // Undo moving piece.
            piece.SetCurrentSquare(originalSquare);

            if (!putsKingInCheck)
            {
                legalMoves.Add(targetSquare);
            }
        }

        return legalMoves;
    }

    [Server]
    private void CapturePiece(PlayerNetworkObject player, ChessPiece piece)
    {
        if (piece == null)
            return;

        //Debug.Log("capturing piece");

        piece.SetIsOut(true);
        piece.SetCurrentSquare(null);

        PlayerStatsNetwork.Instance.TargetAddPieceCaptured(player.connectionToClient);

        if (piece.PieceType == ChessPieceType.King)
        {
            ChessColour winningColour = piece.Colour == ChessColour.White ? ChessColour.Black : ChessColour.White;

            EndGame(winningColour);
        }

        RpcCapturePiece(piece.GetPieceInfo());
    }

    [Server]
    private void BeginPromotion(PlayerNetworkObject player, ChessPiece piece)
    {
        pendingPromotionPlayer = player;
        pendingPromotionPiece = piece;

        player.Chess.TargetShowPromotionUI(piece.pieceInfo, piece.Colour);
        PlayerStatsNetwork.Instance.TargetAddPawnPromotion(player.connectionToClient);
    }

    private bool IsPromotionRank(ChessPiece piece, ChessSquare square)
    {
        if (piece.PieceType != ChessPieceType.Pawn)
            return false;

        if (piece.Colour == ChessColour.White && square.Z == 7)
        {
            return true;
        }

        if (piece.Colour == ChessColour.Black && square.Z == 0)
        {
            return true;
        }

        return false;
    }

    [Server]
    private void EndTurn()
    {
        currentTurn = currentTurn == ChessColour.White ? ChessColour.Black : ChessColour.White;

        // Check whether the player whose turn it now is
        // has any legal moves.
        if (!HasAnyLegalMoves(currentTurn))
        {
            // No legal moves + king is in check = checkmate.
            if (IsKingInCheck(currentTurn))
            {
                ChessColour winningColour = currentTurn == ChessColour.White ? ChessColour.Black : ChessColour.White;

                EndGame(winningColour);

                PlayerNetworkObject winnerPlayer = players.FirstOrDefault(player => !player.IsSpectator && player.Chess.chessColour == winningColour);
                PlayerStatsNetwork.Instance.TargetAddCheckmate(winnerPlayer.connectionToClient);
                return;
            }

            // No legal moves + king is NOT in check = stalemate.
            EndGameStalemate();
            return;
        }

        // The game is still going.
        foreach (PlayerNetworkObject player in players)
        {
            bool canInteract =
                player.Chess.chessColour == currentTurn;

            TargetSetCanInteract(
                player.connectionToClient,
                canInteract
            );
        }
    }



    private void ClearSelection()
    {
        selectedPiece = null;

        possibleMoves.Clear();

        chessAnimator.ClearSelection();
    }
    
    private ChessSquare GetSquareFromRay(Ray ray)
    {
        if (!Physics.Raycast(ray,out RaycastHit hit))
        {
            return null;
        }

        return hit.collider.GetComponent<ChessSquare>();
    }

    [Server]
    private void EndGameStalemate()
    {
        gameEnded = true;
        isGameActive = false;

        foreach (PlayerNetworkObject player in players)
        {
            if (player.IsSpectator)
                return;
            PlayerStatsNetwork.Instance.TargetAddStalemate(player.connectionToClient);
        }

      

        // Your stalemate stat/event logic here.

        //RpcGameEnded();
    }

    [Server]
    public void EndGame(ChessColour winningColour)
    {
        if (gameEnded)
            return;

        gameEnded = true;

        ClearSelection();

        PlayerNetworkObject winnerPlayer = players.Where(player =>!player.isOut && !player.IsSpectator && player.Chess.chessColour == winningColour).FirstOrDefault();

        GameEvents.InvokeOnAnyGameEnd(winnerPlayer);

        PlayerStatsNetwork.Instance.TargetAddChessWin(winnerPlayer.connectionToClient);

        Debug.Log(
            $"Chess game ended. Winner: {winningColour}"
        );
    }


    [Server]
    public void StartGame()
    {
        if(PlayersAreReady())
        {
            OnGameStart?.Invoke();

            isGameActive = true;

            AssignPlayerColours();

            RpcAnimateBoard();
        }
        else
        {
            GameEvents.InvokeNotEnoughPlayersNeeded();
        }
    }

    [Server]
    private void AssignPlayerColours()
    {
        List<PlayerNetworkObject> activePlayers = players.Where(player => !player.IsSpectator).Take(2).ToList();      

        bool firstPlayerIsWhite = UnityEngine.Random.value > 0.5f;        

        activePlayers[0].Chess.chessColour = firstPlayerIsWhite ? ChessColour.White : ChessColour.Black;

        activePlayers[1].Chess.chessColour = firstPlayerIsWhite ? ChessColour.Black : ChessColour.White;
    }

    [Server]
    private bool PlayersAreReady()
    {
        return readyPlayers.Count == players.Count && players.Count > 1;
    }

    [Server]
    public void PlayerFinishedBoardAnimation(NetworkConnectionToClient connectionId)
    {
        playersFinishedBoardAnimation.Add(connectionId);

        if (playersFinishedBoardAnimation.Count == players.Count)
        {
            playersFinishedBoardAnimation.Clear();

            StartTurn();
        }
    }


    [Server]
    public void PlayerReady(NetworkConnectionToClient conn)
    {
        readyPlayers.Add(conn.connectionId);

        Debug.Log($"Player {conn.connectionId} ready.");
    }

    [Server]
    public void PlayerUnready(NetworkConnectionToClient conn)
    {
        readyPlayers.Remove(conn.connectionId);

        Debug.Log($"Player {conn.connectionId} unready.");

    }

    [Server]
    private void ServerResetGame()
    {
        ResetGame();
    }

    private void ClientResetGame()
    {
        PlayerBehaviour.Instance.canInteract = false;

        ChessAnimator.Instance.ResetPieces();
    } 

    [Server]
    public void ResetGame()
    {
        //add stuff in here buh            
        isGameActive = false;
        readyPlayers.Clear();

        foreach (PlayerNetworkObject player in players)
        {            
            TargetSetCanInteract(player.connectionToClient, false);
        }

        ChessAnimator.Instance.ResetPieces();

        gameStarted = false;
        gameEnded = false;

        currentTurn = startingTurn;

        selectedPiece = null;
        possibleMoves.Clear();

        ResetPieces();
    }

    [Server]
    private void StartTurn()
    {        

        currentTurn = startingTurn;
        gameStarted = true;

        foreach (PlayerNetworkObject player in players)
        {
            bool canInteract = player.Chess.chessColour == currentTurn;

            TargetSetCanInteract(player.connectionToClient, canInteract);
        }
    }

    [Server]
    private void ResetPieces()
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

            ChessSquare startingSquare =
                GetSquare(
                    piece.BoardX,
                    piece.BoardZ
                );

            if (startingSquare == null)
                continue;

            piece.SetCurrentSquare(startingSquare);
            piece.SetIsOut(false);
        }
    }  

    [ClientRpc]
    private void RpcAnimateBoard()
    {
        ChessColour localColour = PlayerNetworkObject.LocalPlayer.Chess.chessColour;

        bool flipBoard = localColour == ChessColour.Black;

        ChessAnimator.Instance.PlayIntroAnimation(flipBoard);
        
    }

    [ClientRpc]
    private void RpcMovePiece(int pieceBoardX, int pieceBoardZ, int startX, int startZ, int targetX, int targetZ)
    {
        ChessSquare startSquare = GetSquare(startX, startZ);

        ChessSquare targetSquare = GetSquare(targetX, targetZ);

        if (startSquare == null || targetSquare == null)
        {
            return;
        }

        ChessPieceInfo pieceInfo =
            new ChessPieceInfo(
                pieceBoardX,
                pieceBoardZ
            );

        ChessPiece piece = chessAnimator.GetPiece(pieceInfo);

        if (piece == null)
            return;

        chessAnimator.PlayMove(
            piece,
            startSquare,
            targetSquare
        );

        // Update this client's logical state.
        piece.SetCurrentSquare(targetSquare);
    }

    [ClientRpc]
    private void RpcCapturePiece(ChessPieceInfo info)
    {
        ChessPiece piece = chessAnimator.GetPiece(info);

        if (piece == null)
            return;

        chessAnimator.CapturePiece(piece);
    }

    [ClientRpc]
    private void RpcPromotePiece(ChessPieceInfo pieceInfo, ChessPieceType newType)
    {
        ChessPiece piece = chessAnimator.GetPiece(pieceInfo);

        if (piece == null)
            return;

        piece.SetPieceType(newType);

        piece.SetPieceVisual(piece, newType);
    }    
    
    [TargetRpc]
    private void TargetSetCanInteract(NetworkConnectionToClient connection, bool canInteract)
    {
        PlayerBehaviour.Instance.canInteract = canInteract;
        PlayerBehaviour.Instance.OnChessTurn();
    }

   

    public void PlayerJoined(PlayerNetworkObject player, NetworkConnectionToClient connection)
    {
        players.Add(player);
    }

    public void PlayerLeft(PlayerNetworkObject player, NetworkConnectionToClient connection)
    {
        players.Remove(player);
        PlayerUnready(connection);
    }
}
