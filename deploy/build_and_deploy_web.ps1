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
(Get-Content ./publish/.ebextensions/ebs.config).replace('#AspNetCoreEnv', $environment) | Set-Content ./publish/.ebextensions/ebs.config
(Get-Content ./publish/.ebextensions/ebs.config).replace('#MySqlConfigPw', $mysqlConfigPw) | Set-Content ./publish/.ebextensions/ebs.config
(Get-Content ./publish/.ebextensions/ebs.config).replace('#MySqlConfigServer', $mysqlConfigServer) | Set-Content ./publish/.ebextensions/ebs.config
(Get-Content ./publish/.ebextensions/ebs.config).replace('#MySqlConfigUser', $mysqlConfigUser) | Set-Content ./publish/.ebextensions/ebs.config
(Get-Content ./publish/.ebextensions/ebs.config).replace('#ConfigSecret', $configSecret) | Set-Content ./publish/.ebextensions/ebs.config
(Get-Content ./publish/.ebextensions/ebs.config).replace('#ConfigPath', $configPath) | Set-Content ./publish/.ebextensions/ebs.config

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

# https://docs.aws.amazon.com/elasticbeanstalk/latest/dg/command-options-general.html

aws elasticbeanstalk update-environment `
  --region us-east-2 `
  --application-name $app `
  --environment-name $app-$ebsEnv `
  --solution-stack-name "64bit Amazon Linux 2 v2.2.8 running .NET Core" `
  --version-label $version `
  --option-settings `
  Namespace=aws:elasticbeanstalk:application:environment,OptionName=ASPNETCORE_ENVIRONMENT,Value=$environment `
  Namespace=aws:elasticbeanstalk:application:environment,OptionName=NtFrexMySqlConfigPw,Value=$mysqlConfigPw `
  Namespace=aws:elasticbeanstalk:application:environment,OptionName=NtFrexMySqlConfigServer,Value=$mysqlConfigServer `
  Namespace=aws:elasticbeanstalk:application:environment,OptionName=NtFrexMySqlConfigUser,Value=$mysqlConfigUser `
  Namespace=aws:elasticbeanstalk:application:environment,OptionName=NtFrexConfigSecret,Value=$configSecret `
  Namespace=aws:elasticbeanstalk:application:environment,OptionName=NtFrexConfigPath,Value=$configPath `
  Namespace=aws:elasticbeanstalk:cloudwatch:logs,OptionName=StreamLogs,Value=false `
  Namespace=aws:elasticbeanstalk:environment,OptionName=ServiceRole,Value=aws-elasticbeanstalk-service-role `
  Namespace=aws:elasticbeanstalk:environment,OptionName=EnvironmentType,Value=SingleInstance `
  Namespace=aws:autoscaling:launchconfiguration,OptionName=IamInstanceProfile,Value=aws-elasticbeanstalk-ec2-role `
  Namespace=aws:ec2:instances,OptionName=InstanceTypes,Value=t2.nano `
  Namespace=aws:elasticbeanstalk:xray,OptionName=XRayEnabled,Value=false `
  Namespace=aws:elasticbeanstalk:healthreporting:system,OptionName=SystemType,Value=enhanced

echo "deployed application " + $version + " with source " + $filename