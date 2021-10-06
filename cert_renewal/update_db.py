import os
import time
import base64
import mysql.connector

cert = ""
try:
    print("Reading the base64 cert file")
    with open('/tmp/.certbot/config/live/ntfrex.com/cert_base64.pfx') as f:
        cert = f.read().replace("\n", "")
except Exception as e: 
    print("Reading the base64 cert file failed")
    print(e)

if not cert:
    exit(1)

# retry because the db could be stopped and needs some time for the cold start
retries = 5
for i in range(retries):
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
        cursor = db.cursor()

        print("Writing new cert and pw to db")
        cursor.execute("UPDATE configuration SET `value` = '" + cert + "' WHERE `key` = '" + os.environ["DomainSslCertValueConfigKey"] + "'")
        print(cursor.rowcount, "record(s) affected") 
        cursor.execute("UPDATE configuration SET `value` = '" + os.environ["DomainSslCertPw"] + "' WHERE `key` = '" + os.environ["DomainSslCertPwConfigKey"] + "'")
        print(cursor.rowcount, "record(s) affected") 

        db.commit()
        cursor.close()
        db.close()

        print("Update successful")
        break
    except Exception as e: 
        print("Writing new cert and pw to db failed")
        print(e)

        if i + 1 == retries:
            exit(1)
        else:
            time.sleep(15)

exit(0)
