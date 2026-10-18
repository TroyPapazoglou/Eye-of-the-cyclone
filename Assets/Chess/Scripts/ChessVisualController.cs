using System.Collections.Generic;
using UnityEngine;

public class ChessVisualController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ChessBoard board;

    [Header("Move Highlights")]
    [SerializeField] private Transform[] moveHighlights;

    [Header("Selection")]
    [SerializeField] private GameObject selectionOutline;

    private ChessPiece selectedPiece;

    public void SelectPiece(ChessPiece piece, List<ChessPosition> legalMoves)
    {
        ClearSelection();

        if (piece == null)
            return;

        selectedPiece = piece;

        if (selectionOutline != null)
        {
            selectionOutline.SetActive(true);
            selectionOutline.transform.position =
                piece.transform.position;
        }

        ShowLegalMoves(legalMoves);
    }

    public void ClearSelection()
    {
        selectedPiece = null;

        if (selectionOutline != null)
            selectionOutline.SetActive(false);

        HideLegalMoves();
    }

    private void ShowLegalMoves(List<ChessPosition> legalMoves)
    {
        HideLegalMoves();

        int count = Mathf.Min(legalMoves.Count, moveHighlights.Length);

        for (int i = 0; i < count; i++)
        {
            Transform highlight =
                moveHighlights[i];

            highlight.gameObject.SetActive(true);

            highlight.position =
                board.GetWorldPosition(
                    legalMoves[i]
                );
        }
    }

    private void HideLegalMoves()
    {
        foreach (RectTransform highlight
                 in moveHighlights)
        {
            highlight.gameObject.SetActive(false);
        }
    }
}
