using UnityEngine;

public class CustomisationNavigationButton : WorldButtonAction
{
    public enum Direction
    {
        Previous,
        Next
    }

    [SerializeField]
    private PlayerCustomisationMachine machine;

    [SerializeField]
    private Direction direction;


    public override void Execute(PlayerNetworkObject player)
    {
        if (machine == null)
            return;

        int change = direction == Direction.Next ? 1 : -1;

        machine.ChangeSelection(player, change);
    }
}
