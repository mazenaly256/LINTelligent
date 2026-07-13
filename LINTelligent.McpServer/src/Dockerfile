FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /LINTelligentMcpServer

# Restore as a separate step, to reuse cached layer when rebuild the image instead restore on each image building
COPY LINTelligent.McpServer.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /out --no-restore



FROM mcr.microsoft.com/dotnet/runtime:10.0

WORKDIR /LINTelligentMcpServer

COPY --from=build /out .

ENTRYPOINT ["dotnet", "LINTelligent.McpServer.dll"]