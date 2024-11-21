import * as signalR from '@microsoft/signalr';

// @ts-ignore
const notificationHubUrl = import.meta.env.VITE_NOTIFICATION_HUB_URL ? `${import.meta.env.VITE_NOTIFICATION_HUB_URL}/notification-hub` : '/notification-hub';
const connection = new signalR.HubConnectionBuilder()
    .withUrl(notificationHubUrl) // Ensure this matches the backend SignalR endpoint
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

export async function stopConnection() {
    try {
        await connection.stop();
        console.log('SignalR Disconnected');
    } catch (err) {
        console.error('Error while disconnecting SignalR:', err);
    }
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function onReceiveLog(callback: any) {
    connection.on('ReceiveLog', callback);
}
export default connection;
