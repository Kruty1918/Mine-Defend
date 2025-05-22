using UnityEngine;
using Unity.Netcode;


namespace Kruty1918
{
    public class NameReporter : NetworkBehaviour
    {
        public static NameReporter Instance;

        private void Awake()
        {
            Instance = this;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendNameToHostServerRpc(string name, ServerRpcParams serverRpcParams = default)
        {
            ulong clientID = serverRpcParams.Receive.SenderClientId;
            Debug.Log($"New player: {name} is connected! \n ClientID: {clientID}");
        }
    }
}