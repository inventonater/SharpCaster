﻿using Microsoft.Extensions.Logging;
using Sharpcaster.Interfaces;
using Sharpcaster.Messages.Spotify;
using Sharpcaster.Models.Spotify;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sharpcaster.Channels
{
    public class SpotifyChannel : ChromecastChannel
    {
        public event EventHandler<SpotifyStatus> SpotifyStatusUpdated;
        public SpotifyStatus SpotifyStatus { get; set; }
        public event EventHandler<AddUserResponseMessagePayload> AddUserResponseReceived;

        public SpotifyChannel(ILogger<SpotifyChannel> logger = null) : base("urn:x-cast:com.spotify.chromecast.secure.v1", logger, false)
        {
        }

        /// <summary>
        /// Called when a message for this channel is received
        /// </summary>
        /// <param name="message">message to process</param>
        public override Task OnMessageReceivedAsync(IMessage message)
        {
            switch (message)
            {
                case GetInfoResponseMessage getInfoResponseMessage:
                    SpotifyStatus = getInfoResponseMessage.Payload;
                    SpotifyStatusUpdated?.Invoke(this, getInfoResponseMessage.Payload);
                    break;
                case AddUserResponseMessage addUserResponseMessage:
                    AddUserResponseReceived?.Invoke(this, addUserResponseMessage.Payload);
                    break;
                default:
                    break;
            }

            return base.OnMessageReceivedAsync(message);
        }

        /// <summary>
        /// Raises the StatusChanged event
        /// </summary>
        protected virtual void OnStatusChanged()
        {
        }

        public async Task GetSpotifyInfo()
        {
            await SendAsync(new GetInfoMessage
            {
                Payload = new GetInfoMessagePayload
                {
                    DeviceId = SpotifyDeviceId,
                    RemoteName = Client.FriendlyName,
                    DeviceAPI_isGroup = false
                }
            }, Client.GetChromecastStatus().Application.TransportId);
        }

        public async Task AddUser(string accessToken)
        {
            await SendAsync(new AddUserMessage
            {
                Payload = new AddUserMessagePayload
                {
                    Blob = accessToken,
                    TokenType = "accesstoken"
                }
            }, Client.GetChromecastStatus().Application.TransportId);
        }

        public string SpotifyDeviceId
        {
            get
            {
                var friendlyName = Client.FriendlyName;
                return ComputeMd5Hash(friendlyName);
            }
        }

        public static string ComputeMd5Hash(string input)
        {
            // Create an instance of the MD5 service provider
            using (MD5 md5 = MD5.Create())
            {
                // Compute the hash as a byte array
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Convert the byte array to a hexadecimal string
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
