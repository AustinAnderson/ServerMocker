using ServerMocker.Models;

namespace ServerMocker
{
    public class MockServerHostBuilder
    {
        private readonly EndpointSpecification[] specs;
        private readonly int? port;
        private readonly bool useHttps;
        private readonly TextWriter outputWriter;

        public MockServerHostBuilder(EndpointSpecification[] specs, int? port, bool useHttps, TextWriter outputWriter)
        {
            this.specs = specs;
            this.port = port;
            this.useHttps = useHttps;
            this.outputWriter = outputWriter;
        }
        public WebApplication Build()
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.Services.AddSingleton(specs);//same instance so resetter can act on them
            builder.Services.AddSingleton<SequenceStateResetter>();
            builder.Services.AddHostedService<KeyCommandService>();
            builder.Services.AddSingleton(p =>
            {
                var lifeTime=p.GetRequiredService<IHostApplicationLifetime>();
                var seqResetter = p.GetRequiredService<SequenceStateResetter>();
                return new Dictionary<char, KeyCommand>
                {
                    ['r'] = new KeyCommand
                    {
                        CommandDescription = $"reset all {ResponseSequence.Id} state",
                        KeyAction = () =>
                        {
                            seqResetter.ResetSequences();
                            outputWriter.WriteLine(ResponseSequence.Id + "s reset");
                        }
                    },
                    ['q']= new KeyCommand
                    {
                        CommandDescription = $"quit the app",
                        KeyAction = () => lifeTime.StopApplication()
                    }
                };
            });
            var app = builder.Build();
            app.Urls.Add((useHttps?"https":"http")+"://localhost:" + (port ?? 5001));
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                foreach (var endpoint in specs)
                {
                    endpoints.MapMethods(
                        endpoint.ApiRoute,
                        new[] { endpoint.Method.Verb },
                        endpoint.ToRequestDelegate(outputWriter)
                    );
                }
            });
            return app;

        }
    }
}
