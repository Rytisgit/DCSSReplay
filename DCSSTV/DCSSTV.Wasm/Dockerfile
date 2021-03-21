#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["DCSSTV/DCSSTV.Wasm/DCSSTV.Wasm.csproj", "DCSSTV/DCSSTV.Wasm/"]
COPY ["TtyRecDecoder/TtyRecDecoder.csproj", "TtyRecDecoder/"]
COPY ["PuttySharp/PuttySharp.csproj", "PuttySharp/"]
COPY ["InputParse/InputParser.csproj", "InputParse/"]
COPY ["FrameGenerator/FrameGenerator.csproj", "FrameGenerator/"]
RUN dotnet restore "DCSSTV/DCSSTV.Wasm/DCSSTV.Wasm.csproj"
COPY . .
WORKDIR "/src/DCSSTV/DCSSTV.Wasm"
RUN dotnet build "DCSSTV.Wasm.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DCSSTV.Wasm.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DCSSTV.Wasm.dll"]