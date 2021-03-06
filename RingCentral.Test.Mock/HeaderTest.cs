﻿using NUnit.Framework;
using RingCentral.Http;

namespace RingCentral.Test
{
    [TestFixture]
    public class HeaderTest : BaseTest
    {
        protected const string AccountInformationEndPoint = "/restapi/v1.0/account/";

        [Test]
        public void GetHeaders()
        {
            Request request = new Request(AccountInformationEndPoint);
            ApiResponse response = sdk.Platform.Get(request);
            Assert.IsNotNull(response.GetHeaders());
            Assert.AreEqual("application/json; charset=utf-8", response.GetHeaders().ContentType.ToString());
        }

        [Test]
        public void GetUrlEncoded()
        {
            Request request = new Request(AccountInformationEndPoint);
            ApiResponse response = sdk.Platform.Get(request);
            Assert.IsFalse(response.IsUrlEncoded());
        }
    }
}
