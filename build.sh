#!/bin/bash

TAG=v3

dotnet publish -c Release
docker build -t brendanburns/eraser:${TAG} .
