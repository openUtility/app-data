FROM mcr.microsoft.com/dotnet/sdk:6.0 as compiling

COPY app-data-switch.csproj .
RUN dotnet restore 
COPY . .
RUN dotnet build -c Release --no-restore --verbosity minimal
RUN dotnet publish -c Release -o out --no-restore --verbosity minimal


FROM scratch

WORKDIR /app

COPY --from=compiling ./out /app

ENTRYPOINT [ "app-data-switch" ]