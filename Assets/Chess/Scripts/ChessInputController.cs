using UnityEngine;
using UnityEngine.InputSystem;

public class ChessInputController : MonoBehaviour
{
    public static ChessInputController Instance { get; private set; }

    [SerializeField] private Camera mainCamera;
    [SerializeField] private ChessGameManager chessManager;

    [SerializeField] private LayerMask chessPieceMask;
    [SerializeField] private LayerMask chessSquareMask;

    private ChessPiece selectedPiece;
    private bool holdingPiece;

    private Plane dragPlane;

    private Vector3 dragOffset;

    private void Awake()
    {
        Instance = this;
    }

    public void BeginHold(Vector2 mousePosition)
    {
        if (holdingPiece)
        {
            Debug.Log("already holding piece?");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, chessPieceMask))                    
            return;
        

        if (!hit.collider.TryGetComponent(out ChessPiece piece))
        {
            Debug.Log("didnt hit chess piece");
            return;
        }

        PlayerNetworkObject localPlayer = PlayerNetworkObject.LocalPlayer;     

        selectedPiece = piece;
        holdingPiece = true;

        dragPlane = new Plane(ChessBoard.Instance.transform.up, ChessBoard.Instance.transform.position);

        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 clickWorldPosition = ray.GetPoint(distance);
                       
            dragOffset = piece.transform.position - clickWorldPosition;
                       
            dragOffset.y = 0f;
        }


        ChessAnimator.Instance.SetAllPieceColliders(false);

        chessManager.SelectPiece(piece);
    }

    public void UpdateMousePosition(Vector2 mousePosition)
    {
        if (!holdingPiece)
            return;

        if (selectedPiece == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (!dragPlane.Raycast(ray, out float distance))
            return;

        Vector3 mouseWorldPosition = ray.GetPoint(distance);

       
        Vector3 pieceWorldPosition = mouseWorldPosition + dragOffset;

        ChessAnimator.Instance.PreviewPiece(selectedPiece, pieceWorldPosition);
    }

    public void EndHold(Vector2 mousePosition)
    {
        if (!holdingPiece)
            return;

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, chessSquareMask))
        {
            Debug.Log("ray hit not a square?");
            CancelSelection();
            return;
        }

        if (!hit.collider.TryGetComponent(out ChessSquare square))
        {
            Debug.Log("ray hit soimething with not a chesssquare comp");
            CancelSelection();
            return;
        }

        // Was this square actually one of the possible moves?
        if (!chessManager.IsPossibleMove(square))
        {
            Debug.Log("ray hit a square that was not a valid move");
            CancelSelection();
            return;
        }      

        ChessSquare startSquare = selectedPiece.CurrentSquare;

        // Send the move to the server.
        PlayerNetworkObject.LocalPlayer.Chess.CmdMovePiece(
            startSquare.X,
            startSquare.Z,
            square.X,
            square.Z
        );      

        CancelSelection();

        //PlayerBehaviour.Instance.canInteract = false;

    }
    
    private void CancelSelection()
    {
        if (selectedPiece != null)
        {
            ChessAnimator.Instance.SetAllPieceColliders(true);

            ChessAnimator.Instance.ResetPreview(selectedPiece);
        }


        holdingPiece = false;
        selectedPiece = null;

        chessManager.ClearPossibleMoves();       
    }

}
