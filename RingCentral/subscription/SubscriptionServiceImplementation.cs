﻿using Newtonsoft.Json;
using PCLCrypto;
using PubNubMessaging.Core;
using RingCentral.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;


namespace RingCentral.Subscription
{
    public class SubscriptionServiceImplementation
    {
        private Pubnub _pubnub;
<<<<<<< HEAD
        private bool _encrypted;
        public Platform _platform;
        private Subscription _subscription;
        private Timer timeout;
        private bool subscribed;
        private List<string> eventFilters = new List<string>();
        private const string SubscriptionEndPoint = "/restapi/v1.0/subscription";
        private const int RenewHandicap = 100000;
        private Action<object> notificationAction, connectionAction, errorAction;
        public Action<object> disconnectAction { private get; set; }
=======
		private bool _encrypted;
		private PubnubCrypto _decrypto;
		public Platform _platform;
		private Subscription _subscription;
		private Timer timeout;
		private bool subscribed;
		private List<string> eventFilters =  new List<string>();
		private const string SubscriptionEndPoint = "/restapi/v1.0/subscription";
		private const int RenewHandicap = 100000;
		private Action<object> notificationAction, connectionAction, errorAction;
		public Action<object> disconnectAction { private get; set; }
>>>>>>> master
        private bool _enableSSL;
        private Dictionary<string, object> _events = new Dictionary<string, object>
        {
            {"notification",""},
            {"errorMessage",""},
            {"connectMessage", ""},
            {"disconnectMessage",""}
        };

        private void SetTimeout()
        {
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            timeout = new Timer(OnTimedExpired, autoEvent, (int)((_subscription.ExpiresIn * 1000) - RenewHandicap), Timeout.Infinite);
            GC.KeepAlive(timeout);
        }

        public bool IsSubscribed()
        {
            return subscribed;
        }

        private void ClearTimeout()
        {
            if (timeout != null)
            {
                timeout.Dispose();
            }
        }

        public List<string> GetEvents()
        {
            return eventFilters;
        }

        public void ClearEvents()
        {
            eventFilters.Clear();
        }

        private void OnTimedExpired(Object source)
        {
            timeout.Dispose();
            if (subscribed)
            {
                Renew();
            }
            else
            {
                Unsubscribe();
            }
        }

        public void SetEvents(List<string> newEventFilters)
        {
            eventFilters = newEventFilters;
        }

        public void AddEvent(string eventToAdd)
        {
            eventFilters.Add(eventToAdd);
        }

        public ApiResponse Renew()
        {
            ClearTimeout();
            try
            {
                if (_subscription == null || string.IsNullOrEmpty(_subscription.Id))
                {
                    throw new Exception("Subscription ID is required");
                }
                if (eventFilters.Count == 0)
                {
                    throw new Exception("Events are undefined");
                }
                var jsonData = GetFullEventsFilter();
                Request request = new Request(SubscriptionEndPoint + "/" + _subscription.Id, jsonData);
                ApiResponse response = _platform.Put(request);
                UpdateSubscription(JsonConvert.DeserializeObject<Subscription>(response.GetBody()));
                return response;
            }
            catch (Exception e)
            {
                Unsubscribe();
                throw e;
            }
        }

        public ApiResponse Remove()
        {
            if (_subscription == null || string.IsNullOrEmpty(_subscription.Id))
            {
                throw new Exception("Subscription ID is required");
            }
            try
            {
                Request request = new Request(SubscriptionEndPoint + "/" + _subscription.Id);
                ApiResponse response = _platform.Delete(request);
                Unsubscribe();
                return response;
            }
            catch (Exception e)
            {
                Unsubscribe();
                throw e;
            }
        }

        public void UpdateSubscription(Subscription subscription)
        {
            ClearTimeout();
            _subscription = subscription;
            subscribed = true;
            SetTimeout();
        }

        public ApiResponse Subscribe(Action<object> userCallback, Action<object> connectCallback, Action<object> errorCallback)
        {
            if (eventFilters.Count == 0)
            {
                throw new Exception("Event filters are undefined");
            }
            if (userCallback != null)
            {
                notificationAction = userCallback;
            }
            if (connectCallback != null)
            {
                connectionAction = connectCallback;
            }
            if (errorCallback != null)
            {
                errorAction = errorCallback;
            }
            try
            {
                var jsonData = GetFullEventsFilter();
                Request request = new Request(SubscriptionEndPoint, jsonData);
                ApiResponse response = _platform.Post(request);
                _subscription = JsonConvert.DeserializeObject<Subscription>(response.GetBody());
                if (_subscription.DeliveryMode.Encryption)
                {
                    PubNubServiceImplementation("", _subscription.DeliveryMode.SubscriberKey,
                        _subscription.DeliveryMode.SecretKey, _subscription.DeliveryMode.EncryptionKey);
                }
                else
                {
                    PubNubServiceImplementation("", _subscription.DeliveryMode.SubscriberKey);
                }
                Subscribe(_subscription.DeliveryMode.Address, "", NotificationReturnMessage, SubscribeConnectStatusMessage, ErrorMessage);
                subscribed = true;
                SetTimeout();
                return response;
            }
            catch (Exception e)
            {
                Unsubscribe();
                throw e;
            }
        }

        public void Unsubscribe()
        {
            ClearTimeout();
            if (_pubnub != null)
            {
                Unsubscribe(_subscription.DeliveryMode.Address, "", NotificationReturnMessage,
                    SubscribeConnectStatusMessage, DisconnectMessage, ErrorMessage);
            }
            _subscription = new Subscription();
            ClearEvents();
            subscribed = false;
        }

        private string GetFullEventsFilter()
        {
            var fullEventsFilter = "{ \"eventFilters\": ";
            string eventFiltersToString = "[ ";
            foreach (string filter in eventFilters)
            {
                eventFiltersToString += ("\"" + filter + "\",");
            }
            eventFiltersToString = eventFiltersToString.TrimEnd(',');
            eventFiltersToString += "]";
            fullEventsFilter += (eventFiltersToString + ", \"deliveryMode\" : { \"transportType\" : \"PubNub\" } }");
            return fullEventsFilter;
        }

        private void PubNubServiceImplementation(string publishKey, string subscribeKey)
        {
            if (_enableSSL)
            {
                _pubnub = new Pubnub(publishKey, subscribeKey, "", "", _enableSSL);
            }
            else
            {
                _pubnub = new Pubnub(publishKey, subscribeKey);
            }
            GC.KeepAlive(_pubnub);
        }

        public void PubNubServiceImplementation(string publishKey, string subscribeKey, string secretKey, string cipherKey)
        {
            _encrypted = true;
            _pubnub = new Pubnub(publishKey, subscribeKey, secretKey, "", _enableSSL);
            GC.KeepAlive(_pubnub);
        }

        public void Subscribe(string channel, string channelGroup, Action<object> subscribeCallback,
            Action<object> connectCallback, Action<object> errorCallback)
        {
            _pubnub.Subscribe<string>(channel, channelGroup, subscribeCallback,
                connectCallback, errorCallback);
        }


        // todo: why channelGroup
        public void Unsubscribe(string channel, string channelGroup, Action<object> subscribeCallback,
            Action<object> connectCallback, Action<object> disconnectCallback, Action<object> errorCallback)
        {
            _pubnub.Unsubscribe(channel, subscribeCallback, connectCallback,
                disconnectCallback, errorCallback);
        }

        private void NotificationReturnMessage(object message)
        {
            if (_encrypted)
            {
                _events["notification"] = DecryptMessage(message);
            }
            else
            {
                _events["notification"] = JsonConvert.DeserializeObject((string)message);
            }
            if (notificationAction != null)
            {
                notificationAction(_events["notification"]);
            }
            Debug.WriteLine("Subscribe Message: " + message);
        }

        private void SubscribeConnectStatusMessage(object message)
        {
            _events["connectMessage"] = JsonConvert.DeserializeObject((string)message);
            if (connectionAction != null)
            {
                connectionAction(_events["connectMessage"]);
            }
            Debug.WriteLine("Connect Message: " + message);
        }

        private void ErrorMessage(object message)
        {
            _events["errorMessage"] = message;
            if (errorAction != null)
            {
                errorAction(_events["errorMessage"]);
            }
            Debug.WriteLine("Error Message: " + message);
        }

        private void DisconnectMessage(object message)
        {
            //Disconnect does not return JSON, it returns list of strings. Only need [1]
            var seperatedMessage = (List<object>)message;
            _events["disconnectMessage"] = seperatedMessage[1].ToString();
            if (disconnectAction != null)
            {
                disconnectAction(_events["disconnectMessage"]);
            }
            Debug.WriteLine("Disconnect Message: " + message);
        }

        private object ReturnMessage(string requestedMessage)
        {
            if (_events.ContainsKey(requestedMessage))
            {
                return _events[requestedMessage];
            }
            return "Error: Message not found";
        }

        private object DecryptMessage(object message)
        {
            var deserializedMessage = JsonConvert.DeserializeObject<List<string>>(message.ToString());
            byte[] keyArray = Convert.FromBase64String(_subscription.DeliveryMode.EncryptionKey);
            byte[] messageData = Convert.FromBase64String(deserializedMessage[0]);
            var encyptionProvider = WinRTCrypto.SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithm.AesEcbPkcs7);
            var key = encyptionProvider.CreateSymmetricKey(keyArray);
            byte[] decodedMessage = WinRTCrypto.CryptographicEngine.Decrypt(key, messageData);
            return Encoding.UTF8.GetString(decodedMessage, 0, decodedMessage.Length);
        }

        public void EnableSSL(bool enableSSL)
        {
            _enableSSL = enableSSL;
        }

        public bool IsSSL()
        {
            return _enableSSL;
        }
    }
}
