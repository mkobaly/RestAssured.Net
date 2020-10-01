using System;
using NUnit.Framework;

namespace RA.Tests
{
    [TestFixture]
    public class MockSetupContextWithJson
    {
        public SetupContext _setup;
        private string _id;
        private string _name1;
        private string _name2;

        public MockSetupContextWithJson()
        {
            _id = Guid.Parse("4fc99dd1-9aad-4d63-a247-75c9b72de204").ToString();
            _name1 = "Foo";
            _name2 = "f00Baz";
            var body = new
            {
                id = _id,
                company1 = new
                {
                    name = _name1
                },
                company2 = new
                {
                    name = _name2
                }
            };

            _setup = new SetupContext();
            _setup.Body(body);
        }

        [Test]
        public void BodySerializationMatch()
        {
            Assert.AreEqual(_setup.Body(),
                "{\"id\":\"4fc99dd1-9aad-4d63-a247-75c9b72de204\",\"company1\":{\"name\":\"Foo\"},\"company2\":{\"name\":\"f00Baz\"}}");
        }

        [Test]
        public void UriWithPort()
        {
           _setup
                .Host("http://localhost")
                .Port(1500)
                .Uri("/test");

            var httpContext = _setup.When();
            httpContext.SetUrl(null);

            Assert.AreEqual("http://localhost:1500", _setup.Host());
            Assert.AreEqual("http://localhost:1500/test", httpContext.Url());
        }

        [Test]
        public void UriWithModulePort()
        {
            _setup
                 .Host("http://localhost")
                 .Port(1500)
                 .ModuleUri("/abc.api")
                 .Uri("/test");

            var httpContext = _setup.When();
            httpContext.SetUrl(null);

            Assert.AreEqual("http://localhost:1500", _setup.Host());
            Assert.AreEqual("http://localhost:1500/abc.api/test", httpContext.Url());
        }
    }
}