using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Android.Util;
using Newtonsoft.Json;
using PubNubMessaging.Core;
using Newtonsoft.Json.Linq;

namespace RingCentral.Subscription
{
    public class SubscriptionServiceImplementation : ISubscriptionService
    {
        private readonly Pubnub _pubnub;
        private const string Tag = "RingCentral Android SDK";
        private bool _encrypted;
        private ICryptoTransform _decrypto;

        private Dictionary<string, object> _events = new Dictionary<string, object>
        {
            {"notification",""},
            {"errorMessage",""},
            {"connectMessage", ""},
            {"disconnectMessage",""}

        };

        public SubscriptionServiceImplementation(string publishKey, string subscribeKey)
        {
            _pubnub = new Pubnub(publishKey, subscribeKey);
        }
        public SubscriptionServiceImplementation(string publishKey, string subscribeKey, string secretKey)
        {
            _pubnub = new Pubnub(publishKey, subscribeKey, secretKey);
        }

        public SubscriptionServiceImplementation(string publishKey, string subscribeKey, string secretKey, string cipherKey, bool sslOn)
        {
            _pubnub = new Pubnub(publishKey, subscribeKey);
            _encrypted = true;
            var aes = new AesManaged { Key = Convert.FromBase64String(cipherKey), Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 };
            _decrypto = aes.CreateDecryptor();
        }

        public void Subscribe(string channel, string channelGroup, Action<object> userCallback, Action<object> connectCallback, Action<object> errorCallback)
        {
            _pubnub.Subscribe<string>(channel, channelGroup, NotificationReturnMessage,
                SubscribeConnectStatusMessage, ErrorMessage);
        }

        public void Unsubscribe(string channel, string channelGroup, Action<object> userCallback, Action<object> connectCallback,
            Action<object> disconnectCallback, Action<object> errorCallback)
        {
            _pubnub.Unsubscribe(channel, NotificationReturnMessage, SubscribeConnectStatusMessage,
                DisconnectMessage, ErrorMessage);
        }

        public void NotificationReturnMessage(object message)
        {
            if (_encrypted) _events["notification"] = DecryptMessage(message);
            else _events["notification"] = JObject.Parse(JsonConvert.DeserializeObject<List<string>>(message.ToString())[0]);
            Log.Debug(Tag, "Subscribe Message: " + message);
        }

        public void SubscribeConnectStatusMessage(object message)
        {
            _events["connectMessage"] = message;
            Log.Debug(Tag, "Connect Message: " + message);
        }

        public void ErrorMessage(object message)
        {
            _events["errorMessage"] = message;
            Log.Debug(Tag, "Error Message: " + message);
        }

        public void DisconnectMessage(object message)
        {
            _events["disconnectMessage"] = message;
            Log.Debug(Tag, "Disconnect Message: " + message);
        }
        public JObject DecryptMessage(object message)
        {

            var deserializedMessage = JsonConvert.DeserializeObject<List<string>>(message.ToString());
            byte[] decoded64Message = Convert.FromBase64String(deserializedMessage[0]);
            byte[] decryptedMessage = _decrypto.TransformFinalBlock(decoded64Message, 0, decoded64Message.Length);
            deserializedMessage[0] = Encoding.UTF8.GetString(decryptedMessage);
            return JObject.Parse(deserializedMessage[0]);
        }
        
    }
}