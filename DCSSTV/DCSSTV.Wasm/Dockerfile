#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM dcsstvintermediate:latest AS build

COPY ["DCSSTV/DCSSTV.Wasm/", "./DCSSTV/DCSSTV.Wasm/"]
COPY ["DCSSTV/DCSSTV.Shared/", "./DCSSTV/DCSSTV.Shared/"]
COPY ["TtyRecDecoder/", "./TtyRecDecoder/"]
COPY ["PuttySharp/", "./PuttySharp/"]
COPY ["InputParse/", "./InputParse/"]
COPY ["FrameGenerator/", "./FrameGenerator/"]
RUN dotnet build "DCSSTV/DCSSTV.Wasm/DCSSTV.Wasm.csproj" -v:n -c Release -o /app/build 

FROM build AS publish
RUN dotnet publish "DCSSTV/DCSSTV.Wasm/DCSSTV.Wasm.csproj" -c Release -o /app/publish

FROM ibebbs/wasm-serve:latest
EXPOSE 80
COPY --from=publish /app/publish /usr/share/nginx/html