
# assume you are already logged into the Azure CLI with a target subscription
#  https://docs.microsoft.com/en-us/cli/azure/authenticate-azure-cli

#$AZURE_TENANT_ID		= "34228754-5bf2-4b90-ba44-b6b4e3ac98d5"
#$AZURE_SUBSCRIPTION_ID	= "b2dd0aa1-6cd7-40cb-b2d9-91c14108d668"
#$AZURE_CLIENT_ID		= "4f59e267-6410-414c-8ca7-49e30eae50b0"
#$AZURE_CLIENT_SECRET	= "57718494-a704-4aa3-bee3-92bb2a032740"
$FUNCTION_APP_LOCATION	= "eastus2"
$FUNCTION_APP_NAME		= "scdnjosh"
$SCDN_HUB_NAME			= "josh"
$SCDN_CALLBACK_URI		= ""

terraform init

terraform apply `
	-auto-approve `
	-var "name=$FUNCTION_APP_NAME" `
	-var "location=$FUNCTION_APP_LOCATION" `
	-var "hubName=$SCDN_HUB_NAME" `
	-var "callbackUri=$SCDN_CALLBACK_URI"
