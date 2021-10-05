docker build `
 --build-arg DomainSslCertPw="" `
 --build-arg DnsAuthChallengeUser="" `
 --build-arg DnsAuthChallengePw="" `
 --build-arg DnsAuthChallengeFullDomain="" `
 --build-arg DnsAuthChallengeSubDomain="" `
 --build-arg DomainSslCertPwConfigKey="" `
 --build-arg DomainSslCertValueConfigKey="" `
 --build-arg DbHostVariable="" `
 --build-arg DbUserVariable="" `
 --build-arg DbPasswordVariable="" `
 -t ntfrex/certrenewal `
 ./cert_renewal