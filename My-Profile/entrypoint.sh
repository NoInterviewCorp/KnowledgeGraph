#!/bin/bash

set -e
run_cmd="dotnet run"

./wait-for-it.sh -t 0 neo4j:11008 -- echo "neo4j is up"
./wait-for-it.sh -t 0 rabbitmq:15672 -- echo "RabbitMQ is up"

>&2 echo "RabbitMQ and Neo4j is Up"
exec $run_cmd