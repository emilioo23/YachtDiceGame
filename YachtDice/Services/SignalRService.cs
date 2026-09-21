using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace YachtDice.Services
{
    public class SignalRService
    {
        private HubConnection _connection;
        
        public event Action<string, int[]> OnDiceRolledReceived;
        public event Action<string, string> OnChatMessageReceived;
        public event Action<string> OnPlayerJoined;
        public event Action<string> OnPlayerLeft;

        public SignalRService(string hubUrl)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string, int[]>("ReceiveDiceRoll", (player, dice) =>
            {
                App.Current.Dispatcher.Invoke(() => OnDiceRolledReceived?.Invoke(player, dice));
            });

            _connection.On<string, string>("ReceiveChatMessage", (player, message) =>
            {
                App.Current.Dispatcher.Invoke(() => OnChatMessageReceived?.Invoke(player, message));
            });

            _connection.On<string>("PlayerJoined", (player) =>
            {
                App.Current.Dispatcher.Invoke(() => OnPlayerJoined?.Invoke(player));
            });

            _connection.On<string>("PlayerLeft", (player) =>
            {
                App.Current.Dispatcher.Invoke(() => OnPlayerLeft?.Invoke(player));
            });
        }

        public async Task ConnectAsync()
        {
            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al conectar con el servidor SignalR.", ex);
            }
        }

        public async Task DisconnectAsync()
        {
            if (_connection != null)
            {
                await _connection.StopAsync();
            }
        }

        public async Task JoinRoomAsync(string roomCode, string playerName)
        {
            await _connection.InvokeAsync("JoinRoom", roomCode, playerName);
        }

        public async Task RollDiceAsync(string roomCode, string playerName, int[] dice)
        {
            await _connection.InvokeAsync("RollDice", roomCode, playerName, dice);
        }

        public async Task SendMessageAsync(string roomCode, string playerName, string message)
        {
            await _connection.InvokeAsync("SendMessage", roomCode, playerName, message);
        }
    }
}