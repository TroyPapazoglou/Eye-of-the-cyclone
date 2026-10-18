using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private Transform boardPivot;

    [Header("Square Positions")]
    [SerializeField] private float firstSquareX = -0.067883f;
    [SerializeField] private float firstSquareZ = -0.067883f;
    [SerializeField] private float squareSize = 0.019394f;

    public static ChessBoard Instance;

    private void Awake()
    {
        Instance = this;
    }

    public Vector3 GetWorldPosition(ChessPosition position)
    {
        Vector3 localPosition = new Vector3(
            firstSquareX + position.x * squareSize,
            0.01f,
            firstSquareZ + position.z * squareSize
        );

        return boardPivot.TransformPoint(localPosition);
    }

    public bool IsInsideBoard(ChessPosition position)
    {
        return position.x >= 0 &&
               position.x < 8 &&
               position.z >= 0 &&
               position.z < 8;
    }

    public ChessPosition GetPositionFromWorld(Vector3 worldPosition)
    {
        Vector3 local =  boardPivot.InverseTransformPoint(worldPosition);

        float x =
            (local.x - firstSquareX) /
            squareSize;

        float z =
            (local.z - firstSquareZ) /
            squareSize;

        int boardX =
            Mathf.RoundToInt(x);

        int boardZ =
            Mathf.RoundToInt(z);

        ChessPosition position = new ChessPosition(boardX, boardZ);

        if (!IsInsideBoard(position))
            return null;

        return position;
    }
}
