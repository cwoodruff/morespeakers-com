# Azure Deployment Guide for MoreSpeakers.com

This guide covers deploying the MoreSpeakers.com application to Microsoft Azure using various approaches.

## Prerequisites

Before deploying to Azure, ensure you have:

- Azure subscription with appropriate permissions
- Azure CLI installed and configured
- .NET 8.0 SDK installed
- Docker Desktop (for containerized deployments)
- Visual Studio 2022 or VS Code with Azure extensions

## Deployment Options

### Option 1: Azure App Service with Azure SQL Database (Recommended)

This is the most straightforward deployment option for ASP.NET Core applications.

#### Step 1: Create Azure Resources

```bash
# Set variables
RESOURCE_GROUP="MoreSpeakers-RG"
LOCATION="eastus"
APP_SERVICE_PLAN="MoreSpeakers-Plan"
WEB_APP="morespeakers-web"
SQL_SERVER="morespeakers-sql-server"
DATABASE="MoreSpeakers"
STORAGE_ACCOUNT="morespeakersstorage"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create App Service Plan (Production tier for better performance)
az appservice plan create \
    --name $APP_SERVICE_PLAN \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --sku P1V3 \
    --is-linux

# Create Web App
az webapp create \
    --name $WEB_APP \
    --resource-group $RESOURCE_GROUP \
    --plan $APP_SERVICE_PLAN \
    --runtime "DOTNETCORE:8.0"

# Create SQL Server
az sql server create \
    --name $SQL_SERVER \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --admin-user sqladmin \
    --admin-password "YourSecurePassword123!"

# Create SQL Database
az sql db create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER \
    --name $DATABASE \
    --service-objective S2

# Create Storage Account for file uploads
az storage account create \
    --name $STORAGE_ACCOUNT \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --sku Standard_LRS

# Configure firewall to allow Azure services
az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER \
    --name AllowAzureServices \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0
```

#### Step 2: Configure Application Settings

```bash
# Get connection string
CONNECTION_STRING=$(az sql db show-connection-string \
    --client ado.net \
    --server $SQL_SERVER \
    --name $DATABASE \
    --auth-type SqlPassword \
    --output tsv)

# Replace placeholders in connection string
CONNECTION_STRING=${CONNECTION_STRING//<username>/sqladmin}
CONNECTION_STRING=${CONNECTION_STRING//<password>/YourSecurePassword123!}

# Set application settings
az webapp config appsettings set \
    --name $WEB_APP \
    --resource-group $RESOURCE_GROUP \
    --settings \
        "ConnectionStrings__DefaultConnection=$CONNECTION_STRING" \
        "ASPNETCORE_ENVIRONMENT=Production" \
        "WEBSITES_ENABLE_APP_SERVICE_STORAGE=false"

# Get storage connection string
STORAGE_CONNECTION=$(az storage account show-connection-string \
    --name $STORAGE_ACCOUNT \
    --resource-group $RESOURCE_GROUP \
    --output tsv)

az webapp config appsettings set \
    --name $WEB_APP \
    --resource-group $RESOURCE_GROUP \
    --settings "ConnectionStrings__Storage=$STORAGE_CONNECTION"
```

#### Step 3: Deploy Application

**Using Azure CLI:**

```bash
# Build and publish the application
dotnet publish MoreSpeakers.Web -c Release -o ./publish

# Create deployment package
cd publish
zip -r ../deploy.zip .
cd ..

# Deploy to Azure
az webapp deploy \
    --name $WEB_APP \
    --resource-group $RESOURCE_GROUP \
    --src-path deploy.zip \
    --type zip
```

**Using Visual Studio:**

1. Right-click on the `MoreSpeakers.Web` project
2. Select "Publish"
3. Choose "Azure" as target
4. Select "Azure App Service (Linux)"
5. Choose your subscription and the created App Service
6. Click "Publish"

**Using GitHub Actions (Recommended for CI/CD):**

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Azure

on:
  push:
    branches: [ main ]
  workflow_dispatch:

env:
  AZURE_WEBAPP_NAME: morespeakers-web
  AZURE_WEBAPP_PACKAGE_PATH: './publish'
  DOTNET_VERSION: '8.0.x'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
    
    - name: Publish
      run: dotnet publish MoreSpeakers.Web -c Release -o ${{ env.AZURE_WEBAPP_PACKAGE_PATH }}
    
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: ${{ env.AZURE_WEBAPP_NAME }}
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ${{ env.AZURE_WEBAPP_PACKAGE_PATH }}
```

### Option 2: Container Deployment

#### Create Dockerfile

Create `Dockerfile` in the solution root:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MoreSpeakers.Web/MoreSpeakers.Web.csproj", "MoreSpeakers.Web/"]
COPY ["MoreSpeakers.AppHost/MoreSpeakers.AppHost.csproj", "MoreSpeakers.AppHost/"]
RUN dotnet restore "MoreSpeakers.Web/MoreSpeakers.Web.csproj"
COPY . .
WORKDIR "/src/MoreSpeakers.Web"
RUN dotnet build "MoreSpeakers.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MoreSpeakers.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MoreSpeakers.Web.dll"]
```

#### Deploy Container to Azure Container Apps

```bash
# Create Container Apps environment
az containerapp env create \
    --name morespeakers-env \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION

# Build and push container
az acr create \
    --resource-group $RESOURCE_GROUP \
    --name morespeakersacr \
    --sku Basic \
    --admin-enabled true

# Build image
docker build -t morespeakersacr.azurecr.io/morespeakers:latest .

# Push to ACR
az acr login --name morespeakersacr
docker push morespeakersacr.azurecr.io/morespeakers:latest

# Deploy container app
az containerapp create \
    --name morespeakers-app \
    --resource-group $RESOURCE_GROUP \
    --environment morespeakers-env \
    --image morespeakersacr.azurecr.io/morespeakers:latest \
    --target-port 80 \
    --ingress 'external' \
    --registry-server morespeakersacr.azurecr.io \
    --registry-username morespeakersacr \
    --registry-password $(az acr credential show --name morespeakersacr --query passwords[0].value -o tsv) \
    --env-vars \
        "ConnectionStrings__DefaultConnection=$CONNECTION_STRING" \
        "ASPNETCORE_ENVIRONMENT=Production"
```

## Configuration for Production

### Application Settings

Set these in Azure App Service Configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:morespeakers-sql-server.database.windows.net,1433;Initial Catalog=MoreSpeakers;Persist Security Info=False;User ID=sqladmin;Password=YourSecurePassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
    "Storage": "DefaultEndpointsProtocol=https;AccountName=morespeakersstorage;AccountKey=your-key-here;EndpointSuffix=core.windows.net"
  },
  "ASPNETCORE_ENVIRONMENT": "Production",
  "WEBSITES_ENABLE_APP_SERVICE_STORAGE": "false",
  "WEBSITE_RUN_FROM_PACKAGE": "1"
}
```

### Security Configuration

#### Enable HTTPS Only
```bash
az webapp update \
    --name $W