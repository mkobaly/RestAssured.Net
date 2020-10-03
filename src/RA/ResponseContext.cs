using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using RA.Enums;
using RA.Extensions;
using System.Net.Http;
using ServiceStack;
using ServiceStack.Text;
using System.Xml.Linq;

namespace RA
{
    public class ResponseContext<T>
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;
        private T _parsedContent;
        private readonly Dictionary<string, IEnumerable<string>> _headers = new Dictionary<string, IEnumerable<string>>();
        private TimeSpan _elapsedExecutionTime;      
        private readonly HttpResponseMessage _response;

        public ResponseContext(HttpResponseMessage response, TimeSpan elaspedExecutionTime) 
        {
            _response = response;
            _content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            
            _statusCode = response.StatusCode;
            _headers = response.Content.Headers.ToDictionary(x => x.Key.Trim(), x => x.Value);
            _elapsedExecutionTime = elaspedExecutionTime;
            Parse();
        }

        /// <summary>
        /// Retrieve an deserialized object from the response document.
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public T Retrieve()
        {
            return _parsedContent;
        }

        public ResponseContext<T> TestBody(Action<T> action)
        {
            action.Invoke(_parsedContent);
            return this;
        }

        public ResponseContext<T> TestHeader(string headerKey, Action<string> action)
        {
            var result = HeaderValue(headerKey);
            action.Invoke(result);
            return this;
        }

        public ResponseContext<T> TestResponse(Action<HttpResponseMessage> action)
        {
            action.Invoke(_response);
            return this;
        }


        public ResponseContext<T> TestElaspedTime(Action<double> action)
        {
            action.Invoke(_elapsedExecutionTime.TotalMilliseconds);
            return this;
            //return TestWrapper(ruleName, () => func.Invoke(_elapsedExecutionTime.TotalMilliseconds));
        }


        /// <summary>
        /// Setup a test against the response status
        /// eg: OK 200 or Error 400
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public ResponseContext<T> TestStatus(Action<HttpStatusCode> action)
        {
            action.Invoke(_statusCode);
            return this;
        }


        private void Parse()
        {
            var contentType = ContentType();

            if (contentType.Contains("json"))
            {
                if (!string.IsNullOrEmpty(_content))
                {
                    
                    _parsedContent = ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(_content);
                    return;
                    
                }
                else
                {
                    return;
                }
            }
            //else if (contentType.Contains("xml"))
            //{
            //    if (!string.IsNullOrEmpty(_content))
            //    {
            //        try
            //        {
            //            _parsedContent = XDocument.Parse(_content);
            //            return;
            //        }
            //        catch
            //        {
            //        }
            //    }
            //}

            _parsedContent = (dynamic)_content;
        }

       
        private string ContentType()
        {
            return HeaderValue(HeaderType.ContentType.Value);
        }

        public string HeaderValue(string key)
        {
            return _headers.Where(x => x.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    .Select(x => string.Join(", ", x.Value))
                    .DefaultIfEmpty(string.Empty)
                    .FirstOrDefault();
        }

        /// <summary>
        /// Output all debug values from the setup context.
        /// </summary>
        /// <returns></returns>
        public ResponseContext<T> Debug()
        {
            "status code".WriteHeader();
            ((int)_statusCode).ToString().WriteLine();

            "response headers".WriteHeader();
            foreach (var header in _headers)
            {
                "{0} : {1}".WriteLine(header.Key, string.Join(", ", header.Value));
            }

            "content".WriteHeader();
            if (this.ContentType().Contains("json"))
            {
                "{0}\n".Write(JsvFormatter.Format(_content));
            }
            //else if (this.ContentType().Contains("xml"))
            //{
            //    XDocument doc = XDocument.Parse(_content);
            //    "{0}\n".Write(JsvFormatter.Format(doc.ToString()));
            //}
            "{0}\n".Write(_content);


            return this;
        }
    }
}