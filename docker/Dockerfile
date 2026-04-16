FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/GroupDocs.Metadata.Mcp/GroupDocs.Metadata.Mcp.csproj \
    -c Release -o /app --no-self-contained

FROM base AS final
COPY --from=build /app .

ENV GROUPDOCS_MCP_STORAGE_PATH=/data
VOLUME /data

RUN adduser --disabled-password --gecos "" mcpuser
USER mcpuser

ENTRYPOINT ["dotnet", "GroupDocs.Metadata.Mcp.dll"]
