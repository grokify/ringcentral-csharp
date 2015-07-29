﻿using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RingCentral.Helper;
using RingCentral.Subscription;

namespace RingCentral.NET40.Test
{
    [TestFixture]
    public class PubNubSubscriptionTest : TestConfiguration
    {
        private const string SubscriptionEndPoint = "/restapi/v1.0/subscription";

        private const string JsonData =
            "{\"eventFilters\": " +
            "[ \"/restapi/v1.0/account/~/extension/~/presence\", " +
            "\"/restapi/v1.0/account/~/extension/~/message-store\" ], " +
            "\"deliveryMode\": " +
            "{ \"transportType\": \"PubNub\", \"encryption\": \"false\" } }";

        private const string SmsEndPoint = "/restapi/v1.0/account/~/extension/~/sms";


        public Task Wait(int milliseconds)
        {
            var tcs = new TaskCompletionSource<object>();
            new Timer(_ => tcs.SetResult(null)).Change(milliseconds, -1);
            return tcs.Task;
        }

        [Test]
        public void SetPubNubSubscription()
        {
            RingCentralClient.GetPlatform().SetJsonData(JsonData);

            string createResult = RingCentralClient.GetPlatform().PostRequest(SubscriptionEndPoint);

            JToken token = JObject.Parse(createResult);

            var id = (string) token.SelectToken("id");

            Assert.IsNotNullOrEmpty(id);

            string getResult = RingCentralClient.GetPlatform().GetRequest(SubscriptionEndPoint + "/" + id);

            var SubscriptionItem = JsonConvert.DeserializeObject<Subscription.Subscription>(getResult);

            Assert.IsNotNull(SubscriptionItem.DeliveryMode.Address);

            SubscriptionServiceImplementation = new SubscriptionServiceImplementation("",
                SubscriptionItem.DeliveryMode.SubscriberKey);

            SubscriptionServiceImplementation.Subscribe(SubscriptionItem.DeliveryMode.Address, "", null, null, null);

            var smsHelper = new SmsHelper(ToPhone, UserName, SmsText);

            string jsonObject = JsonConvert.SerializeObject(smsHelper);

            RingCentralClient.GetPlatform().SetJsonData(jsonObject);

            string result = RingCentralClient.GetPlatform().PostRequest(SmsEndPoint);

            token = JObject.Parse(result);

            var messageStatus = (string) token.SelectToken("messageStatus");

            Assert.AreEqual(messageStatus, "Sent");

            Wait(15000).ContinueWith(_ => Debug.WriteLine("Done"));
        }
    }
}