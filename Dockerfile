FROM mcr.microsoft.com/dotnet/core/runtime:3.1

RUN curl https://github.com/containerd/containerd/releases/download/v1.4.0/containerd-1.4.0-linux-amd64.tar.gz -L -o archive.tgz
RUN tar -xvzf archive.tgz
RUN mv bin/ctr /usr/bin/ctr

COPY bin/Release /app

ENTRYPOINT ["dotnet", "/app/netcoreapp3.1/eraser.dll"]
