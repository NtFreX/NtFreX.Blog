import os
import time
import base64
import mysql.connector

# retry because the db could be stopped and needs some time for the cold start
for i in range(5):
    try:
        host = os.environ["DbHostVariable"]
        user = os.environ["DbUserVariable"]
        print("Connecting to " + host + " with user " + user) 

        db = mysql.connector.connect(
            host=host,
            user=user,
            password=base64.b64decode(os.environ["DbPasswordVariable"]).decode('utf-8'),
            database="configuration"
        )

        with open('/tmp/.certbot/config/live/ntfrex.com/cert_base64.pfx') as f:

            print("Writing new cert and pw to db")

            cursor = db.cursor()
            cursor.execute("UPDATE configuration SET `value` = '" + f.read().replace("\n", "") + "' WHERE `key` = '" + os.environ["DomainSslCertValueConfigKey"] + "'")
            print(cursor.rowcount, "record(s) affected") 

            cursor.execute("UPDATE configuration SET `value` = '" + os.environ["DomainSslCertPw"] + "' WHERE `key` = '" + os.environ["DomainSslCertPwConfigKey"] + "'")
            print(cursor.rowcount, "record(s) affected") 

            db.commit()
            cursor.close()
            db.close()

        print("Update successful")
        break
    except Exception as e: 
        print("An exception occurred")
        print(e)
        time.sleep(15)

print("Exiting")