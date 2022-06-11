using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Patterns;
using Newtonsoft.Json;
using ServerMocker;
using ServerMocker.Models;
using ServerMocker.OpenApiConversion;

public class Program 
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length < 1 || !new[] { "mock", "gen", "help" }.Contains(args[0]))
        {
            PrintUsage();
            return 1;
        }
        if (args[0] == "help")
        {
            PrintUsage();
            return 0;
        }
        if (args[0] == "mock")
        {
            int? port = null;
            if (args.Where(x => !x.StartsWith("--")).Count() != 2)
            {
                PrintUsage();
                return 1;
            }
            var portArg = args.FirstOrDefault(x => x.StartsWith("--port="));
            if (portArg != null)
            {
                var split = portArg.Split("=");
                if (split.Length != 2)
                {
                    PrintUsage();
                    return 1;
                }
                if (!int.TryParse(split[1], out int portVal))
                {
                    Console.Error.WriteLine($"invalid port '{split[1]}', port must be an int");
                    PrintUsage();
                    return 1;
                }
                port = portVal;
            }
            FileInfo specFile = new FileInfo(args[1]);
            try { 
                await RunMocker(specFile, port, args.Contains("--https"));
            }
            catch(JsonSerializationException ex)
            {
                Console.Error.WriteLine($"Endpoints file {specFile.Name} is invalid ");
                Console.Error.WriteLine("at path "+ex.Path);
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is FileNotFoundException)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }
        if (args[0] == "gen")
        {
            if (args.Length != 2)
            {
                PrintUsage();
                return 1;
            }
            var uri = new Uri(args[1]);
            try
            {
                await GenerateMockSpecFile(uri);
            }
            catch(Exception ex) when (ex is FileNotFoundException|| ex is HttpRequestException)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }
        return 0;
    }
    static async Task GenerateMockSpecFile(Uri openApiSpecUrl)
    {
        FileInfo resultFile;
        if (openApiSpecUrl.IsFile || openApiSpecUrl.IsUnc)
        {
            resultFile = await GenerateMockSpecFileFromFile(openApiSpecUrl.ToString());
        }
        else
        {
            resultFile = await GenerateMockSpecFileFromWeb(openApiSpecUrl);
        }
        Console.WriteLine("Wrote ServerMocker spec to " + resultFile.FullName);
    }
    static async Task<FileInfo> GenerateMockSpecFileFromWeb(Uri openApiSpecFetchUrl)
    {
        HttpClient httpClient = new HttpClient();
        var resp=await httpClient.GetAsync(openApiSpecFetchUrl);
        string content = "(unable to fetch content)";
        try
        {
            content = await resp.Content.ReadAsStringAsync();
        }
        catch (Exception) { }
        if (!resp.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"failed to fetch {openApiSpecFetchUrl}, got {resp.StatusCode}: {content}");
        }
        return await ProcessOpenApiToEndpointSpecFile(content);
    }
    static async Task<FileInfo> GenerateMockSpecFileFromFile(string filePath)
    {
        var file=new FileInfo(filePath);
        if (!file.Exists)
        {
            throw new FileNotFoundException(file.FullName);
        }
        return await ProcessOpenApiToEndpointSpecFile(await File.ReadAllTextAsync(file.FullName));
    }
    static async Task<FileInfo> ProcessOpenApiToEndpointSpecFile(string openApiJson)
    {
        var spec=EndpointSpecConverter.ConvertOpenApiDocument(/*JsonConvert.DeserializeObject<OpenAPI>(openApiJson)*/);
        var output = new FileInfo("endpoints.json");
        int uniqueNum = 0;
        while (output.Exists)
        {
            uniqueNum++;
            output = new FileInfo("endpoints" + uniqueNum + ".json");
        }
        await File.WriteAllTextAsync(output.FullName,JsonConvert.SerializeObject(spec,Formatting.Indented));
        return output;
    }
    static async Task RunMocker(FileInfo specFile,int? port,bool useHttps) 
    {
        if (!specFile.Exists)
        {
            throw new FileNotFoundException($"spec file not found: '{specFile.FullName}'");
        }
        var specs = JsonConvert.DeserializeObject<EndpointSpecification[]>(
            await File.ReadAllTextAsync(specFile.FullName),
            new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            }
        );
        if (specs == null)
        {
            throw new ArgumentException("spec file " + specFile.FullName + " parsed to null");
        }
        for (int i = 0; i < specs.Length; i++)
        {
            specs[i].Validate("[" + i + "]");
        }
        var webApp = new MockServerHostBuilder(specs, port, useHttps, Console.Out);
        await webApp.Build().RunAsync();
    }
    static void PrintUsage()
    {
        Console.WriteLine("dotnet run ServerMocker mock|gen|help <args>");
        Console.WriteLine();
        Console.WriteLine("mock <mock specification file path> [--port=<number>] [--https]");
        Console.WriteLine("    Starts the server mocker with the provided conf");
        Console.WriteLine("    defaults to https on port 5001");
        Console.WriteLine("gen <url to GET OpenAPI 3 specification>");
        Console.WriteLine("    outputs a server mocker configuration file to ./endpoints.json");
        Console.WriteLine("    generated from the OpenApi specification downloaded from the endpoint");
        Console.WriteLine("    If ./endpoints.json already exists, the file will be created as endpoints1.json");
        Console.WriteLine("    If ./endpoints1.json already exists, endpoints2.json and so on");
        Console.WriteLine("gen <file path to saved OpenAPI 3 specification json file>");
        Console.WriteLine("    Does the same as the above options but sources the spec from a file");
        Console.WriteLine("help");
        Console.WriteLine("    Prints this page");
    }

}