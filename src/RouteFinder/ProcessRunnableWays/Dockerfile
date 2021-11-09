FROM mcr.microsoft.com/dotnet/aspnet

# don't forget to
# dotnet publish -c Release

COPY  bin/Release/net6.0/publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "ProcessRunnableWays.dll"]
