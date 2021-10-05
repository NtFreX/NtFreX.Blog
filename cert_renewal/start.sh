#!/bin/bash

echo "Starting cert refresh" && id && \

	echo "Requesting certificate" && \
	certbot certonly --text --agree-tos --email ntfrex@gmail.com \
			    --non-interactive \
				--manual-public-ip-logging-ok --manual \
				--manual-auth-hook /etc/letsencrypt/acme-dns-auth.py \
				--preferred-challenges dns \
                --config-dir /tmp/.certbot/config --logs-dir /tmp/.certbot/logs --work-dir /tmp/.certbot/work \
				-d \*.ntfrex.com -d ntfrex.com && \
	
	echo "Writing pfx file" && \
	openssl pkcs12 -inkey  /tmp/.certbot/config/live/ntfrex.com/privkey.pem \
			    -in /tmp/.certbot/config/live/ntfrex.com/fullchain.pem \
				-export -out /tmp/.certbot/config/live/ntfrex.com/cert.pfx \
				-password env:DomainSslCertPw && \

	echo "Writing base64 file" && \
	cat /tmp/.certbot/config/live/ntfrex.com/cert.pfx | base64 > /tmp/.certbot/config/live/ntfrex.com/cert_base64.pfx && \

	python3 /etc/letsencrypt/update_db.py
