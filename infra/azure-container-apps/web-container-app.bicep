@description('Azure region for the web Container App.')
param location string = resourceGroup().location

@description('Existing Azure Container Registry name.')
param registryName string

@description('Existing Azure Container Apps managed environment name.')
param environmentName string

@description('Existing user-assigned identity name used by Container Apps to pull images from ACR.')
param pullIdentityName string = '${environmentName}-acr-pull'

@description('Public web Container App name.')
param webAppName string

@description('Fully qualified web container image, including tag.')
param webImage string

@description('Public HTTPS origin for the API app.')
param apiBaseUrl string

@description('Frontend log level.')
param logLevel string = 'info'

@description('Common Azure resource tags.')
param tags object = {}

@minValue(0)
param webMinReplicas int = 1

@minValue(1)
param webMaxReplicas int = 3

resource registry 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' existing = {
  name: registryName
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: environmentName
}

resource pullIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: pullIdentityName
}

resource webApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: webAppName
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
        targetPort: 3000
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
          name: 'web'
          image: webImage
          env: [
            {
              name: 'NODE_ENV'
              value: 'production'
            }
            {
              name: 'PORT'
              value: '3000'
            }
            {
              name: 'HOSTNAME'
              value: '0.0.0.0'
            }
            {
              name: 'NEXT_PUBLIC_API_BASE'
              value: apiBaseUrl
            }
            {
              name: 'LOG_LEVEL'
              value: logLevel
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        minReplicas: webMinReplicas
        maxReplicas: webMaxReplicas
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

output webFqdn string = webApp.properties.configuration.ingress.fqdn
