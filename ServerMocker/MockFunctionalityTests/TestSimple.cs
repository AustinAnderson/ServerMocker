using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using ServerMocker;
using ServerMocker.Models;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MockFunctionalityTests
{
    [TestClass][TestCategory("integrationTests")]
    public class TestSimple
    {
        [TestMethod]
        public async Task MockGET()
        {
            var expected = JToken.FromObject(new
            {
                FirstThing="key1",
                SecondThing = new[] {1,2,3},
                ThirdThing=new
                {
                    SubThing1=1.2,
                    SubThing2="string"
                }
            });
            var endpoint = new EndpointSpecification
            {
                ApiRoute = "/integrationTests/go/3",
                StatusCode = 200,
                Method = SerializableHttpMethodEnum.Get,
                Body = expected
            };
            var harness = new TestHarness(new[] { endpoint });
            harness.StartServer();
            try
            {
                var response = await harness.Request(endpoint.Method, endpoint.ApiRoute, null);
                Assert.AreEqual(endpoint.StatusCode, response.Status);
                Assert.AreEqual(endpoint.Body?.ToString(), response.Body?.ToString());
                harness.AssertOutputContainsMatchingLine(new Regex(".*[Gg][Ee][Tt]\\s+"+endpoint.ApiRoute));
            }
            finally 
            {
                await harness.StopServer();
            }
        }
        [TestMethod]
        public async Task MockPOST()
        {
            var expected = JToken.FromObject(new
            {
                FirstThing="key1",
                SecondThing = new[] {1,2,3},
                ThirdThing=new
                {
                    SubThing1=1.2,
                    SubThing2="string"
                }
            });
            var expectedPostBody = JToken.FromObject(new
            {
                F="inf",
                S= new[] {1,2,3},
                ThirdThing=new
                {
                    SubThing1=1.2,
                    SubThing2="string"
                }
            });
            var endpoint = new EndpointSpecification
            {
                ApiRoute = "/integrationTests/go/3",
                StatusCode = 200,
                Method = SerializableHttpMethodEnum.Post,
                Body = expected
            };
            var harness = new TestHarness(new[] { endpoint });
            harness.StartServer();
            try
            {
                var response = await harness.Request(endpoint.Method, endpoint.ApiRoute, expectedPostBody);
                Assert.AreEqual(endpoint.StatusCode, response.Status);
                Assert.AreEqual(endpoint.Body?.ToString(), response.Body?.ToString());
                harness.AssertOutputContainsMatchingLine(new Regex(".*[Pp][Oo][Ss][Tt]\\s+"+endpoint.ApiRoute));
                harness.AssertOutputContainsMatchingSection(expectedPostBody.ToString());
            }
            finally 
            {
                await harness.StopServer();
            }
        }
        [TestMethod]
        public async Task MockPATCH()
        {
            var expected = JToken.FromObject(new
            {
                FirstThing="key1",
                SecondThing = new[] {1,2,3},
                ThirdThing=new
                {
                    SubThing1=1.2,
                    SubThing2="string"
                }
            });
            var expectedPostBody = JToken.FromObject(new
            {
                F="inf",
                S= new[] {1,2,3},
                ThirdThing=new
                {
                    SubThing1=1.2,
                    SubThing2="string"
                }
            });
            var endpoint = new EndpointSpecification
            {
                ApiRoute = "/integrationTests/go/3",
                StatusCode = 200,
                Method = SerializableHttpMethodEnum.Patch,
                Body = expected
            };
            var harness = new TestHarness(new[] { endpoint });
            harness.StartServer();
            try
            {
                var response = await harness.Request(endpoint.Method, endpoint.ApiRoute, expectedPostBody);
                Assert.AreEqual(endpoint.StatusCode, response.Status);
                Assert.AreEqual(endpoint.Body?.ToString(), response.Body?.ToString());
                harness.AssertOutputContainsMatchingLine(new Regex(".*[Pp][Aa][Tt][Cc][Hh]\\s+"+endpoint.ApiRoute));
                harness.AssertOutputContainsMatchingSection(expectedPostBody.ToString());
            }
            finally 
            {
                await harness.StopServer();
            }
        }
        [TestMethod]
        public async Task MockPUT()
        {
            var expected = JToken.FromObject(new
            {
                FirstThing="key1",
                SecondThing = new[] {1,2,3},
                ThirdThing=new
                {
                    SubThing1=1.2,
                    SubThing2="string"
                }
            });
            var expectedPostBody = JToken.FromObject(new
            {
                F="inf",
                S= new[] {1,2,3},
                ThirdThing=new
                {
                    SubThing1=1.2,
                    SubThing2="string"
                }
            });
            var endpoint = new EndpointSpecification
            {
                ApiRoute = "/integrationTests/go/3",
                StatusCode = 200,
                Method = SerializableHttpMethodEnum.Put,
                Body = expected
            };
            var harness = new TestHarness(new[] { endpoint });
            harness.StartServer();
            try
            {
                var response = await harness.Request(endpoint.Method, endpoint.ApiRoute, expectedPostBody);
                Assert.AreEqual(endpoint.StatusCode, response.Status);
                Assert.AreEqual(endpoint.Body?.ToString(), response.Body?.ToString());
                harness.AssertOutputContainsMatchingLine(new Regex(".*[Pp][Uu][Tt]\\s+"+endpoint.ApiRoute));
                harness.AssertOutputContainsMatchingSection(expectedPostBody.ToString());
            }
            finally 
            {
                await harness.StopServer();
            }
        }
        [TestMethod]
        public async Task MockDELETE()
        {
            var expected = (JToken)"object F deleted";
            var expectedPostBody = JToken.FromObject(new
            {
                F="inf",
                S= new[] {1,2,3},
                ThirdThing=new
                {
                    SubThing1=1.2,
                    SubThing2="string"
                }
            });
            var endpoint = new EndpointSpecification
            {
                ApiRoute = "/integrationTests/go/3",
                StatusCode = 200,
                Method = SerializableHttpMethodEnum.Delete,
                Body = expected
            };
            var harness = new TestHarness(new[] { endpoint });
            harness.StartServer();
            try
            {
                var response = await harness.Request(endpoint.Method, endpoint.ApiRoute, expectedPostBody);
                Assert.AreEqual(endpoint.StatusCode, response.Status);
                Assert.AreEqual(endpoint.Body?.ToString(), response.Body?.ToString());
                harness.AssertOutputContainsMatchingLine(new Regex(".*[Dd][Ee][Ll][Ee][Tt][Ee]\\s+"+endpoint.ApiRoute));
                harness.AssertOutputContainsMatchingSection(expectedPostBody.ToString());
            }
            finally 
            {
                await harness.StopServer();
            }
        }
    }
}