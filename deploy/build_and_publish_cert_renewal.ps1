$dockerRegistry = "$env:VAR_DOCKERREGISTRY"

docker build `
 --build-arg DomainSslCertPw="$env:VAR_DOMAINSSLCERTPW" `
 --build-arg DnsAuthChallengeUser="$env:VAR_DNSAUTHCHALLENGEUSER" `
 --build-arg DnsAuthChallengePw="$env:VAR_DNSAUTHCHALLENGEPW" `
 --build-arg DnsAuthChallengeFullDomain="$env:VAR_DNSAUTHCHALLENGEFULLDOMAIN" `
 --build-arg DnsAuthChallengeSubDomain="$env:VAR_DNSAUTHCHALLENGESUBDOMAIN" `
 --build-arg DomainSslCertPwConfigKey="$env:VAR_DOMAINSSLCERTPWCONFIGKEY" `
 --build-arg DomainSslCertValueConfigKey="$env:VAR_DOMAINSSLCERTVALUECONFIGKEY" `
 --build-arg DbHostVariable="$env:VAR_DBHOSTVARIABLE" `
 --build-arg DbUserVariable="$env:VAR_DBUSERVARIABLE" `
 --build-arg DbPasswordVariable="$env:VAR_DBPASSWORDVARIABLE" `
 -t ntfrexcertrenewal `
 ./infrastructure/prod/cert_renewal

aws ecr get-login-password --region us-east-2 | docker login --username AWS --password-stdin $dockerRegistry
docker tag ntfrexcertrenewal:latest $dockerRegistry/ntfrexcertrenewal:latest
docker push $dockerRegistry/ntfrexcertrenewal:latest
