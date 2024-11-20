import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
    .withUrl('/notification-hub') // Ensure this matches the backend SignalR endpoint
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

export async function startConnection() {
    try {
        await connection.start();
        console.log('SignalR Connected');
    } catch (err) {
        console.error('SignalR Connection Error:', err);
        setTimeout(startConnection, 5000); // Retry after 5 seconds
    }
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function onReceiveLog(callback: any) {
    connection.on('ReceiveLog', callback);
}
// // Reconnection state handling
// connection.onreconnecting((error) => {
//     console.warn('SignalR reconnecting...', error);
//     // Optionally notify the user
// });

// connection.onreconnected((connectionId) => {
//     console.log('SignalR reconnected. Connection ID:', connectionId);
//     // Optionally notify the user
// });

// connection.onclose(async (error) => {
//     console.error('SignalR connection closed. Attempting to reconnect...', error);
//     await startConnection(); // Restart connection on close
// });
export default connection;
