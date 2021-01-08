
# thanks https://raw.githubusercontent.com/zenika-open-source/terraform-azure-cli/master/Dockerfile

# Setup build arguments with default versions
ARG AZURE_CLI_VERSION=2.15.1
ARG TERRAFORM_VERSION=0.14.0
ARG PYTHON_MAJOR_VERSION=3.7
ARG DEBIAN_VERSION=buster-20201012-slim

# Download Terraform binary
FROM debian:${DEBIAN_VERSION} as terraform-cli
RUN apt-get update
RUN apt-get install -y --no-install-recommends curl=7.64.0-4+deb10u1
RUN apt-get install -y --no-install-recommends ca-certificates=20190110
RUN apt-get install -y --no-install-recommends unzip=6.0-23+deb10u1
RUN apt-get install -y --no-install-recommends gnupg=2.2.12-1+deb10u1
RUN curl -Os https://releases.hashicorp.com/terraform/${TERRAFORM_VERSION}/terraform_${TERRAFORM_VERSION}_SHA256SUMS
RUN curl -Os https://releases.hashicorp.com/terraform/${TERRAFORM_VERSION}/terraform_${TERRAFORM_VERSION}_linux_amd64.zip
RUN curl -Os https://releases.hashicorp.com/terraform/${TERRAFORM_VERSION}/terraform_${TERRAFORM_VERSION}_SHA256SUMS.sig
COPY hashicorp.asc hashicorp.asc
RUN gpg --import hashicorp.asc
RUN gpg --verify terraform_${TERRAFORM_VERSION}_SHA256SUMS.sig terraform_${TERRAFORM_VERSION}_SHA256SUMS
SHELL ["/bin/bash", "-o", "pipefail", "-c"]
RUN grep terraform_${TERRAFORM_VERSION}_linux_amd64.zip terraform_${TERRAFORM_VERSION}_SHA256SUMS | sha256sum -c -
RUN unzip -j terraform_${TERRAFORM_VERSION}_linux_amd64.zip

# Install az CLI using PIP
FROM debian:${DEBIAN_VERSION} as azure-cli
RUN apt-get update
RUN apt-get install -y --no-install-recommends python3=${PYTHON_MAJOR_VERSION}.3-1
RUN apt-get install -y --no-install-recommends python3-pip=18.1-5
RUN apt-get install -y --no-install-recommends gcc=4:8.3.0-1
RUN apt-get install -y --no-install-recommends python3-dev=${PYTHON_MAJOR_VERSION}.3-1
RUN pip3 install setuptools==50.3.2
RUN pip3 install wheel==0.35.1
RUN pip3 install azure-cli==${AZURE_CLI_VERSION}

# Build final image
FROM mcr.microsoft.com/azure-functions/dotnet:3.0-dotnet3-core-tools

# install Terraform and Azure CLI
RUN apt-get update \
  && apt-get install -y --no-install-recommends \
    ca-certificates=20190110 \
    git=1:2.20.1-2+deb10u3 \
    python3=${PYTHON_MAJOR_VERSION}.3-1 \
    python3-distutils=${PYTHON_MAJOR_VERSION}.3-1 \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/* \
  && update-alternatives --install /usr/bin/python python /usr/bin/python${PYTHON_MAJOR_VERSION} 1
COPY --from=terraform-cli /terraform /usr/local/bin/terraform
COPY --from=azure-cli /usr/local/bin/az* /usr/local/bin/
COPY --from=azure-cli /usr/local/lib/python${PYTHON_MAJOR_VERSION}/dist-packages /usr/local/lib/python${PYTHON_MAJOR_VERSION}/dist-packages
COPY --from=azure-cli /usr/lib/python3/dist-packages /usr/lib/python3/dist-packages

# copy source
COPY /src/app /src/app
COPY /src/function-defs /func-app
COPY /src/host.json /func-app/host.json
COPY /src/infrastructure.tf /src/tf/infrastructure.tf
COPY /src/publish.sh /src/publish.sh

CMD[ "/src/publish.sh" ]
