This is the source code of my blog. See it live here: https://ntfrex.com

**Dependencies**

 - Redis
 - MongoDb

**Build and run localy**

```
dotnet run --project .\NtFreX.Blog\NtFreX.Blog.csproj
```

**What I run to deploy on my server**

```
git pull \
	&& cp -rf /mnt/nas/ftr/blog.ntfrex.com/ntfrex.com.pfx ./NtFreX.Blog \
	&& sudo rm -r nuget/ && sudo cp -r /mnt/nas/ftr/nuget ./nuget \
	&& docker build -f "./Dockerfile" --force-rm -t ntfrexblog "./" \
	&& sudo systemctl restart blog \
	&& sudo journalctl -u blog -f -n 10
```

I have setup a systemd service named blog which runs my docker image similar to the following command.
```
docker run -e NTFREXBLOGCERTPWD -p 80:80 -p 443:443 --name blog ntfrexblog
```


**TODO**

 - Use ASPNETCORE_URLS env variable from launchSettings.json to setup kestrel listeners (get rid of multiple configuration locations)
 - Use certificate store for production environment
 - Setup Blazor Hybrid
 - Randomize cache livetime so not all caches are invalidated at the same time
 - restricted /Private folder access doesn't work
 - cache blazor pages by route
 - rolling updates
 - deployment pipeline with pre prod stage
 - ...