FROM ubuntu:20.04

ARG DnsAuthChallengeUser
ARG DnsAuthChallengePw
ARG DnsAuthChallengeFullDomain
ARG DnsAuthChallengeSubDomain
ARG DomainSslCertPw
ARG DomainSslCertPwConfigKey
ARG DomainSslCertValueConfigKey
ARG DbHostVariable
ARG DbUserVariable
ARG DbPasswordVariable

ENV DnsAuthChallengeUser ${DnsAuthChallengeUser}
ENV DnsAuthChallengePw ${DnsAuthChallengePw}
ENV DnsAuthChallengeFullDomain ${DnsAuthChallengeFullDomain}
ENV DnsAuthChallengeSubDomain ${DnsAuthChallengeSubDomain}
ENV DomainSslCertPw ${DomainSslCertPw}
ENV DomainSslCertPwConfigKey ${DomainSslCertPwConfigKey}
ENV DomainSslCertValueConfigKey ${DomainSslCertValueConfigKey}
ENV DbHostVariable ${DbHostVariable}
ENV DbUserVariable ${DbUserVariable}
ENV DbPasswordVariable ${DbPasswordVariable}

RUN apt-get update && apt-get install -y letsencrypt python3 wget curl

RUN wget https://bootstrap.pypa.io/get-pip.py
RUN python3 get-pip.py
RUN python3 -m pip install mysql-connector-python

COPY ./acme-dns-auth.py /etc/letsencrypt/acme-dns-auth.py
COPY ./update_db.py /etc/letsencrypt/update_db.py
COPY ./acmedns.json /etc/letsencrypt/acmedns.json
COPY ./start.sh /start.sh

RUN sed -i 's/DnsAuthChallengeUser/'"$DnsAuthChallengeUser"'/g' /etc/letsencrypt/acmedns.json
RUN sed -i 's/DnsAuthChallengePw/'"$DnsAuthChallengePw"'/g' /etc/letsencrypt/acmedns.json
RUN sed -i 's/DnsAuthChallengeFullDomain/'"$DnsAuthChallengeFullDomain"'/g' /etc/letsencrypt/acmedns.json
RUN sed -i 's/DnsAuthChallengeSubDomain/'"$DnsAuthChallengeSubDomain"'/g' /etc/letsencrypt/acmedns.json

RUN chmod +x /etc/letsencrypt/acme-dns-auth.py
RUN chmod +x /etc/letsencrypt/update_db.py
RUN chmod +x /start.sh

CMD ["/start.sh"]
