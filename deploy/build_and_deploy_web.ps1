$environment = "#environment#"
$mysqlConfigPw = "#mysqlConfigPw#"
$mysqlConfigUser = "#mysqlConfigUser#"
$mysqlConfigServer = "#mysqlConfigServer#"
$configSecret = "#configSecret#"
$s3bucket = "#s3bucket#"
$app = "#app#"
$ebsEnv = "#ebsEnv#"


dotnet publish .\NtFreX.Blog\NtFreX.Blog.csproj --self-contained true --runtime linux-x64 --configuration Release


Copy-Item ./NtFreX.Blog/bin/Release/net5.0/linux-x64/publish ./publish -Recurse
(Get-Content ./publish/.ebextensions/ebs.config).replace('#AspNetCoreEnv', $environment) | Set-Content ./publish/.ebextensions/ebs.config
(Get-Content ./publish/.ebextensions/ebs.config).replace('#MySqlConfigPw', $mysqlConfigPw) | Set-Content ./publish/.ebextensions/ebs.config
(Get-Content ./publish/.ebextensions/ebs.config).replace('#MySqlConfigServer', $mysqlConfigServer) | Set-Content ./publish/.ebextensions/ebs.config
(Get-Content ./publish/.ebextensions/ebs.config).replace('#MySqlConfigUser', $mysqlConfigUser) | Set-Content ./publish/.ebextensions/ebs.config
(Get-Content ./publish/.ebextensions/ebs.config).replace('#ConfigSecret', $configSecret) | Set-Content ./publish/.ebextensions/ebs.config

tar -C ./publish -vacf publish.zip .

Remove-Item ./publish -Recurse


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
  --option-settings `
  Namespace=aws:ec2:instances,OptionName=InstanceTypes,Value=t2.nano `
  Namespace=aws:elasticbeanstalk:environment,OptionName=EnvironmentType,Value=SingleInstance `
  Namespace=aws:elasticbeanstalk:xray,OptionName=XRayEnabled,Value=true `
  Namespace=aws:elasticbeanstalk:healthreporting:system,OptionName=SystemType,Value=enhanced

echo "deployed application " + $version + " with source " + $filename