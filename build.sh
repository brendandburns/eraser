#!/bin/bash

TAG=v1

dotnet public -c Release
docker build -t brendanburns/eraser:${TAG} .