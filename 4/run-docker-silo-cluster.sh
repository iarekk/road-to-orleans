#!/bin/bash
export ADVERTISEDIP=`ifconfig | grep -Eo 'inet (addr:)?([0-9]*\.){3}[0-9]*' | grep -Eo '([0-9]*\.){3}[0-9]*' | grep -v '127.0.0.1'`
docker-compose -f docker-compose-silocluster.yml build  &&
  docker-compose -f docker-compose-silocluster.yml up 