using System.Collections;
using Unity.Netcode;

public class PlayerHUD : NetworkBehaviour
{
    private NetworkVariable<NetworkString> playersName = new NetworkVariable<NetworkString>();
    private bool overlaySet = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        { 
            playersName.Value = $"Player {OwnerClientId}";
        }
        
    }


}


