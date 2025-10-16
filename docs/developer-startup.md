# Developer Startup

This document contains everything you need to get started developing on this project.

## Required Software

The following software is required to get started:

- Visual Studio, or Visual Studio Code, or JetBrains Rider
- Docker Desktop
- [Azure Storage Explorer](https://azure.microsoft.com/en-us/products/storage/storage-explorer/)
- Git
- [Microsoft Library Manager](https://github.com/aspnet/LibraryManager)

## Cloning the Repository

Clone this [repository](https://github.com/cwoodruff/morespeakers-com) to your local machine.
You can clone it with any tool you like.  If you want clone it via the command line, you can use the following command:

```bash
git clone https://github.com/cwoodruff/morespeakers-com.git
```

## Getting Started

Before you open the project, I recommend you install the [Microsoft Library Manager](https://github.com/aspnet/LibraryManager).
This will make it easier to install the required JavaScript libraries.

Installing the library manager is as simple as running the following command:

```bash
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
```

After installing Library Manager, or if you already have it, open up a terminal/command prompt and navigate to the project directory.

```bash
cd /src/Morespeakers.Web
libman restore
```

This will install the required JavaScript libraries. It can take a few minutes to complete depending on your internet connection speed.

Now you can open the project in your favorite IDE.

## Updating Application Settings

Most of the application settings are with the project and will be available to you when you clone the repository.
However, there are few settings that are not in the repository and need to be added.

You can reach out to a member of the project team to get these settings.
The settings below are required for the project to run and just sample values. 

### MoreSpeakers.Web

#### User Secrets

Open up the user secrets for this project.  Add the following secrets:

```json
{

  "APPLICATIONINSIGHTS_CONNECTION_STRING": "InstrumentationKey=<Replace_Me>;IngestionEndpoint=https://centralus-2.in.applicationinsights.azure.com/;LiveEndpoint=https://centralus.livediagnostics.monitor.azure.com/;ApplicationId=<Replace_Me>",
  "APPINSIGHTS_INSTRUMENTATIONKEY": ""
}
```

Or execute the following commands in the terminal/Developer PowerShell:
```bash
dotnet user-secrets set APPLICATIONINSIGHTS_CONNECTION_STRING "InstrumentationKey=<Replace_Me>;IngestionEndpoint=https://centralus-2.in.applicationinsights.azure.com/;LiveEndpoint=https://centralus.livediagnostics.monitor.azure.com/;ApplicationId=<Replace_Me>"
dotnet user-secrets set APPINSIGHTS_INSTRUMENTATIONKEY "<Replace_Me"
```

### MoreSpeakers.Functions

#### Local.Settings.json

You will need to add a file to the project called `local.settings.json`.
This file will contain the following:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "WEBSITE_CLOUD_ROLENAME": "FunctionApp",
    "ProcessPoisonedSendEmailMessages_CronSettings": "0 */2 * * * *",
    "AzureWebJobs.ProcessEmailDeliveryReports.Disabled": true,
    "AzureWebJobs.ProcessPoisonedSendEmailMessages.Disabled": true,
    "AzureWebJobs.SendEmails.Disabled": false
  },
  "Settings": {
    "AzureCommunicationsConnectionString": "Set in Users Secrets/Azure App Service",
    "BouncedEmailStatuses": "550 5.4.310;550 5.1.1;550.30;552 5.2.2;552-5.2.2;554.30;"
  }
}
```

#### User Secrets

Open up the user secrets for this project.  Add the following secrets:

```json
{
  "APPLICATIONINSIGHTS_CONNECTION_STRING": "InstrumentationKey=<Replace_Me>;IngestionEndpoint=https://centralus-2.in.applicationinsights.azure.com/;LiveEndpoint=https://centralus.livediagnostics.monitor.azure.com/;ApplicationId=70b7a451-3c7a-4ef8-934b-3702cde1e366",
  "APPINSIGHTS_INSTRUMENTATIONKEY": "<Replace_Me>",
  "Settings:AzureCommunicationsConnectionString": "endpoint=https://acs-more-speakers.unitedstates.communication.azure.com/;accesskey=<Replace_Me>"
}
```

## Running the Application

Once you have the application settings configured, you can run the application.

***NOTE:***: The first time you run the application, it will take a few minutes to build the application, database, and supporting services.

Run or Debug the *MoreSpeakers.AppHost: https* project. 

After the application is running, you will most likely get an error from the function application.
This is expected because the queue were not created initially.

Open up the Azure Storage Explorer. 

* In the *Emulator & Attached* tree, expand *Storage Accounts* nodes.
* Look for the storage account names that starts with `devstoreaccount`.
* Expand it
* Expand the *Queues* node.
* Create the following queues:
  * `bounced-emails`
  * `send-email`
  * `send-email-poison`

Restart the *MoreSpeakers.AppHost: https* project.

You should now be able to navigate to the application and use it.