using System;
using System.Collections.Concurrent;
using RA.Extensions;
using ServiceStack;

namespace RA
{
    public enum HttpActionType
    {
        GET,
        POST,
        PUT,
        PATCH,
        DELETE
    }

    public class HttpActionContext
    {
        private readonly SetupContext _setupContext;
        private string _url;
        private HttpActionType _httpAction;
      
        public HttpActionContext(SetupContext setupContext)
        {
            _setupContext = setupContext;
        }

        /// <summary>
        /// Return the Http Verb that will be used for the test.
        /// </summary>
        /// <returns></returns>
        public HttpActionType HttpAction()
        {
            return _httpAction;
        }

        /// <summary>
        /// Return the Url
        /// </summary>
        /// <returns></returns>
        public string Url()
        {
            return _url;
        }

              
       
        /// <summary>
        /// Configure test with a GET verb.  The url parameter is optional if similar info was provided through the setup context.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ExecutionContext Get(string url = null)
        {
            return SetHttpAction(url, HttpActionType.GET);
        }

        public ExecutionContext Get(object request)
        {
            var url =  request.ToGetUrl();
            _setupContext.Body(request);
            _setupContext.Uri(url);
            return SetHttpAction(null, HttpActionType.GET);
        }

        /// <summary>
        /// Configure test with a POST verb.  The url parameter is optional if similar info was provided through the setup context.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ExecutionContext Post(string url = null)
        {
            return SetHttpAction(url, HttpActionType.POST);
        }

        public ExecutionContext Post(object request)
        {
            var url = request.ToPostUrl();
            _setupContext.Body(request);
            _setupContext.Uri(url);
            return SetHttpAction(null, HttpActionType.POST);
        }


        /// <summary>
        /// Configure test with a PUT verb.  The url parameter is optional if similar info was provided through the setup context.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ExecutionContext Put(string url = null)
        {
            return SetHttpAction(url, HttpActionType.PUT);
        }

        public ExecutionContext Put(object request)
        {
            var url = request.ToPutUrl();
            _setupContext.Body(request);
            _setupContext.Uri(url);
            return SetHttpAction(null, HttpActionType.PUT);
        }

        /// <summary>
        /// Configure test with a PATCH verb.  The url parameter is optional if similar info was provided through the setup context.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ExecutionContext Patch(string url = null)
        {
            return SetHttpAction(url, HttpActionType.PATCH);
        }

        public ExecutionContext Patch(object request)
        {
            var url = request.ToUrl("PATCH");
            _setupContext.Body(request);
            _setupContext.Uri(url);
            return SetHttpAction(null, HttpActionType.PATCH);
        }

        /// <summary>
        /// Configure test with a DELETE verb.  The url parameter is optional if similar info was provided through the setup context.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ExecutionContext Delete(string url = null)
        {
            return SetHttpAction(url, HttpActionType.DELETE);
        }

        public ExecutionContext Delete(object request)
        {
            var url = request.ToDeleteUrl();
            _setupContext.Body(request);
            _setupContext.Uri(url);
            return SetHttpAction(null, HttpActionType.DELETE);
        }

        /// <summary>
        /// Output all debug values from the setup context.
        /// </summary>
        /// <returns></returns>
        public HttpActionContext Debug()
        {
            "url".WriteLine();
            "- {0}".WriteLine(_url);

            "action".WriteLine();
            "- {0}".WriteLine(_httpAction.ToString());

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        public void SetUrl(string url)
        {
            if (url.IsEmpty() && _setupContext.Host().IsEmpty())
                throw new ArgumentException("url must be provided");

            Uri uri;
            //nothing passed in so take all values from setupContext (host, port, module, uri)
            if (url.IsEmpty())
            {
                uri = new Uri(new Uri(_setupContext.Host()), $"{_setupContext.ModuleUri()}{_setupContext.Uri()}");
                _url = uri.OriginalString;
                return;
            } 

            if (url.ToLower().StartsWith("http")) //user provided full URL so just use that
            {
                uri = new Uri(url);
                _url = uri.OriginalString;
                return;
            } 

            //provided rest url path, so just use host, port and passed in url)
             uri = new Uri(new Uri(_setupContext.Host()), url);
            _url = uri.OriginalString;
        }

        private ExecutionContext SetHttpAction(string url, HttpActionType actionType)
        {
            SetUrl(url);
            _httpAction = actionType;
            return new ExecutionContext(_setupContext, this);
        }
    }
}