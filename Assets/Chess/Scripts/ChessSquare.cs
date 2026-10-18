using UnityEngine;

public class ChessSquare : MonoBehaviour
{
    
    [SerializeField] private Material highlightMat;

    [SerializeField] private int x;
    [SerializeField] private int z;

    private MeshRenderer meshRenderer;
    private Material[] materials;
    private Material originalInnerMat;
    private Material originalOuterMat;

    public int X => x;
    public int Z => z;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        materials = meshRenderer.materials;

        originalOuterMat = materials[0];

        originalInnerMat = materials[1];
    }

    public void HighlightSquare(bool hasPiece)
    {
        materials[1] = highlightMat;

        if (hasPiece)
        {
            materials[0] = highlightMat;
        }

        meshRenderer.materials = materials;
    }

    public void ResetSquare()
    {
        materials[0] = originalOuterMat;
        materials[1] = originalInnerMat;

        meshRenderer.materials = materials;
    }
}
