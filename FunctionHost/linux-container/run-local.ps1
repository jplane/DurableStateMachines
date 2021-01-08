
docker run `
	-e "ASPNETCORE_URLS=http://*:5002" `
	-e "SCDN_STORAGE_CONNECTION_STRING=" `
	-e "SCDN_HUB_NAME=" `
	-e "SCDN_CALLBACK_URI=" `
	-p 5002:5002 `
	-it `
	scdn/function-host:v1