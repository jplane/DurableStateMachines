
docker run `
	-e "SCDN_STORAGE_CONNECTION_STRING=" `
	-e "SCDN_HUB_NAME=" `
	-e "SCDN_CALLBACK_URI=" `
	-p 5002:80 `
	-it `
	scdn/function-host:v1
