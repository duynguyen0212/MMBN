using System;
using System.Collections;
using System.Collections.Generic;
using QFSW.QC;
using QFSW.QC.Actions;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TestRelay : MonoBehaviour
{
    [SerializeField] private Button createRelayBtn, joinRelayBtn;
    [SerializeField] private TextMeshProUGUI joinCodeUI;
    [SerializeField] private TMP_InputField inputField;

    private string joinCode;
    public GameObject relayUI;

    //When server start give the player an anonymous account 
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    //After one player create the relay, other player can input the join code
    private void Awake()
    {
        createRelayBtn.onClick.AddListener(() =>
        {
            CreateRelay();
        }
        );

        joinRelayBtn.onClick.AddListener(() =>
        {
            string str = inputField.text;
            if (str == null)
            {
                Debug.Log("Please enter join code");
                return;
            }
            try
            {
                JoinRelay(str);
                DeactivateUIServerRPC();
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
            }
        });
    }

    //Create relay function
    private async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);

            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            //joinCodeUI.text = joinCode;
            Debug.Log(joinCode);
            ShowJoinCodeServerRPC();
            StartCoroutine(TimerCoroutine());
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

        }
        catch (RelayServiceException e) { Debug.Log(e); }


    }

    //Join relay function
    private async void JoinRelay(string joinCodeR)
    {
        try
        {
            Debug.Log("Joining relay with " + joinCodeR);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCodeR);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

        }
        catch (RelayServiceException e) { Debug.Log(e); }
    }

    //Function to call the server to noitice client deactivate the UI
    [ServerRpc]
    private void DeactivateUIServerRPC()
    {
        DeactivateUIClientRPC();
    }


    [ServerRpc]
    private void ShowJoinCodeServerRPC()
    {
        joinCodeUI.text = joinCode;

    }

    IEnumerator TimerCoroutine()
    {
        yield return new WaitForSeconds(10f);
        relayUI.SetActive(false);
    }

    [ClientRpc]
    private void DeactivateUIClientRPC()
    {
        relayUI.SetActive(false);
    }
}
