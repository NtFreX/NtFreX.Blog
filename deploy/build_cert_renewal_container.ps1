$dockerRegistry = "#dockerRegistry#"

docker build `
 --build-arg DomainSslCertPw="#domainSslCertPw#" `
 --build-arg DnsAuthChallengeUser="#dnsAuthChallengeUser#" `
 --build-arg DnsAuthChallengePw="#dnsAuthChallengePw#" `
 --build-arg DnsAuthChallengeFullDomain="#dnsAuthChallengeFullDomain#" `
 --build-arg DnsAuthChallengeSubDomain="#dnsAuthChallengeSubDomain#" `
 --build-arg DomainSslCertPwConfigKey="#domainSslCertPwConfigKey#" `
 --build-arg DomainSslCertValueConfigKey="domainSslCertValueConfigKey" `
 --build-arg DbHostVariable="dbHostVariable" `
 --build-arg DbUserVariable="dbUserVariable" `
 --build-arg DbPasswordVariable="dbPasswordVariable" `
 -t ntfrexcertrenewal `
 ./infrastructure/prod/cert_renewal

aws ecr get-login-password --region us-east-2 | docker login --username AWS --password-stdin $dockerRegistry
docker tag ntfrexcertrenewal:latest $dockerRegistry/ntfrexcertrenewal:latest
docker push $dockerRegistry/ntfrexcertrenewal:latest
