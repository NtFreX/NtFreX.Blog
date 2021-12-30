#!/bin/sh

iptables -t nat -A PREROUTING -i eth0 -p tcp --dport 443 -j REDIRECT --to-port 5001

#wget https://aws-otel-collector.s3.amazonaws.com/amazon_linux/amd64/latest/aws-otel-collector.rpm
#rpm -qa | grep -qw 'aws-otel-collector' || sudo rpm -Uvh  ./aws-otel-collector.rpm
#sudo /opt/aws/aws-otel-collector/bin/aws-otel-collector-ctl  -a stop
#sudo /opt/aws/aws-otel-collector/bin/aws-otel-collector-ctl -c /var/app/current/otel_config.yaml -a start
