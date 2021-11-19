This is the source code of my blog. A great place to test things in a used production environment. See it live here: https://ntfrex.com

**Build and run localy**

 - Set a value for NtFrexMySqlConfigPw in launchSettings.json
 - Set a value for MYSQL_ROOT_PASSWORD in local_infra.yaml
 - Set a value for Pwd in the NtFreX.Blog.Development.MySqlDbConnectionString connection string in 01-databases.sql

```
docker-compose -f .\infrastructure\dev\local_infra.yaml up --force-recreate --abort-on-container-exit
dotnet run --project .\NtFreX.Blog\NtFreX.Blog.csproj
```

 - browse https://localhost:5001/ to view the blog
 - browse http://localhost:9411/ for Zipkin
 - browse http://localhost:9090/ for Prometheus
 - mysql is running on 127.0.0.1:3306

**Dependencies**

 - Mysql/MongoDb
 - (Redis)
 - [to be removed]  NtFreX.Core/ NtFreX.Core.Web / NtFreX.Audio / NtFreX.Audio.Infrastructure / NtFreX.ConfigFlow.DotNet

 **Deployment**
 
To configure the production environment you can set environment variables in the `https-instance-single.config` file.
To configure/customize this project you can start with the Configuration.cs file in the NtFreX.Blog.Configuration project. It contains a lot of options to play around. (Examples can be found in `01-databases.sql`)
For all values in the ConfigNames class a value must be set where the choosen configuration provider can find it.
Next you might want to add your own privacy.txt, ads,txt, manifest.json, references.txt, robots.txt, security.txt and terms.html.
The `aws-beanstalk-tools-defaults.json` file can be used to deploy the beanstalk app with the aws cli.

There is IaC for the health check lambda but you need deploy the code yourself.

To build the SSL certificate renewal container you can set the variables in the `build_cert_renewal_container.ps1` file.
 - Note: The DbPasswordVariable needs to be a base64 encoded value to support all special characters.
 - Note: The acme credentials need to be resoved manualy.
 - Note: The lambda for the cert renewal container with event (rate(2months)) has to be created manually.

**TODO**

 - init script to replace variables
 - integration tests
 - dashboard&alarms
 - deployment pipeline
   - for web app
 - static code analyzis (dependency security, code security, improvements, etc)
 - dependabot


 - host own acme instance in cert renewal container
 - setup prod infrastructure if not exits in pipeline
   - dashboard alarms, mongodb/rds, elastic beanstalk, ec2 config, (redis), lambda (cert & health), ecs, networking, security, cloudflare?)
   - setup db's and tables if not exits in prod
 - add and remove nat gateway before and after cert renewal to avoid cost (automaticaly, iac exits) 
 - deployment pipeline
   - pre prod stages (canaries&bake time)
   - for cert renewal container
   - for health check lambda
   - cleanup cloudfare cache after release


 - randomize cache livetime so not all caches are invalidated at the same time
 - cache blazor pages by route
 - minify at build/release and disable minification in cloudflare
 - code first + migrations
 - upvote or downvote article
 - share article
 - page metadata (SEO)
 - exclude own page visits (counts)
 - optimize db queries
 - make everything configurable
 - swagger
 - lighthouse checks
 - improve redirection to https
 - improve css for loading tag on mobile
 - more logging, metrics, tracing, comments and docu


 - ...
