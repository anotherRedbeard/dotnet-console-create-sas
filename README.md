# Create SAS token for storage account demo

This application is an example console app that will create a SAS token scoped to either the blob or container based on how you execute it.

## Description

This project was created initially using `dotnet new console` and by using the examples from the Microsoft learn site:  [Create an account SAS with .NET](https://learn.microsoft.com/en-us/azure/storage/common/storage-account-sas-create-dotnet?toc=%2Fazure%2Fstorage%2Fblobs%2Ftoc.json&bc=%2Fazure%2Fstorage%2Fblobs%2Fbreadcrumb%2Ftoc.json).  

Each SAS must be signed with a key and this example is creating the SAS tokens signed with an account storage key.  One thing to call out is in the appsettings.json file there is a `BlobNameForSAS` and `BlobNameForTest`.  This is so you can confirm that when you create a SAS for a particular blob name, you can only create that blob name in the container.  When you create the SAS for a container you can create whatever blob name you want in the test.

## How to use

This is meant to be a repo that you can clone and use as you like, but please keep in mind this is not intended to be 'production-ready' code.  Check out the "Run Locally" section.

### Requirements

- **Azure Subscription**
- **This repo cloned in your own GitHub repo**
- **Storage Account with a container**
  - You will get the ConnectionString from this storage account to be used in the `appsettings.json` file

### Running Options

| Option Name | Description |
| -------- | -------- |
| create-blob [test]   | Create the sas token with blob context, optional attribute to test the sas token by creating a "Hello World" file in the container   |
| create-container [test]  | Create the sas token with container context, optional attribute to test the sas token by creating a "Hello World" file in the container   |

## Run Locally

### VS Code

- Clone repo
- Create new `appsettings.json` and make sure it has the following variables:

    ```json
    {
        "AzureStorageConfig": {
            "ConnectionString": "<storage-account-connection-string>",
            "ContainerName": "test",
            "IpAddressStart": "<ip-address-range-start>",
            "IpAddressEnd": "<ip-address-range-end>",
            "BlobNameForSAS": "test.txt",
            "BlobNameForTest": "test.txt",
            "BlobContentAsString": "Hello World! This is a test."
        }
    }
    ```

- Command examples:
  - Create a sas token with blob context and display it in the console

    ```bash
    =>  dotnet run create-blob
    ```

  - Create a sas token with container context, display it in the console and create a new blob in the container using the sas token
  
    ```bash
    =>  dotnet run create-container test
    ```
