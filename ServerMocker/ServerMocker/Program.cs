using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Patterns;
using Newtonsoft.Json;
using ServerMocker;
using ServerMocker.OpenApiConversion;

if (args.Length < 1  || !new[] { "mock", "gen", "help" }.Contains(args[0]))
{
    PrintUsage();
    return;
}
if(args[0] == "help")
{
    PrintUsage();
    return;
}
if(args[0] == "mock")
{
    if(args.Length != 2)
    {
        PrintUsage();
        return;
    }
    await RunMocker(args[1]);
    return;
}
if(args[0] == "gen")
{
    if (args.Length != 2)
    {
        PrintUsage();
        return;
    }
    var uri = new Uri(args[1]);
    string path = "";
    if (uri.IsFile)
    {
        path = await GenerateMockSpecFileFromFile(args[1]);
    }
    else
    {
        path = await GenerateMockSpecFile(uri);
    }
    Console.WriteLine("Wrote ServerMocker spec to " + path);
    return;
}
static async Task<string> GenerateMockSpecFile(Uri openApiSpecFetchUrl)
{
    HttpClient httpClient = new HttpClient();
    var resp=await httpClient.GetAsync(openApiSpecFetchUrl);
    var content=await resp.Content.ReadAsStringAsync();
    if (!resp.IsSuccessStatusCode)
    {
        throw new HttpRequestException($"failed to fetch {openApiSpecFetchUrl}, got {resp.StatusCode}: {content}");
    }
    return await ProcessOpenApiToEndpointSpecFile(content);
}
static async Task<string> GenerateMockSpecFileFromFile(string filePath)
{
    var file=new FileInfo(filePath);
    if (!file.Exists)
    {
        throw new FileNotFoundException(file.FullName);
    }
    return await ProcessOpenApiToEndpointSpecFile(await File.ReadAllTextAsync(file.FullName));
}
static async Task<string> ProcessOpenApiToEndpointSpecFile(string openApiJson)
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
    return output.FullName;
}
static async Task RunMocker(string specPath) 
{
    FileInfo specFile = new FileInfo( specPath );
    if (!specFile.Exists)
    {
        throw new FileNotFoundException(specPath);
    }
    var specs = JsonConvert.DeserializeObject<EndpointSpecification[]>(
        await File.ReadAllTextAsync(specPath)
    );
    if (specs == null)
    {
        throw new ArgumentException("spec file " + specPath + " parsed to null");
    }
    for(int i = 0; i < specs.Length; i++)
    {
        specs[i].Validate("["+i+"]");
    }
    var builder = WebApplication.CreateBuilder(new string[] {});
    var app = builder.Build();
    app.UseEndpoints(endpoints => {
        foreach (var endpoint in specs)
        {
            endpoints.MapMethods(
                endpoint.ApiRoute,
                new[] { endpoint.Method.Verb },
                endpoint.ToRequestDelegate()
            );
        }
    });
    await app.RunAsync();
}
static void PrintUsage()
{
    Console.WriteLine("dotnet run ServerMocker mock|gen|help <args>");
    Console.WriteLine();
    Console.WriteLine("mock <mock specification file path>");
    Console.WriteLine("    Starts the server mocker with the provided conf");
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
