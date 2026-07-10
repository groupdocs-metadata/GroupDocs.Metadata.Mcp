FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/GroupDocs.Metadata.Mcp/GroupDocs.Metadata.Mcp.csproj \
    -c Release -o /app --no-self-contained

FROM base AS final

# Native graphics deps for GroupDocs.Metadata's image-format paths (JPEG, PNG,
# image-bearing PDFs) on Linux.
#   - libfontconfig1 is REQUIRED as of engine 26.6.0: its net8.0 runtime sub-package
#     pulls the full SkiaSharp.NativeAssets.Linux (not the self-contained
#     NoDependencies variant), whose libSkiaSharp.so dlopens libfontconfig.
#   - libgdiplus is kept as a safety net. 26.6.0 pulls System.Drawing.Common 8.0.0
#     (which no longer honours System.Drawing.EnableUnixSupport) alongside
#     Aspose.Drawing 26.4.0, a managed cross-platform GDI+ replacement. If CI on
#     the 3-OS matrix confirms Aspose.Drawing covers every image path, libgdiplus
#     (and the csproj flag) can be dropped. See assets/2026-07-01.md.
USER root
RUN apt-get update \
 && apt-get install -y --no-install-recommends libgdiplus libfontconfig1 \
 && rm -rf /var/lib/apt/lists/*

# OCI image labels. VERSION is supplied by the publish workflow via
# --build-arg VERSION=YY.MM.N. Default 'dev' makes local `docker build` runs
# self-identify instead of inheriting the base image's misleading version.
ARG VERSION=dev
LABEL org.opencontainers.image.title="GroupDocs.Metadata.Mcp" \
      org.opencontainers.image.description="MCP server exposing GroupDocs.Metadata for .NET as AI-callable tools (read_metadata, search_metadata, write_metadata, remove_metadata, get_document_info) for Claude, Cursor, GitHub Copilot, and other MCP agents." \
      org.opencontainers.image.version="${VERSION}" \
      org.opencontainers.image.source="https://github.com/groupdocs-metadata/GroupDocs.Metadata.Mcp" \
      org.opencontainers.image.url="https://www.nuget.org/packages/GroupDocs.Metadata.Mcp" \
      org.opencontainers.image.licenses="MIT" \
      org.opencontainers.image.vendor="Aspose Pty Ltd"

COPY --from=build /app .

ENV GROUPDOCS_MCP_STORAGE_PATH=/data
VOLUME /data

USER $APP_UID

ENTRYPOINT ["dotnet", "GroupDocs.Metadata.Mcp.dll"]
