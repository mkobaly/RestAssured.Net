using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using RA.Enums;
using RA.Extensions;
using RA.Net;

namespace RA
{
    public class ExecutionContext
    {
        private readonly SetupContext _setupContext;
        private readonly HttpActionContext _httpActionContext;
        private readonly HttpClient _httpClient;

        public ExecutionContext(SetupContext setupContext, HttpActionContext httpActionContext)
        {
            _setupContext = setupContext;
            _httpActionContext = httpActionContext;

            _httpClient = _setupContext.HttpClient();
        }

        public ResponseContext<T> Then<T>()
        {
            try
            {
                var response = ExecuteCall().GetAwaiter().GetResult();
                return BuildFromResponse<T>(response);
            }
            catch (OperationCanceledException cex)
            {

                throw new Exception("api call exceeded timeout threshold", cex);
            }
        }

        public ResponseContext<dynamic> Then()
        {
            try
            {
                var response = ExecuteCall().GetAwaiter().GetResult();
                return BuildFromResponse<dynamic>(response);
            }
            catch (OperationCanceledException cex)
            {

                throw new Exception("api call exceeded timeout threshold", cex);
            }
           
        }

        #region HttpAction Strategy
        private HttpRequestMessage BuildRequest()
        {
            switch (_httpActionContext.HttpAction())
            {
                case HttpActionType.GET:
                    return BuildGet();
                case HttpActionType.POST:
                    return BuildPost();
                case HttpActionType.PUT:
                    return BuildPut();
                case HttpActionType.PATCH:
                    return BuildPatch();
                case HttpActionType.DELETE:
                    return BuildDelete();
                default:
                    throw new Exception("should not have gotten here");
            }
        }

        private HttpRequestMessage BuildGet()
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = BuildUri(),
                Method = HttpMethod.Get
            };

            AppendHeaders(request);
	        AppendCookies(request);
	        //SetTimeout();

            request.Content = BuildContent();

            return request;
        }

        private HttpRequestMessage BuildPost()
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = BuildUri(),
                Method = HttpMethod.Post
            };

            AppendHeaders(request);
	        AppendCookies(request);
	        //SetTimeout();

			request.Content = BuildContent();

            return request;
        }

        private HttpRequestMessage BuildPut()
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = BuildUri(),
                Method = HttpMethod.Put
            };

            AppendHeaders(request);
	        AppendCookies(request);
	        //SetTimeout();

			request.Content = BuildContent();

            return request;
        }

        private HttpRequestMessage BuildPatch()
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = BuildUri(),
                Method = new HttpMethod("PATCH")
            };

            AppendHeaders(request);
	        AppendCookies(request);
	        //SetTimeout();

			request.Content = BuildContent();

            return request;
        }

        private HttpRequestMessage BuildDelete()
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = BuildUri(),
                Method = HttpMethod.Delete
            };

            AppendHeaders(request);
	        AppendCookies(request);
	        //SetTimeout();

			request.Content = BuildContent();

            return request;
        }

        private Uri BuildUri()
        {
            var builder = new UriBuilder(_httpActionContext.Url());
            var query = HttpUtility.ParseQueryString(builder.Query);

            foreach (var queryString in _setupContext.Queries())
            {
                query.Add(queryString.Key, queryString.Value);
            }

            builder.Query = query.ToString();
            return new Uri(builder.ToString());
        }

        private void AppendHeaders(HttpRequestMessage request)
        {
            // httpclient default headers must be used because otherwise no value like "application/json;version=1" is allowed.
            _setupContext.HeaderAccept().ForEach(x => _httpClient.DefaultRequestHeaders.Add(HeaderType.Accept.Value, x));
            _setupContext.HeaderAcceptEncoding().ForEach(x => request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue(x)));
            _setupContext.HeaderAcceptCharset().ForEach(x => request.Headers.AcceptCharset.Add(new StringWithQualityHeaderValue(x)));
            _setupContext.HeaderForEverythingElse().ForEach(x => request.Headers.Add(x.Key, x.Value));
        }

	    private void AppendCookies(HttpRequestMessage request)
	    {
		    request.Headers.Add("Cookie", string.Join(";", _setupContext.Cookies().Select(x => x.Key + "=" + x.Value)));
		}

	   // private void SetTimeout()
	   // {
		  //  if (_setupContext.Timeout().HasValue)
		  //  {
				//_httpClient.Timeout = new TimeSpan(_setupContext.Timeout().Value.Ticks);
		  //  }
	   // }

        #endregion

        #region Content Buildup Strategy

        private HttpContent BuildContent()
        {
            if (_setupContext.Files().Any())
                return BuildMultipartContent();
            if (_setupContext.Params().Any())
                return BuildFormContent();
            if (!string.IsNullOrEmpty(_setupContext.Body()))
                return BuildStringContent();

            return null;
        }

        private HttpContent BuildMultipartContent()
        {
            var content = new MultipartFormDataContent();

            _setupContext.Params().ForEach(x => content.Add(new StringContent(x.Value), x.Key.Quote()));

            _setupContext.Files().ForEach(x =>
            {
                var fileContent = new ByteArrayContent(x.Content);
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    Name = x.ContentDispositionName.Quote(),
                    FileName = x.FileName
                };
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(x.ContentType);

                content.Add(fileContent);
            });

            return content;
        }

        private HttpContent BuildFormContent()
        {
            return new FormUrlEncodedContent(
                _setupContext.Params().Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToList());
        }

        private HttpContent BuildStringContent()
        {
            return new StringContent(_setupContext.Body(), Encoding.UTF8,
                _setupContext.HeaderContentType().FirstOrDefault());
        }

        #endregion

       
        private async Task<HttpResponseMessageWrapper> ExecuteCall()
        {
            CancellationToken cancellationToken = CancellationToken.None;
            if (_setupContext.Timeout().HasValue)
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(_setupContext.Timeout().Value);
                cancellationToken = cts.Token;
            }

            var watch = new Stopwatch();
            watch.Start();
            var response = await _httpClient.SendAsync(BuildRequest(), cancellationToken);
            watch.Stop();
            return new HttpResponseMessageWrapper { ElaspedExecution = watch.Elapsed, Response = response };
        }

        private ResponseContext<T> BuildFromResponse<T>(HttpResponseMessageWrapper result)
        {
            // var content = AsyncContext.Run(async () => await result.Response.Content.ReadAsStringAsync());
            //var content = result.Response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return new ResponseContext<T>(result.Response, result.ElaspedExecution);
        }

        /// <summary>
        /// Output all debug values from the setup context.
        /// </summary>
        /// <returns></returns>
        public ExecutionContext Debug()
        {
            var uri = BuildUri();

            "host".WriteHeader();
            uri.Host.WriteLine();
            "absolute path".WriteHeader();
            uri.AbsolutePath.WriteLine();
            "absolute uri".WriteHeader();
            uri.AbsoluteUri.WriteLine();
            "authority".WriteHeader();
            uri.Authority.WriteLine();
            "path and query".WriteHeader();
            uri.PathAndQuery.WriteLine();
            "original string".WriteHeader();
            uri.OriginalString.WriteLine();
            "scheme".WriteHeader();
            uri.Scheme.WriteLine();

            return this;
        }
    }
}