This is the source code of my blog. See it live here: https://ntfrex.com

**Dependencies**

 - Mysql/MongoDb
 - (Redis)

 **Configuration**
 
To configure the production environment you can set environment variables in the `https-instance-single.config` file.
To configure the development environment you can set env variables in the `launchSettings.json` file. 
To build the SSL certificate renewal container you can set the variables in the `build_cert_renewal_container.ps1` file.
 - Note: The DbPasswordVariable needs to be a base64 encoded value to support all special characters

To configure/customize this project you can start with the Configuration.cs file in the NtFreX.Blog.Configuration project. It contains a lot of options to play around. 
For all values in the ConfigNames class a value must be set where the choosen configuration provider can find it.
Next you might want to add your own privacy.txt, ads,txt, manifest.json, references.txt, robots.txt, security.txt and terms.html.
As there is currently no IaC you need to setup your dependencies and prod environment manualy. 

TODO: NtFreX.** dependencies (one command build/run)

**Build and run localy**

```
dotnet run --project .\NtFreX.Blog\NtFreX.Blog.csproj
```

**TODO**

 - deployment pipeline with pre prod stage (pipeline for cert renewal container)
 - host own acme instance in cert renewal container
  
 - dashboard&alarms
 - add and remove nat gateway before and after cert renewal to avoid cost
 - integration tests

 - IaC (dashboard alarms, mongodb, rds, elastic beanstalk, ec2 config, redis, cert renewal lambda, networking, security, dev env? sql install script?)
 
 - opentelemetry-dotnet, maybe also for lambda (https://aws.amazon.com/blogs/opensource/aws-distro-for-opentelemetry-adds-net-tracing-support/, https://github.com/open-telemetry/opentelemetry-dotnet)
 - cleanup cloudfare cache after release
 - static code analyzis (dependency security, code security, improvements, etc)
 - Randomize cache livetime so not all caches are invalidated at the same time
 - cache blazor pages by route
 - code first + migrations
 - upvote or downvote article
 - share article
 - page metadata (SEO)
 - exclude own page visits (counts)
 - optimize db queries
 - make everything configurable
 - swagger?
 - lighthouse checks
 - more logging and docu
 - improve redirection to https
 - ...

  - minify at build/release and disable for cloudflare
  - improve css for loading tag on mobile