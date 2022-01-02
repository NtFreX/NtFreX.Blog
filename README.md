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

 **More**

 - [Deploy](DEPLOY.md)
 - [Todo](TODO.md)
