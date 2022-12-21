FROM mcr.microsoft.com/dotnet/aspnet:7.0
ARG source
WORKDIR /app
EXPOSE 80
COPY ${source:-bin/Release/net7.0/publish} .
ENTRYPOINT ["dotnet", "OTelPrimer.dll"]