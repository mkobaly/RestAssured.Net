using NUnit.Framework;
using RA;
using RestSharp;
using ServiceStack.TestApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceStack.TestApi.Tests
{
    public class RestAssuredTests
    {
        [Test]
        public void Can_call_MyServices()
        {
            var helloRequest = new Hello { Name = "Bob" };
            new RestAssured()
              .Given()
                //Optional, set the name of this suite
                .Name("JsonIP Test Suite")
                //.Host("https://localhost:5001")
                .Header("Content-Type", "application/json")
                .Header("Accept-Encoding", "gzip,deflate")
              .When()
                //.Get("https://localhost:5001/hello")
                .Get(helloRequest)
              .Then()
                //Give the name of the test and a lambda expression to test with
                //The lambda expression keys off of 'x' which represents the json blob as a dynamic.
                //.TestBody("test a", x => x.result != "Hello, Bob!")
                .TestBody<HelloResponse>("foo", x => x.Result == "Hello, Bob!")
                //Throw an AssertException if the test case is false.
                .Assert("foo");

        }

        [Test]
        public void Test_Post()
        {
            var helloRequest = new Hello { Name = "Mary" };
            new RestAssured()
              .Given()
                .Name("post - hello")
              //.Host("https://localhost:5001")
              //.Header("Content-Type", "application/json")
              //.Header("Accept-Encoding", "gzip,deflate")
              .When()
                //.Get("https://localhost:5001/hello")
                .Post(helloRequest)
              .Then()
                  .TestBody<HelloResponse>(x => Assert.AreEqual("Hello, Bob!", x.Result));
                  //.TestBody<HelloResponse>("foo", x => x.Result == "Hello, Bob!")
                //Throw an AssertException if the test case is false.
                //.Assert("foo");
            

        }

        [Test]
        public void TestRestSharp()
        {
            var client = new RestClient("https://localsymmetry.net/AMAG.Auth/Login");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            //request.AddHeader("Cookie", ".AspNetCore.Antiforgery.9TtSrW0hzOs=CfDJ8B3TfLjBRaBGi8YyoZ3XiwByUtZJ-gQbMzZdoIhP3rk2sr47ad4xR5ms7fpw7_Ig9WhZgmGmm8K6JRT3q1B-yom9jx0NDbFhkbwAxko0sk5nPpQNQX2o3awJXK5uyeiCTVgScSs7-bhyH3ATzuuN4sM");
            request.AlwaysMultipartFormData = true;
            request.AddParameter("Username", "admin1234");
            request.AddParameter("Password", "Passw0rd1");
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }

        [Test]
        public void TestLogin1()
        {
            new RestAssured()
             .Given()
               .Name("login")
               .Host("https://localsymmetry.net")
               .Uri("/AMAG.Auth/Login")
               .Param("Username", "admin1234")
               .Param("Password", "Passw0rd1")
               .Cookie("Cookie", ".AspNetCore.Antiforgery.9TtSrW0hzOs=CfDJ8B3TfLjBRaBGi8YyoZ3XiwBkoEnFAwNf4xBX81cGBoWYbvpvHd1i6AHJPPYKQN4ij0f-BjG_QYX3ja5vaL4j-0evtJDM9KoG88J8M9Yx-Q5TH1HE3Rsfojj3PUM4XvxDSIMlzMcj_8_9CEjCfizPoVw; AmagSessionInactivityTimeout=30; G4SLogoutUrl=%2F%2Flocalsymmetry.net%2FAMAG.Auth%2FLogin; G4SIdentitySessionGuid=8d9e1396-48ce-4a61-8786-ac45010655c6")
             .When()
               .Post()
             .Then()
                 .TestBody("SessionData Exists", x => ((string)x).Contains("SessionData"))
                 .Debug()
                 .AssertAll(false);
                 
        }

        [Test]
        public void TestLogin()
        {
            //string cookie = null;
            //new RestAssured()
            //  .Given()
            //    .Name("login")
            //    .Host("https://localsymmetry.net")
            //    .Uri("/AMAG.Auth/Login")
            //  //.Header("Content-Type", "application/json")
            //  //.Header("Accept-Encoding", "gzip,deflate")
            //  .When()
            //    .Get()
            //  .Then()
            //      //.TestHeader((x) => { cookie = x.HeaderValue("cookie"); })
            //      .TestResponse((r) => { cookie = r.Headers.GetValues("Set-Cookie").FirstOrDefault().Split(";").FirstOrDefault(); });
            //// .Debug();

            ////Console.WriteLine($"Cookie: {cookie}");
            ////.TestBody("Should contain cookies", resp => TestCookieResponse(resp))
            ////.Assert("Should contain cookies")

            new RestAssured()
              .Given()
                .Name("login")
                .Host("https://localsymmetry.net")
                .Uri("/AMAG.Auth/Login")
                .Param("Username", "admin1234")
                .Param("Password", "Passw0rd1")
                //.Cookie("Cookie", cookie)
              //.Header("Cookie", cookie)
              //.Header("Content-Type", "application/json")
              //.Header("Content-Type", "application/json")
              //.Header("Accept-Encoding", "gzip,deflate")
              .When()
                .Post()
              .Then()
                  //.TestBody("SessionData Exists", x => x.ToString() != null)
                  .TestBody("SessionData Exists", x => ((string)x).Contains("SessionData"))
                  .Debug()
                  .AssertAll(false);



        }

        //
    }
}
