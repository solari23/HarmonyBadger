# Helps set up secrets in the developer environment by fetching them from KeyVault
# and leveraging 'dotnet user-secrets'.
#
# Requires Azure CLI.

# Must match the UserSecretsId property in the HarmonyBadger project file.
$USER_SECRETS_ID = "HarmonyBadgerDevSecrets"
$KEYVAULT_NAME = "harmony-badger-secrets"

# Log into azure management
az login

# Initialize the local secret store
$harmonyBadgerProjFile = Resolve-Path "$PSScriptRoot/../src/HarmonyBadger/HarmonyBadger.csproj"
dotnet user-secrets init --id $USER_SECRETS_ID -p $harmonyBadgerProjFile

# Fetch KeyVault secrets and set them in local store
$secret = az keyvault secret show --vault-name $KEYVAULT_NAME --name DiscordBotSecret --query value
dotnet user-secrets --id $USER_SECRETS_ID -p $harmonyBadgerProjFile set "DISCORD_BOT_SECRET" $secret

$secret = az keyvault secret show --vault-name $KEYVAULT_NAME --name AadAuthorizationAppCert --query value
dotnet user-secrets --id $USER_SECRETS_ID -p $harmonyBadgerProjFile set "AAD_AUTHORIZATION_APP_CERT" $secret

$secret = az keyvault secret show --vault-name $KEYVAULT_NAME --name TokenEncryptionKey --query value
dotnet user-secrets --id $USER_SECRETS_ID -p $harmonyBadgerProjFile set "TOKEN_ENCRYPTION_KEY" $secret

$secret = az keyvault secret show --vault-name $KEYVAULT_NAME --name TelesignCustomerId --query value
dotnet user-secrets --id $USER_SECRETS_ID -p $harmonyBadgerProjFile set "TELESIGN_CUSTOMER_ID" $secret

$secret = az keyvault secret show --vault-name $KEYVAULT_NAME --name TelesignApiKey --query value
dotnet user-secrets --id $USER_SECRETS_ID -p $harmonyBadgerProjFile set "TELESIGN_API_KEY" $secret