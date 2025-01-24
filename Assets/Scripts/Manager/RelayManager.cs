using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Http;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Collections;

public class RelayManager : UnitySingleton<RelayManager>
{
    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback +=
            (NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse) =>
            {
                connectionApprovalResponse.Approved = true;
            };
    }

    /// <summary>
    /// 創建Relay
    /// </summary>
    /// <param name="maxConnections"></param>
    public async Task<string> CreateRelay(int maxConnections)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            RelayServerData relayServerData = allocation.ToRelayServerData("dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);            
            NetworkManager.Singleton.StartHost();

            Debug.Log($"創建Relay: {joinCode}");
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"創建Relay錯誤: {e}");
            return "";
        }
    }

    /// <summary>
    /// 加入Relay
    /// </summary>
    /// <param name="joinCode"></param>
    public async Task JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = joinAllocation.ToRelayServerData("dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();

            Debug.Log($"加入Relay: {joinCode}");
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"加入Relay錯誤: {e}");
        }
    }
}