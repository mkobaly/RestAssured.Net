using System;
using ServiceStack;
using ServiceStack.TestApi.ServiceModel;

namespace ServiceStack.TestApi.ServiceInterface
{
    public class MyServices : Service
    {
        public object Any(Hello request)
        {
            return new HelloResponse { Result = $"Hello, {request.Name}!" };
        }
    }
}
