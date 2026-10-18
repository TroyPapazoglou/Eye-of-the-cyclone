using UnityEngine;

public class CustomisationColourButton : WorldButtonAction
{
    [SerializeField]
    private PlayerCustomisationMachine machine;

    [SerializeField]
    [Range(0, PlayerColours.Count - 1)]
    private int colourIndex;


    private void Start()
    {
        ApplyButtonColour();
    }

     
    public override void Execute(PlayerNetworkObject player)
    {
        if (machine == null)
        {
            Debug.LogWarning("Customisation machine is not assigned.");
            return;
        }     
   

        machine.SelectColour(player, colourIndex);
    }


    private void ApplyButtonColour()
    {
        Renderer renderer = GetComponent<Renderer>();

        if (renderer == null)
            return;

        renderer.material.color =
            PlayerColours.GetColour(colourIndex);
    }
}
