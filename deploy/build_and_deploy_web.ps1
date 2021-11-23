$environment = "$env:VAR_ENVIRONMENT"
$mysqlConfigPw = "$env:VAR_MYSQLCONFIGPW"
$mysqlConfigUser = "$env:VAR_MYSQLCONFIGUSER"
$mysqlConfigServer = "$env:VAR_MYSQLCONFIGSERVER"
$configSecret = "$env:VAR_CONFIGSECRET"
$configPath = "$env:VAR_CONFIGPATH"
$s3bucket = "$env:VAR_S3BUCKET"
$app = "$env:VAR_APP"
$ebsEnv = "$env:VAR_EBSENV"


dotnet publish .\NtFreX.Blog\NtFreX.Blog.csproj --self-contained true --runtime linux-x64 --configuration Release


Copy-Item ./NtFreX.Blog/bin/Release/net5.0/linux-x64/publish ./publish -Recurse
(Get-Content ./publish/.ebextensions/ebs.yml).replace('#AspNetCoreEnv', $environment) | Set-Content ./publish/.ebextensions/ebs.yml
(Get-Content ./publish/.ebextensions/ebs.yml).replace('#MySqlConfigPw', $mysqlConfigPw) | Set-Content ./publish/.ebextensions/ebs.yml
(Get-Content ./publish/.ebextensions/ebs.yml).replace('#MySqlConfigServer', $mysqlConfigServer) | Set-Content ./publish/.ebextensions/ebs.yml
(Get-Content ./publish/.ebextensions/ebs.yml).replace('#MySqlConfigUser', $mysqlConfigUser) | Set-Content ./publish/.ebextensions/ebs.yml
(Get-Content ./publish/.ebextensions/ebs.yml).replace('#ConfigSecret', $configSecret) | Set-Content ./publish/.ebextensions/ebs.yml
(Get-Content ./publish/.ebextensions/ebs.yml).replace('#ConfigPath', $configPath) | Set-Content ./publish/.ebextensions/ebs.yml

tar -C ./publish -vacf publish.zip .

$date = Get-Date -Format "yyyyMMddHHmmss"
$version = "v" + $date
$filename = "AWSDeploymentArchive_" + $app + "_" + $version + ".zip"
$description = $app + $version

aws s3 cp publish.zip s3://$s3bucket/$app/$filename

Remove-Item publish.zip

aws elasticbeanstalk create-application-version `
  --application-name $app `
  --version-label $version `
  --description $description `
  --source-bundle S3Bucket=$s3bucket,S3Key=$app/$filename

aws elasticbeanstalk update-environment `
  --region us-east-2 `
  --application-name $app `
  --environment-name $app-$ebsEnv `
  --solution-stack-name "64bit Amazon Linux 2 v2.2.6 running .NET Core" `
  --version-label $version `
  --option-settings file://publish/.ebextensions/ebs.config

Remove-Item ./publish -Recurse

echo "deployed application " + $version + " with source " + $filename