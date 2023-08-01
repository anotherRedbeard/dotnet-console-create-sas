using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Core.Pipeline;
using System.Net;
using static SasResourceTypeEnum;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

string action = args.Length > 0 ? args[0].ToLower() : "greet";

var result = action switch
{
    "create-blob" => CreateSasToken(args, configuration, SasResourceType.Blob),
    "create-container" => CreateSasToken(args, configuration, SasResourceType.Container),
    _ => InvalidOption()
};

Environment.ExitCode = result;

static int CreateSasToken(string[] args, IConfiguration configuration, SasResourceType sasResourceType = SasResourceType.Blob)
{
    if (args.Length == 2 && args[1].ToLower() == "test")
    {
        var sasToken = GenerateSasToken(configuration, sasResourceType);
        TestSasToken(sasToken, configuration);
    }
    else
    {
        GenerateSasToken(configuration, sasResourceType);
    }
    return 0;
}

static Uri GenerateSasToken(IConfiguration configuration, SasResourceType sasResourceType)
{
    //Setting this to a dummy value for now
    Uri sasToken = new Uri("https://localhost");
    var connectionString = configuration.GetSection("AzureStorageConfig")["ConnectionString"] ?? string.Empty;
    var containerName = configuration.GetSection("AzureStorageConfig")["ContainerName"] ?? string.Empty;
    var startIPAddress = configuration.GetSection("AzureStorageConfig")["IpAddressStart"] ?? string.Empty;
    var endIPAddress = configuration.GetSection("AzureStorageConfig")["IpAddressEnd"] ?? string.Empty;
    var blobName = configuration.GetSection("AzureStorageConfig")["BlobName"] ?? string.Empty;
    var blobServiceClient = new BlobServiceClient(connectionString);
    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    var blobClient = containerClient.GetBlobClient(blobName);

    var sasBuilder = new BlobSasBuilder()
    {
        BlobContainerName = containerName,
        //add IPRange using a string ip address
        IPRange = new SasIPRange(IPAddress.Parse(startIPAddress), IPAddress.Parse(endIPAddress)),
        StartsOn = DateTimeOffset.UtcNow,
        ExpiresOn = DateTimeOffset.UtcNow.AddSeconds(60), // Expiration time for the SAS token
    };

    sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);

    if (sasResourceType == SasResourceType.Container)
    {
        //Create token at the container level
        sasBuilder.Resource = "c";
        sasToken = containerClient.GenerateSasUri(sasBuilder);
    }
    else if (sasResourceType == SasResourceType.Blob)
    {
        //Create token at the blob level
        sasBuilder.Resource = "b";
        sasBuilder.BlobName = blobName;
        sasToken = blobClient.GenerateSasUri(sasBuilder);
    }

    Console.WriteLine($"Generated SAS token: {sasToken}");
    return sasToken;
}

static void TestSasToken(Uri uri, IConfiguration configuration)
{
    // Your code to test the SAS token goes here
    // You can perform operations like reading, writing, or listing blobs
    // using the SAS token to ensure it works as expected.
    Console.WriteLine("Testing the SAS token...");
    try
    {
        var testFileName = configuration.GetSection("AzureStorageConfig")["BlobName"] ?? string.Empty;
        var testContent = configuration.GetSection("AzureStorageConfig")["BlobContentAsString"] ?? string.Empty;
        var containerName = configuration.GetSection("AzureStorageConfig")["ContainerName"] ?? string.Empty;

        // Create BlobClient with the URI containing the SAS token
        var blobServiceClient = new BlobServiceClient(uri, new BlobClientOptions { Transport = new HttpClientTransport() });
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = blobContainerClient.GetBlobClient(testFileName);

        var blobContent = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(testContent));
        blobClient.Upload(blobContent);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error while testing the SAS token: {ex.Message}");
    }
    Console.WriteLine("Completed testing the SAS token");
}

static int InvalidOption()
{
    Console.WriteLine("Invalid option. Available options: create-blob [test], create-container [test]");
    return 1;
}