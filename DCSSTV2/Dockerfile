#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN apt-get update && apt-get install -y libfontconfig1 && apt-get install fontconfig
RUN apt-get install ncurses-term
RUN apt-get install -y libgtk-3-dev
ENV TERM=putty-256color
RUN export TERM=putty-256color

RUN echo "alias ls='ls --color=auto'" >> /root/.bashrc
RUN echo "alias grep='grep --color=auto'" >> /root/.bashrc
RUN echo "alias fgrep='fgrep --color=auto'" >> /root/.bashrc
RUN echo "alias egrep='egrep --color=auto'" >> /root/.bashrc
RUN echo "force_color_prompt=yes" >> /etc/bash.bashrc
RUN echo "alias ls='ls --color=auto'" >> /etc/bash.bashrc
RUN echo "alias grep='grep --color=auto'" >> /etc/bash.bashrc
RUN echo "alias fgrep='fgrep --color=auto'" >> /etc/bash.bashrc
RUN echo "alias egrep='egrep --color=auto'" >> /etc/bash.bashrc
RUN echo "force_color_prompt=yes" >> /etc/bash.bashrc
RUN echo "force_color_prompt=yes" >> /etc/bash.bashrc
RUN sed -i '1i force_color_prompt=yes' /etc/skel/.bashrc
SHELL ["/bin/sh", "-c", "-l"]


COPY ["DCSSTV2/libPuttyDLL.so", "../lib/"]

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build

RUN ls
WORKDIR /src
COPY ["DCSSTV2/DCSSTV2.csproj", "DCSSTV2/"]

RUN dotnet restore "DCSSTV2/DCSSTV2.csproj"
COPY . .

WORKDIR "/src/DCSSTV2"
RUN dotnet build "DCSSTV2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DCSSTV2.csproj" -c Release -o /app/publish


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .


ENV PATH="/lib/x86_64-linux-gnu:${PATH}"
ENV LD_LIBRARY_PATH="/app:/lib/x86_64-linux-gnu:$LD_LIBRARY_PATH"

ENTRYPOINT ["dotnet", "DCSSTV2.dll"]