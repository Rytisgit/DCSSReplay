#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM rytisgit/wasm-build:7.0 AS build
WORKDIR /src
COPY ["DCSSTV/DCSSTV.Wasm/DCSSTV.Wasm.csproj", "./DCSSTV/DCSSTV.Wasm/"]
COPY ["TtyRecDecoder/TtyRecDecoder.csproj", "./TtyRecDecoder/"]
COPY ["InputParse/InputParser.csproj", "./InputParse/"]
COPY ["FrameGenerator/FrameGenerator.csproj", "./FrameGenerator/"]
RUN dotnet restore "DCSSTV/DCSSTV.Wasm/DCSSTV.Wasm.csproj"

COPY ["DCSSTV/DCSSTV.Wasm/", "./DCSSTV/DCSSTV.Wasm/"]
COPY ["DCSSTV/DCSSTV.Shared/", "./DCSSTV/DCSSTV.Shared/"]
COPY ["TtyRecDecoder/", "./TtyRecDecoder/"]
COPY ["InputParse/", "./InputParse/"]
COPY ["FrameGenerator/", "./FrameGenerator/"]
RUN dotnet build "DCSSTV/DCSSTV.Wasm/DCSSTV.Wasm.csproj" -v:n -c Release -o /app/build 

FROM build AS publish
RUN dotnet publish "DCSSTV/DCSSTV.Wasm/DCSSTV.Wasm.csproj" -c Release -o /app/publish

FROM nginx:alpine
EXPOSE 80
COPY --from=publish /app/publish /usr/share/nginx/html
COPY ["DCSSTV/DCSSTV.Wasm/mime.types", "/etc/nginx/mime.types" ]