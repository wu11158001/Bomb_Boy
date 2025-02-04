using UnityEngine;
using Unity.Services.Vivox;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using System.Collections.Generic;

public class VivoxManager : UnitySingleton<VivoxManager>
{
    // 當前加入頻道
    private string _currChannel;
    // 參與者列表
    public List<VivoxParticipant> VivoxParticipantList = new();
    // LobbyView
    private LobbyView _lobbyView;

    private void OnDestroy()
    {
        VivoxManager.I.UnBindVivoxEvent();
    }

    /// <summary>
    /// 綁定Vivox事件
    /// </summary>
    public void BindVivoxEvents()
    {
        VivoxService.Instance.LoggedIn += OnLoggedInVivox;
        VivoxService.Instance.ChannelJoined += OnChannelConnected;
        VivoxService.Instance.ChannelMessageReceived += OnChannelMessageReceived;
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
        VivoxService.Instance.ChannelLeft += OnChannelLeft;
        VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;
        VivoxService.Instance.LoggedOut += OnLoggedOutVivox;
    }

    /// <summary>
    /// 解除綁定Vivox事件
    /// </summary>
    public void UnBindVivoxEvent()
    {
        VivoxService.Instance.LoggedIn -= OnLoggedInVivox;
        VivoxService.Instance.ChannelJoined -= OnChannelConnected;
        VivoxService.Instance.ChannelMessageReceived -= OnChannelMessageReceived;
        VivoxService.Instance.ParticipantAddedToChannel -= OnParticipantAdded;
        VivoxService.Instance.ChannelLeft -= OnChannelLeft;
        VivoxService.Instance.ParticipantRemovedFromChannel -= OnParticipantRemoved;
        VivoxService.Instance.LoggedOut -= OnLoggedOutVivox;
    }

    /// <summary>
    /// 登入回傳事件
    /// </summary>
    private void OnLoggedInVivox()
    {
        Debug.Log("登入Vivox");
    }

    /// <summary>
    /// 登出回傳事件
    /// </summary>
    private void OnLoggedOutVivox()
    {
        Debug.Log("登出Vivox");
    }

    /// <summary>
    /// 加入Vivox頻道事件
    /// </summary>
    /// <param name="channel"></param>
    private void OnChannelConnected(string channel)
    {
        Debug.Log($"加入Vivox頻道: {channel}");
    }

    /// <summary>
    /// 離開Vivox頻道事件
    /// </summary>
    /// <param name="channel"></param>
    private void OnChannelLeft(string channel)
    {
        Debug.Log($"離開Vivox頻道: {channel}");
    }

    /// <summary>
    /// 參與者加入事件
    /// </summary>
    /// <param name="participant"></param>
    private void OnParticipantAdded(VivoxParticipant participant)
    {
        VivoxParticipantList.Add(participant);
    }

    /// <summary>
    /// 參與者移除事件
    /// </summary>
    /// <param name="participant"></param>
    private void OnParticipantRemoved(VivoxParticipant participant)
    {
        VivoxParticipantList.Remove(participant);
    }

    /// <summary>
    /// 文字消息事件
    /// </summary>
    /// <param name="message"></param>
    private void OnChannelMessageReceived(VivoxMessage message)
    {
        Debug.Log($"接收聊天訊息: {message.SenderDisplayName}: { message.MessageText}");

        if (_lobbyView == null)
        {
            _lobbyView = FindFirstObjectByType<LobbyView>();
        }

        if (_lobbyView != null)
        {
            ChatData chatData = new()
            {
                AuthenticationPlayerId = message.SenderPlayerId,
                Nickname = message.SenderDisplayName,
                ChatMsg = message.MessageText,
            };
            _lobbyView.ChatMessageReceived(chatData);
        }
    }

    /// <summary>
    /// 登入Vivox
    /// </summary>
    public async Task LoginToVivoxAsync()
    {
        LoginOptions options = new()
        {
            PlayerId = AuthenticationService.Instance.PlayerId,
            DisplayName = PlayerPrefs.GetString(LocalDataKeyManager.LOCAL_NICKNAME_KEY),
            EnableTTS = true,
        };

        await VivoxService.Instance.LoginAsync(options);
    }

    /// <summary>
    /// 加入Vivox頻道
    /// </summary>
    public async Task JoinGroupChannelAsync(string channel)
    {
        _currChannel = channel;

        ChannelOptions channelOptions = new()
        {
            MakeActiveChannelUponJoining = true,
        };
        await VivoxService.Instance.JoinGroupChannelAsync(channel, ChatCapability.TextAndAudio, channelOptions);
    }

    /// <summary>
    /// 離開Vivox頻道
    /// </summary>
    public async Task LeaveEchoChannelAsync()
    {
        await VivoxService.Instance.LeaveAllChannelsAsync();
        _currChannel = "";
    }

    /// <summary>
    /// 註銷Vivox
    /// </summary>
    public async Task LogoutOfVivoxAsync()
    {
        await VivoxService.Instance.LogoutAsync();
    }

    /// <summary>
    /// 發送Vivox文字訊息
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="message"></param>
    public async void SendMessageAsync(string message)
    {
        await VivoxService.Instance.SendChannelTextMessageAsync(_currChannel, message);
    }

    /// <summary>
    /// 屏蔽玩家
    /// </summary>
    /// <param name="playerId"></param>
    public async void BlockPlayer(string playerId)
    {
        await VivoxService.Instance.BlockPlayerAsync(playerId);
    }

    /// <summary>
    /// 解除屏蔽玩家
    /// </summary>
    /// <param name="playerId"></param>
    public async void UnblockPlayer(string playerId)
    {
        await VivoxService.Instance.UnblockPlayerAsync(playerId);
    }

    /// <summary>
    /// 本地玩家靜音控制
    /// </summary>
    /// <param name="isMute"></param>
    public void LocalMute(bool isMute)
    {
        if (isMute)
        {
            VivoxService.Instance.MuteInputDevice();
        }
        else
        {
            VivoxService.Instance.UnmuteInputDevice();
        }
    }
}
