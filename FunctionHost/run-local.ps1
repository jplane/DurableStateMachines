
docker run `
	-e "ASPNETCORE_URLS=http://*:5002" `
	-e "storageConnectionString=" `
	-e "hubName=" `
	-e "callbackUri=" `
	-p 5002:5002 `
	-it `
	scdn/function-host:v1