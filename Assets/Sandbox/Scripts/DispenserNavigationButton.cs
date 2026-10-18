using UnityEngine;

public class DispenserNavigationButton : WorldButtonAction
{
    public enum Direction
    {
        Previous,
        Next
    }

    [SerializeField]
    private ObjectDispenserMachine machine;

    [SerializeField]
    private Direction direction;


    public override void Execute(PlayerNetworkObject player)
    {
        if (machine == null)
            return;

        switch (direction)
        {
            case Direction.Previous:
                machine.SelectPrevious();
                break;

            case Direction.Next:
                machine.SelectNext();
                break;
        }
    }
}

