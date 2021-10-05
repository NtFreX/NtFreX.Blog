This is the source code of my blog. See it live here: https://ntfrex.com

**Dependencies**

 - Mysql/MongoDb
 - (Redis)

 **Configuration**
 
To configure the production environment you can set environment variables in the `https-instance-single.config` file.
To configure the development environment you can set env variables in the `launchSettings.json` file. 
To build the SSL certificate renewal container you can set the variables in the `build_cert_renewal_container.ps1` file.

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

 - dashboard&alarms
 - IaC (dashboard alarms, mongodb, rds, elastic beanstalk, ec2 config, redis, cert renewal lambda, networking, security, dev env? sql install script?)
 - deployment pipeline with pre prod stage (pipeline for cert renewal container)
 - Randomize cache livetime so not all caches are invalidated at the same time
 - cache blazor pages by route
 - code first + migrations
 - upvote or downvote article
 - share article
 - page metadata (SEO)
 - exclude own page visits (counts)
 - optimize db queries
 - make everything configurable
 - ...
