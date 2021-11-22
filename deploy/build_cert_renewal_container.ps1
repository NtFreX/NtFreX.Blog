$dockerRegistry = "059702969940.dkr.ecr.us-east-2.amazonaws.com"

docker build `
 --build-arg DomainSslCertPw="Pa55w0rd!+" `
 --build-arg DnsAuthChallengeUser="a68007e2-655e-44d2-9809-c9672d4690b5" `
 --build-arg DnsAuthChallengePw="WI3m5fMVzxkstLeHBuk4_kUvMJJtvLBkJ4NJ9j-g" `
 --build-arg DnsAuthChallengeFullDomain="2e4e4857-d5d2-427b-8e6f-fbcd72d37c51.auth.acme-dns.io" `
 --build-arg DnsAuthChallengeSubDomain="2e4e4857-d5d2-427b-8e6f-fbcd72d37c51" `
 --build-arg DomainSslCertPwConfigKey="NtFreX.Blog.Production.SslCertPw" `
 --build-arg DomainSslCertValueConfigKey="NtFreX.Blog.Production.SslCert" `
 --build-arg DbHostVariable="blog-1.cluster-c4pu8xerd0na.us-east-2.rds.amazonaws.com" `
 --build-arg DbUserVariable="admin" `
 --build-arg DbPasswordVariable="UGEkJHcwcmQhKw==" `
 -t ntfrexcertrenewal `
 ./infrastructure/prod/cert_renewal

aws ecr get-login-password --region us-east-2 | docker login --username AWS --password-stdin $dockerRegistry
docker tag ntfrexcertrenewal:latest $dockerRegistry/ntfrexcertrenewal:latest
docker push $dockerRegistry/ntfrexcertrenewal:latest
