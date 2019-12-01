#!/bin/bash

###########################
# build & deploy services #
###########################

# log-service
docker build -t aspdotnetcore-webapi-sample:1.0.0 .
kubectl apply -f ./aspdotnetcore-webapi-sample.yaml
