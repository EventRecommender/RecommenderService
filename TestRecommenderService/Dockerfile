FROM mcr.microsoft.com/dotnet/sdk:6.0

WORKDIR /app
COPY . .
COPY "/RecommenderService" .

RUN dotnet restore

# run tests on docker build
RUN dotnet test

# run tests on docker run
ENTRYPOINT ["dotnet", "test"]
