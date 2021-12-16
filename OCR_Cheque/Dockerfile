#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["OCR_Cheque/OCR_Cheque.csproj", "OCR_Cheque/"]
RUN dotnet restore "OCR_Cheque/OCR_Cheque.csproj"
COPY . .
WORKDIR "/src/OCR_Cheque"
RUN dotnet build "OCR_Cheque.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OCR_Cheque.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OCR_Cheque.dll"]