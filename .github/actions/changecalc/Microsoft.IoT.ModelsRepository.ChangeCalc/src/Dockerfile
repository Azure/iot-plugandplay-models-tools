FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
WORKDIR /app 

COPY ./* ./
RUN dotnet publish -c Release -o ./publish

FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /app

COPY --from=build /app/publish/ ./
COPY ./entrypoint.sh /entrypoint.sh

RUN chmod +x /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]
