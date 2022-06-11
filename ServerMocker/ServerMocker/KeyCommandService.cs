using Microsoft.Extensions.Hosting;

namespace ServerMocker
{
    public class KeyCommand
    {
        public string CommandDescription { get; set; }
        public Action KeyAction { get; set; }
    }
    public class KeyCommandService : BackgroundService
    {
        private readonly Dictionary<char, KeyCommand> commands;
        public KeyCommandService(Dictionary<char,KeyCommand> commands)
        {
            this.commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }
        Task listenForSequenceReset = Task.CompletedTask;
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach(var commandMapping in commands)
            {
                Console.WriteLine($"press {commandMapping.Key} to {commandMapping.Value.CommandDescription}");
            }
            listenForSequenceReset = Task.Run(async () =>
                {
                    ConsoleKeyInfo key = default;
                    Console.Write("\r \r");
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        if(commands.TryGetValue(key.KeyChar,out var command))
                        {
                            command.KeyAction.Invoke();
                            key = default;
                        }
                        if (Console.KeyAvailable)
                        {
                            key=Console.ReadKey();
                            Console.Write("\r \r");
                        }
                        else
                        {
                            await Task.Delay(200,stoppingToken);
                        }
                    }
                },
                stoppingToken
            );
            return listenForSequenceReset;
        }
    }
}
