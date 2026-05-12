# Single-stage image (SDK) so we can run EF migrations at startup
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app

# Install dotnet-ef tool for migrations (match SDK/runtime 8.x)
RUN dotnet tool install --global dotnet-ef --version 8.0.8
ENV PATH="$PATH:/root/.dotnet/tools"

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /out

WORKDIR /out

# Default connection string (override with env on deployment, e.g. DATABASE_URL)
ENV ConnectionStrings__DefaultConnection="postgresql://huyh0_user:WiPoRZzAnr4aeJNiDEqh2OMVEld6f4oz@dpg-d4ruumi4d50c73b4jt60-a/huyh0"
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY entrypoint.sh /out/entrypoint.sh
RUN chmod +x /out/entrypoint.sh

ENTRYPOINT ["/out/entrypoint.sh"]
