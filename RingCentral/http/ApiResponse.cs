﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace RingCentral.Http
{
    public class ApiResponse : Headers
    {
        private readonly string _body;
        private readonly int _status;
        private readonly HttpResponseMessage _response;

        public ApiResponse(HttpResponseMessage response)
        {
            var statusCode = Convert.ToInt32(response.StatusCode);
            var body = response.Content.ReadAsStringAsync().Result;
            var headers = response.Content.Headers;

            _body = body;
            _status = statusCode;
            _response = response;
            SetHeaders(headers);

            if (!CheckStatus())
            {
                throw new Exception(GetError());
            }
        }

        public HttpResponseMessage Response
        {
            get
            {
                return _response;
            }
        }

        public HttpRequestMessage Request
        {
            get
            {
                return _response.RequestMessage;
            }
        }

        /// <summary>
        ///     Checks to be sure the status code is greater than or equal to 200 and less than 300
        /// </summary>
        /// <returns>bool value if status code is successful</returns>
        public bool CheckStatus()
        {
            return _status >= 200 && _status < 300;
        }

        /// <summary>
        ///     returns raw http response body
        /// </summary>
        /// <returns>string http response body</returns>
        public string GetBody()
        {
            return _body;
        }

        /// <summary>
        ///     If header content is JSON it will return full formed and parsed json
        /// </summary>
        /// <returns>A JObject parsed body</returns>
        public JObject GetJson()
        {
            if (!IsJson())
            {
                throw new Exception("Response is not JSON");
            }

            return JObject.Parse(_body);
        }

        /// <summary>
        ///     Determines if the response is multipart
        /// </summary>
        /// <returns>boolean value based on response being multipart</returns>
        public bool IsMultiPartResponse()
        {
            return IsMultiPart();
        }

        /// <summary>
        ///     Parses a multipart response into List of responses that can be accessed by index.
        /// </summary>
        /// <returns>A List of responses from a multipart response</returns>
        public List<string> GetMultiPartResponses()
        {
            var output = Regex.Split(_body, "--Boundary([^;]+)");

            var splitString = output[1].Split(new[] { "--" }, StringSplitOptions.None);

            var responses = new List<string>();

            //We Can convert this to linq but for the sake of readability we'll leave it like this.
            foreach (var s in splitString)
            {
                if (s.Contains("{"))
                {
                    var json = s.Substring(s.IndexOf('{'));

                    JToken token = JObject.Parse(json);

                    responses.Add(token.ToString());
                }
            }

            return responses;
        }

        /// <summary>
        ///     Gets the repsonse status
        /// </summary>
        /// <returns>response status code</returns>
        public int GetStatus()
        {
            return _status;
        }

        /// <summary>
        ///     Gets error if status code is outside the range of values checked in <c>CheckStatus()</c>
        /// </summary>
        /// <returns></returns>
        public string GetError()
        {
            if (CheckStatus())
            {
                return null;
            }

            var message = "Unknown Error";

            var data = GetJson();

            if (!string.IsNullOrEmpty((string)(data["message"])))
            {
                message = (string)(data["message"]);
            }
            if (!string.IsNullOrEmpty((string)(data["error_description"])))
            {
                message = (string)(data["error_description"]);
            }
            if (!string.IsNullOrEmpty((string)(data["description"])))
            {
                message = (string)(data["description"]);
            }
            return message;
        }
    }
}