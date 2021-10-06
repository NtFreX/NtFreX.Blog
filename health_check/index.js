const https = require('https');

exports.handler = async (event) => {
    return new Promise((resolve, reject) => {
        https.get("https://www.ntfrex.com/health", (response) => {
            let body = ''; 
            response.on('data', chunk => body += chunk);
            response.on('end', () => {
                if(body == "Healthy") {
                    resolve({ statusCode: 200, body: "Healthy" });
                } else {
                    resolve({
                        statusCode: 400,
                        body: body
                    });
                }
            });
        }).on('error', (e) => {
            reject({ 
                statusCode: 500,
                body: JSON.stringify(exception)
            });
        });        
    });
};
