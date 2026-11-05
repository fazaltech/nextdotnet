const { HubConnectionBuilder, HttpTransportType, LogLevel } = require('../../frontend/node_modules/@microsoft/signalr');
const connection = new HubConnectionBuilder().withUrl('http://127.0.0.1:5187/hubs/restaurant', {
  accessTokenFactory: () => process.env.RMS_TEST_TOKEN,
  transport: HttpTransportType.WebSockets,
  skipNegotiation: true,
}).configureLogging(LogLevel.None).build();
const timeout = setTimeout(() => process.exit(1), 12000);
connection.on('Changed', async () => {
  console.log('CHANGED'); clearTimeout(timeout); await connection.stop(); process.exit(0);
});
connection.start().then(() => console.log('CONNECTED')).catch(() => process.exit(1));
