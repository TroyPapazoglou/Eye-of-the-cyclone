using Mirror;
using System;


public interface IPCApplication
{
    void StartGame();

    void PlayerReady(NetworkConnectionToClient connection);

    void PlayerJoined(PlayerNetworkObject player, NetworkConnectionToClient connection);

    void PlayerLeft(PlayerNetworkObject player, NetworkConnectionToClient connection);

    event Action<bool> OnGameStateChanged;
}
