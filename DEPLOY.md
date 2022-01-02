**Deploy yourself**

*Custom*
 
If you do not want to deploy to AWS EBS you can run the following command to get an self contained web application and delploy it to where ever you want. 
Keep in mind that you also need to deploy the dependencies and configure the application to your needs.

```
dotnet publish .\NtFreX.Blog\NtFreX.Blog.csproj --self-contained true --runtime linux-x64 --configuration Release
```

*Github actions*

Else you can use the infrastructure contained in this repository.

 - Copy/fork this repository
 - Create an AWS account
   - Use github_openid_provider.yaml in Cloudformation to allow github to publish new application versions
   (TODO: automate - Create EBS application and environemnt)
   (TODO: automate - Use ingress_security_rule.yaml in Cloudformation to allow direct connections on port 403)
   (TODO: automate - Create a sql or mongo database named "configuration" and "blog")
   (TODO: automate at initialization for each stage - Execute the inserts in 01-databases.sql with the desired configuration values)
 - Set the desired configration in the github environment variables for the variables used in build_and_deploy_web.ps1
 - Change `Configuration.cs`, `privacy.txt`, `ads,txt`, `manifest.json`, `references.txt`, `robots.txt`, `security.txt` and `terms.html` as desired.
 - Push to release branch
 - Setup DNS, Cloudfront, etc
  
*Other components*

To build the SSL certificate renewal container you can set the variables in the `build_and_publish_cert_renewal.ps1` file.
 - Note: The DbPasswordVariable needs to be a base64 encoded value to support all special characters.
 - Note: The acme credentials need to be resoved manualy.
 - Note: The lambda for the cert renewal container with event (rate(2months)) has to be created manually.

No code exists to build the health check lambda.