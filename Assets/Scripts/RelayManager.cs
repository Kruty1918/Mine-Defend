using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Netcode;
using System.Collections;


namespace Kruty1918
{
    public class RelayManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private TMP_InputField joinInput;
        [SerializeField] private TextMeshProUGUI codeText;

        private async void Start()
        {
            await UnityServices.InitializeAsync();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            hostButton.onClick.AddListener(CreateRelay);
            joinButton.onClick.AddListener(() => JoinRelay(joinInput.text));
        }

        private async void CreateRelay()
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                codeText.text = joinCode;

                RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                NetworkManager.Singleton.StartHost();
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"Relay error: {e.Message}");
            }
        }

        private async void JoinRelay(string joinCode)
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

            StartCoroutine(SendNameAfterConnection());
        }

        private IEnumerator SendNameAfterConnection()
        {
            yield return new WaitUntil(() => NetworkManager.Singleton.IsConnectedClient);

            NameReporter.Instance.SendNameToHostServerRpc(nameInput.text);
        }
    }
}