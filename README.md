This is the source code of my blog. A great place to test things in a used production environment. See it live here: https://ntfrex.com

**Build and run localy**

If you want you can change the default password or other options of the local development infrastructure in the following files.

 - launchSettings.json
 - local_infra.yaml
 - 01-databases.sql
 - more...

Runing the development dependencies

```
docker-compose -f .\infrastructure\dev\local_infra.yaml up --force-recreate --abort-on-container-exit
```

Runing the web project

```
dotnet run --project .\NtFreX.Blog\NtFreX.Blog.csproj
```

 - browse https://localhost:5001/ to view the blog
 - browse http://localhost:9411/ for Zipkin
 - browse http://localhost:9090/ for Prometheus
 - browse http://localhost:3000/ for Grafana
 - browse http://localhost:15672/ for RabbitMq
 - mysql is running on 127.0.0.1:3306
 - redis is running on 127.0.0.1:6022

**Dependencies**

 - Mysql/MongoDb
 - (Redis)
 - (RabbitMq/Aws SQS/Aws Event bridge)

 **Deployment**
 
To configure the production environment you can set environment variables in the 'build_and_deploy_web.ps1' and 'build_and_publish_cert_renewal.ps1' files.
To configure/customize this project you can start with the Configuration.cs file in the NtFreX.Blog.Configuration project. It contains a lot of options to play around. (Examples can be found in `01-databases.sql`)
For all values in the ConfigNames class a value must be set where the choosen configuration provider can find it.
Next you might want to add your own privacy.txt, ads,txt, manifest.json, references.txt, robots.txt, security.txt and terms.html.

There is IaC for the health check lambda only. The web deploy script does only update an ebs environment and not create any if it does not exist.

To build the SSL certificate renewal container you can set the variables in the `build_and_publish_cert_renewal.ps1` file.
 - Note: The DbPasswordVariable needs to be a base64 encoded value to support all special characters.
 - Note: The acme credentials need to be resoved manualy.
 - Note: The lambda for the cert renewal container with event (rate(2months)) has to be created manually.

**Feature ideas**

 - upvote or downvote article
 - emoji for article
 - make subtitle optional

**Nice to have**

 - improve deploy
   - Resources creation (IaC)
     - dashboard alarms, mongodb/rds, elastic beanstalk, ec2 config, (redis), lambda (cert & health), ecs, networking, security, cloudflare?)
     - setup db's and tables if not exits in prod (ef core migrations (code first))
       - seed data using models and not sql file (startup)
   - Command execution (testing/documentation)
   - pre prod stages (canaries(selenium, e2e)&bake time)
   - for cert renewal container
     - install newest cert renewal container in lambda after publish
   - for health check lambda
   - cleanup cloudfare cache after release?
 - possibility to disable comments
 - possibility to load articles from disc (disable edit mode in this case (in prod only?))
 - more metrics/event counters for opentel (gc, etc)
 - init script to replace/set variables
 - integration tests
 - static code analyzis (next to codeql and dependabot, dependency security, code security, etc)
 - server side and client side model validation (componentmodel)
 - polly for razor client retry strat
 - host own acme instance in cert renewal container
 - add and remove nat gateway before and after cert renewal to avoid cost (automaticaly, iac exits)
 - public class GoogleTwoFactorAuthenticator : ITwoFactorAuthenticator https://www.nuget.org/packages/GoogleAuthenticator
 - client usage analythics
 - randomize cache livetime so not all caches are invalidated at the same time?
 - cache blazor pages by route
 - minify at build/release and disable minification in cloudflare
 - page metadata (SEO)
   - image SEO
 - exclude own page visits (counts)
 - optimize db queries
 - make everything configurable
 - swagger?
 - lighthouse checks
 - improve redirection to https
 - improve css for loading tag on mobile
 - more logging, metrics, tracing, comments and docu
 - validate functionality of each share article
 - ...
