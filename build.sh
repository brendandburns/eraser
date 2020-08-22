#!/bin/bash

TAG=v2

dotnet publish -c Release
docker build -t brendanburns/eraser:${TAG} .
