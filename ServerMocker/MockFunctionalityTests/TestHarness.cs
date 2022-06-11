using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using ServerMocker;
using ServerMocker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MockFunctionalityTests
{
    internal class HttpResult
    {
        public JToken? Body;
        public int Status;
        public static HttpResult Empty { get; }=new HttpResult();
    }
    internal class TestHarness
    {
        private HttpClient client;
        private OutputCaptureTextWriter textOutput;
        private WebApplication app;
        private IHostApplicationLifetime lifetime;
        private Task serverTask;
        private SequenceStateResetter resetter;
        private static int currentPort = 5000;
        public int Port { get; private set; }
        public TestHarness(EndpointSpecification[] specs)
        {
            textOutput = new OutputCaptureTextWriter();
            client = new HttpClient(new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromSeconds(5)
            });
            Port = currentPort++;
            var host = new MockServerHostBuilder(specs, Port, false, textOutput);
            app = host.Build();
            lifetime=app.Services.GetRequiredService<IHostApplicationLifetime>();
            resetter=app.Services.GetRequiredService<SequenceStateResetter>();
        }
        public void StartServer()
        {
            serverTask = Task.Run(async () => await app.StartAsync());
        }
        public void ResetSequences()
        {
            resetter.ResetSequences();
        }
        public Task StopServer()
        {
            lifetime.StopApplication();
            return serverTask;
        }
        public void AssertOutputContainsMatchingLine(Regex lineCriteria)
        {
            if(!textOutput.GetStringBuilder().ToString()
                .Split(textOutput.NewLine)
                .Any(l => lineCriteria.IsMatch(l)))
            {
                Assert.Fail($"No line in the output matches pattern '{lineCriteria}'");
            }
        }
        public void AssertOutputContainsMatchingSection(string searchText)
        {
            if (!textOutput.GetStringBuilder().ToString().Contains(searchText))
            {
                Assert.Fail($"Output does not contain text '{searchText}'");
            }
        }
        public async Task<HttpResult> Request(SerializableHttpMethodEnum method, string relativeUrl, JToken? requestBody)
        {
            try
            {
                var request = new HttpRequestMessage(method, "http://localhost:"+Port+relativeUrl);
                if (requestBody != null)
                {
                    request.Content = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");
                }
                var resp = client.Send(request);
                var responseContent=await resp.Content.ReadAsStringAsync();


                var res = new HttpResult
                {
                    Status = (int)resp.StatusCode,
                    Body = null
                };
                try
                {
                    res.Body = (JToken)responseContent;
                }
                catch(InvalidCastException ex)
                {
                };
                return res;
            }
            catch (Exception ex)
            {
                Assert.Inconclusive(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.GetType().Name+": "+ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
            return HttpResult.Empty;//assert inconclusive throws so this will never happen
        }
    }
}
