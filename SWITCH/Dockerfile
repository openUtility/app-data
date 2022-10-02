FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim as compiling

WORKDIR /app

COPY . .

RUN dotnet publish -c Release -o out -r linux-x64 --self-contained
RUN chmod +x /app/out/app-data-switch  
RUN cp /app/out/app-data-switch  /app/out/appDataSwitch


FROM mcr.microsoft.com/dotnet/runtime-deps:6.0


WORKDIR /app

COPY --from=compiling /app/out /app

ENV ASPNETCORE_URLS="http://0.0.0.0:5000;http://0.0.0.0:5001"

ENTRYPOINT [ "./app-data-switch" ]