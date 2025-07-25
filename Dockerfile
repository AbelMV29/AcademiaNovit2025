FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ./AcademiaNovit/AcademiaNovit.csproj ./AcademiaNovit/
COPY ./AcademiaNovit.Tests/AcademiaNovit.Tests.csproj ./AcademiaNovit.Tests/

WORKDIR /app/AcademiaNovit
RUN dotnet restore

WORKDIR /app/AcademiaNovit.Tests
RUN dotnet restore

WORKDIR /app/AcademiaNovit

COPY ./AcademiaNovit/. ./  
COPY ./AcademiaNovit.Tests/. ../AcademiaNovit.Tests/

RUN dotnet build
RUN dotnet test ../AcademiaNovit.Tests

RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /publish-app

COPY --from=build /app/out ./

ENV ASPNETCORE_HTTP_PORTS=5000

EXPOSE 5000
ENTRYPOINT ["dotnet", "AcademiaNovit.dll"]
