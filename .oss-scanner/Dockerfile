# syntax=docker/dockerfile:1
#
# Build environment for Anthropic's OSS Scanner (https://red.anthropic.com/oss-scanner).
# Build context is the repository root (https://github.com/iplus-framework/iPlus).
#
# The image installs everything an offline agent needs to audit and *run* iPlus:
#   - the .NET 10 SDK to rebuild the sources,
#   - a local SQL Server 2022 plus the sample database from Database/iPlusV5.7z,
#     so gip.iplus.console (the headless host) can be started and exercised.
# Network access is available only while this image is built; everything is
# pre-fetched here so the agent itself runs fully offline.
FROM ubuntu:22.04

ENV DEBIAN_FRONTEND=noninteractive \
    LANG=C.UTF-8 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    NUGET_XMLDOC_MODE=skip

# Base tools. p7zip-full unpacks the sample database (Database/iPlusV5.7z).
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
      ca-certificates curl git gnupg p7zip-full \
 && rm -rf /var/lib/apt/lists/*

# .NET 10 SDK (network is available during this build only).
RUN curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh \
 && bash /tmp/dotnet-install.sh --channel 10.0 --install-dir /usr/share/dotnet \
 && rm /tmp/dotnet-install.sh \
 && ln -s /usr/share/dotnet/dotnet /usr/local/bin/dotnet

# Local SQL Server 2022 so the offline agent can start the headless console.
# Start it with: ACCEPT_EULA=Y MSSQL_SA_PASSWORD='<8+ chars, 3 of 4 categories>' sqlservr
RUN curl -fsSL https://packages.microsoft.com/keys/microsoft.asc \
      | gpg --dearmor > /usr/share/keyrings/microsoft-prod.gpg \
 && echo "deb [signed-by=/usr/share/keyrings/microsoft-prod.gpg] https://packages.microsoft.com/ubuntu/22.04/mssql-server-2022 jammy main" \
      > /etc/apt/sources.list.d/mssql-server.list \
 && echo "deb [signed-by=/usr/share/keyrings/microsoft-prod.gpg] https://packages.microsoft.com/ubuntu/22.04/prod jammy main" \
      > /etc/apt/sources.list.d/msprod.list \
 && apt-get update \
 && ACCEPT_EULA=Y NEEDRESTART_MODE=a apt-get install -y --no-install-recommends \
      mssql-server mssql-tools18 \
 && rm -rf /var/lib/apt/lists/*

ENV PATH="/opt/mssql-tools18/bin:${PATH}"

# scanner contract: the checkout lives inside the image, at /src
COPY . /src
WORKDIR /src

# Build the headless console host exactly the way build/publish-release.sh does it
# (Android TFM off, CI build). Output lands in /src/bin/Release/net10.0.
RUN dotnet build gip.iplus.console/gip.iplus.console.csproj -c Release \
      -p:BuildAndroidTargetFramework=false \
      -p:ContinuousIntegrationBuild=true --nologo

# Offline license/crypto tests (no database needed). Failures are reported but do
# not abort the image build.
RUN dotnet test gip.core.licensing.unit.test/gip.core.licensing.unit.test.csproj \
      -c Release -p:BuildAndroidTargetFramework=false --nologo \
      || echo "WARNING: licensing unit tests reported failures"

# Helper for the agent: starts SQL Server, restores the sample database from
# Database/iPlusV5.7z and runs gip.iplus.console (/U00 /P00). See the script
# header for details.
RUN chmod +x /src/.oss-scanner/run-console.sh
ENV IPLUS_CONSOLE_BIN="/src/bin/Release/net10.0"
