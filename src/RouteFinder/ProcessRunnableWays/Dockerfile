FROM mcr.microsoft.com/dotnet/aspnet

COPY  bin/Release/net5.0/publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "ProcessRunnableWays.dll"]
