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
RUN dotnet build "DCSSTV/DCSSTV.Wasm/DCSSTV.Wasm.csproj" -v:n -c Release

#FROM georgjung/nginx-brotli
FROM fholzer/nginx-brotli
EXPOSE 80
COPY --from=build /src/DCSSTV/DCSSTV.Wasm/bin/Release/net7.0/dist /usr/share/nginx/html
# Uno Wasm Bootstrap generates compressed gz and br files
# They are placed in _compressed_(br|gz) without the extensions for asp.net web.config
# Add the respective extensions and move into main folder to allow nginx to support precompressed files.
RUN find /usr/share/nginx/html/_compressed_gz/ -type f -exec mv {} {}.gz \;
RUN find /usr/share/nginx/html/_compressed_br/ -type f -exec mv {} {}.br \;
RUN cp -r /usr/share/nginx/html/_compressed_gz/* /usr/share/nginx/html/
RUN cp -r /usr/share/nginx/html/_compressed_br/* /usr/share/nginx/html/
RUN rm -r /usr/share/nginx/html/_compressed_br/
RUN rm -r /usr/share/nginx/html/_compressed_gz/
COPY ["DCSSTV/DCSSTV.Wasm/mime.types", "/etc/nginx/mime.types" ]
COPY ["DCSSTV/DCSSTV.Wasm/nginx.conf", "/etc/nginx/nginx.conf" ]