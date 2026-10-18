using UnityEngine;

public class DispenserSpawnButton : WorldButtonAction
{
    [SerializeField]
    private ObjectDispenserMachine machine;

    public override void Execute(PlayerNetworkObject player)
    {
        if (machine == null)
            return;

        machine.SpawnSelectedObject();
    }
}
