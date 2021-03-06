﻿using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RingCentral.Http;
using System.Collections.Generic;

namespace RingCentral.Test
{
    [TestFixture]
    public class GeographicalDictionaryTest : BaseTest
    {
        private const string DictionaryEndPoint = "/restapi/v1.0/dictionary";
        private const string CountryEndPoint = DictionaryEndPoint + "/country";
        private const string StateEndPoint = DictionaryEndPoint + "/state";
        private const string LocationEndPoint = DictionaryEndPoint + "/location";
        private const string TimeZoneEndPoint = DictionaryEndPoint + "/timezone";
        private const string LanguageEndPoint = DictionaryEndPoint + "/language";


        [Test]
        public void GetCountries()
        {
            Request request = new Request(CountryEndPoint);
            ApiResponse response = sdk.Platform.Get(request);

            JToken token = response.GetJson();

            var countryName = (string)token.SelectToken("records")[0].SelectToken("name");

            Assert.AreEqual(countryName, "Afghanistan");
        }

        [Test]
        public void GetCountryById()
        {
            Request request = new Request(CountryEndPoint + "/3");
            ApiResponse response = sdk.Platform.Get(request);

            JToken token = response.GetJson();

            var countryName = (string)token.SelectToken("name");

            Assert.AreEqual(countryName, "Albania");
        }

        [Test]
        public void GetLanguage()
        {
            Request request = new Request(LanguageEndPoint);
            ApiResponse response = sdk.Platform.Get(request);

            JToken token = response.GetJson();

            var languageName = (string)token.SelectToken("records")[0].SelectToken("name");

            Assert.AreEqual(languageName, "English (United States)");
        }

        [Test]
        public void GetLanguagesById()
        {
            const string languageId = "1033";

            Request request = new Request(LanguageEndPoint + "/" + languageId);
            ApiResponse response = sdk.Platform.Get(request);

            JToken token = response.GetJson();

            var timeZoneName = (string)token.SelectToken("name");

            Assert.AreEqual(timeZoneName, "English (United States)");
        }

        [Test]
        public void GetLocation()
        {


            var queryParameters = new List<KeyValuePair<string, string>>
                                  {
                                      new KeyValuePair<string, string>(
                                          "stateId", "13")
                                  };

            Request request = new Request(LocationEndPoint, queryParameters);

            ApiResponse response = sdk.Platform.Get(request);

            JToken token = response.GetJson();

            var city = (string)token.SelectToken("records")[0].SelectToken("city");

            Assert.AreEqual(city, "Anchorage");
        }

        [Test]
        public void GetStateById()
        {
            const string stateId = "13";
            Request request = new Request(StateEndPoint + "/" + stateId);
            ApiResponse response = sdk.Platform.Get(request);

            JToken token = response.GetJson();

            var stateName = (string)token.SelectToken("name");

            Assert.AreEqual(stateName, "Alaska");
        }

        [Test]
        public void GetStates()
        {

            var queryParameters = new List<KeyValuePair<string, string>>
                                  {
                                      new KeyValuePair<string, string>("countryId", "1"),
                                      new KeyValuePair<string, string>("withPhoneNumbers", "True"),
                                      new KeyValuePair<string, string>("perPage", "2")
                                  };

            Request request = new Request(StateEndPoint, queryParameters);

            ApiResponse response = sdk.Platform.Get(request);

            JToken token = response.GetJson();

            var stateName = (string)token.SelectToken("records")[0].SelectToken("name");

            Assert.AreEqual(stateName, "Alabama");
        }

        [Test]
        public void GetTimeZoneById()
        {
            Request request = new Request(TimeZoneEndPoint + "/1");

            ApiResponse response = sdk.Platform.Get(request);

            JToken token = response.GetJson();

            var timeZoneName = (string)token.SelectToken("name");

            Assert.AreEqual(timeZoneName, "GMT");
        }

        [Test]
        public void GetTimeZones()
        {
            Request request = new Request(TimeZoneEndPoint);
            ApiResponse response = sdk.Platform.Get(request);

            JToken token = response.GetJson();

            var stateName = (string)token.SelectToken("records")[0].SelectToken("name");

            Assert.AreEqual(stateName, "GMT");
        }
    }
}