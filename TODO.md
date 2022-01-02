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
