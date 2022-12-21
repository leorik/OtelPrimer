dotnet publish -c Release
docker build .\OtelPrimer -t pingpong_player
docker-compose up -d