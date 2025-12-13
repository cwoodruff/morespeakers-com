# Azure Deployment Guide for MoreSpeakers.com

This guide covers deploying the MoreSpeakers.com application to Microsoft Azure.

## Azure Resources Needed

All of these resources were created using the Azure Portal.
They are all required for the application to function properly.

- Azure Communication Services (ACS)
  - Email Communication Services
- Azure Application Insights
- Azure App Service Plan
  - App Service (Web App)
  - Function App
- Azure SQL Server
  - SQL Database
- Azure Key Vault
- Azure Storage Account
  - Blobs
  - Queues

## Application Deployment

Both the web application and function application are deployed with a commit to the main branch.

Deployment Files

- [Web](../.github/workflows/publish-function-app.yml)
- [Function](../.github/workflows/publish-web-app.yml)
 