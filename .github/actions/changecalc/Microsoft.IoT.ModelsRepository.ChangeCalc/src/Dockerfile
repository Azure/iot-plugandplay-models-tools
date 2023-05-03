FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
WORKDIR /app 

COPY ./* ./
RUN dotnet publish -c Release -o ./publish

FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app

COPY --from=build /app/publish/ ./
COPY ./entrypoint.sh /entrypoint.sh

RUN chmod +x /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]
