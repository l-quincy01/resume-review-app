@description('Azure region for the API Container App.')
param location string = resourceGroup().location

@description('Existing Azure Container Registry name.')
param registryName string

@description('Existing Azure Container Apps managed environment name.')
param environmentName string

@description('Existing user-assigned identity name used by Container Apps to pull images from ACR.')
param pullIdentityName string = '${environmentName}-acr-pull'

@description('Public API Container App name.')
param apiAppName string

@description('Fully qualified API container image, including tag.')
param apiImage string

@description('Allowed CORS origin for the web app.')
param webBaseUrl string

@description('ASP.NET Core AllowedHosts value for the API app.')
param apiAllowedHosts string

@description('Common Azure resource tags.')
param tags object = {}

@minValue(0)
param apiMinReplicas int = 1

@minValue(1)
param apiMaxReplicas int = 3

resource registry 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' existing = {
  name: registryName
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: environmentName
}

resource pullIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: pullIdentityName
}

resource apiApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: apiAppName
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${pullIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 5053
        transport: 'auto'
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      registries: [
        {
          server: registry.properties.loginServer
          identity: pullIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: apiImage
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://0.0.0.0:5053'
            }
            {
              name: 'Cors__AllowedOrigins__0'
              value: webBaseUrl
            }
            {
              name: 'AllowedHosts'
              value: apiAllowedHosts
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        minReplicas: apiMinReplicas
        maxReplicas: apiMaxReplicas
        rules: [
          {
            name: 'http-scale'
            http: {
              metadata: {
                concurrentRequests: '50'
              }
            }
          }
        ]
      }
    }
  }
}

output apiFqdn string = apiApp.properties.configuration.ingress.fqdn
