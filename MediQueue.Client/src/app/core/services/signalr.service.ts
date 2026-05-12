import { Injectable, inject, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel, HttpTransportType } from '@microsoft/signalr';
import { AuthService } from '../auth/auth.service';
import { NotificationService } from './notification.service';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private readonly auth          = inject(AuthService);
  private readonly notifications = inject(NotificationService);

  private hub?: HubConnection;
  readonly connectionState = signal<'connected'|'connecting'|'disconnected'>('disconnected');
  readonly notificationCount = signal(0);

  async connect(): Promise<void> {
    if (this.hub?.state === HubConnectionState.Connected) return;

    this.connectionState.set('connecting');

    this.hub = new HubConnectionBuilder()
      .withUrl('/hubs/clinic', {
        accessTokenFactory: () => this.auth.getToken() ?? '',
        transport: HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build();

    // ═══ Events from Backend (ClinicHub)
    this.hub.on('AppointmentBooked', (data: any) => {
      this.notifications.info(
        `New Appointment: ${data.patientName} with Dr. ${data.doctorName}`
      );
      this.incrementCount();
    });

    this.hub.on('VisitStarted', (data: any) => {
      this.notifications.success(
        `Visit Started: ${data.patientName} — Dr. ${data.doctorName}`
      );
      this.incrementCount();
    });

    this.hub.on('InvoicePaid', (data: any) => {
      this.notifications.success(
        `Payment Received: Invoice #${data.invoiceNumber} — EGP ${data.amount.toLocaleString()}`
      );
      this.incrementCount();
    });

    // ═══ Reconnection handlers
    this.hub.onreconnecting(() => this.connectionState.set('connecting'));
    this.hub.onreconnected(() => this.connectionState.set('connected'));
    this.hub.onclose(() => this.connectionState.set('disconnected'));

    try {
      await this.hub.start();
      this.connectionState.set('connected');
      console.log('SignalR Connected');
    } catch (err) {
      this.connectionState.set('disconnected');
      console.error('SignalR failed to connect:', err);
    }
  }

  clearNotifications(): void {
    this.notificationCount.set(0);
  }

  private incrementCount(): void {
    this.notificationCount.update(n => n + 1);
  }

  async disconnect(): Promise<void> {
    await this.hub?.stop();
    this.connectionState.set('disconnected');
  }
}
