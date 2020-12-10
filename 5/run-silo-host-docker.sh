#!/bin/bash

# 2020-12-07 PJ:
# Code below gets local IP for dockerized applications to function and communicate correctly.
ADVERTISEDIP=`ifconfig | grep -Eo 'inet (addr:)?([0-9]*\.){3}[0-9]*' | grep -Eo '([0-9]*\.){3}[0-9]*' | grep -v '127.0.0.1'`
SILOPORT=2002
GATEWAYPORT=3002
PRIMARYSILOPORT=2001
DASHBOARDPORT=8082

docker build -t silo-host -f ./ops/SiloHost/Dockerfile ./ &&
  docker run -it -e ADVERTISEDIP=$ADVERTISEDIP -e SILOPORT=$SILOPORT -e GATEWAYPORT=$GATEWAYPORT -e PRIMARYSILOPORT=$PRIMARYSILOPORT -p $SILOPORT:2000 -p $GATEWAYPORT:3000 -p $DASHBOARDPORT:8080 --rm silo-host