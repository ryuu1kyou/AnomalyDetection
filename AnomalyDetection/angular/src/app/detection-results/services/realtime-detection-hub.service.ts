import { Injectable, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AnomalyDetectionResult } from '../models/detection-result.model';

export enum HubConnectionState {
  Disconnected = 'Disconnected',
  Connecting = 'Connecting',
  Connected = 'Connected',
  Reconnecting = 'Reconnecting',
  Error = 'Error',
}

@Injectable({
  providedIn: 'root',
})
export class RealtimeDetectionHubService implements OnDestroy {
  private hubConnection: signalR.HubConnection | null = null;
  private connectionState$ = new BehaviorSubject<HubConnectionState>(
    HubConnectionState.Disconnected
  );
  private newDetectionResult$ = new BehaviorSubject<AnomalyDetectionResult | null>(null);
  private detectionResultUpdated$ = new BehaviorSubject<AnomalyDetectionResult | null>(null);
  private detectionResultDeleted$ = new BehaviorSubject<string | null>(null);

  constructor() {}

  /**
   * Get the current connection state
   */
  get connectionState(): Observable<HubConnectionState> {
    return this.connectionState$.asObservable();
  }

  /**
   * Observable for new detection results
   */
  get onNewDetectionResult(): Observable<AnomalyDetectionResult | null> {
    return this.newDetectionResult$.asObservable();
  }

  /**
   * Observable for updated detection results
   */
  get onDetectionResultUpdated(): Observable<AnomalyDetectionResult | null> {
    return this.detectionResultUpdated$.asObservable();
  }

  /**
   * Observable for deleted detection results
   */
  get onDetectionResultDeleted(): Observable<string | null> {
    return this.detectionResultDeleted$.asObservable();
  }

  /**
   * Start the SignalR connection
   */
  async startConnection(accessToken?: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      console.log('SignalR connection already established');
      return;
    }

    try {
      this.connectionState$.next(HubConnectionState.Connecting);

      // Build connection with automatic reconnection
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(
          environment.signalR?.detectionHubUrl || 'https://localhost:44318/signalr-hubs/detection',
          {
            accessTokenFactory: () => accessToken || '',
            skipNegotiation: false,
            transport:
              signalR.HttpTransportType.WebSockets |
              signalR.HttpTransportType.ServerSentEvents |
              signalR.HttpTransportType.LongPolling,
          }
        )
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: retryContext => {
            // Exponential backoff: 0s, 2s, 10s, 30s, then 30s intervals
            if (retryContext.previousRetryCount === 0) return 0;
            if (retryContext.previousRetryCount === 1) return 2000;
            if (retryContext.previousRetryCount === 2) return 10000;
            return 30000;
          },
        })
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Register event handlers
      this.registerEventHandlers();

      // Register reconnection handlers
      this.hubConnection.onreconnecting(error => {
        console.warn('SignalR reconnecting...', error);
        this.connectionState$.next(HubConnectionState.Reconnecting);
      });

      this.hubConnection.onreconnected(connectionId => {
        console.log('SignalR reconnected. Connection ID:', connectionId);
        this.connectionState$.next(HubConnectionState.Connected);
      });

      this.hubConnection.onclose(error => {
        console.error('SignalR connection closed', error);
        this.connectionState$.next(HubConnectionState.Disconnected);
      });

      // Start the connection
      await this.hubConnection.start();
      console.log('SignalR connection established successfully');
      this.connectionState$.next(HubConnectionState.Connected);
    } catch (error) {
      console.error('Error starting SignalR connection:', error);
      this.connectionState$.next(HubConnectionState.Error);
      throw error;
    }
  }

  /**
   * Stop the SignalR connection
   */
  async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.stop();
        console.log('SignalR connection stopped');
        this.connectionState$.next(HubConnectionState.Disconnected);
      } catch (error) {
        console.error('Error stopping SignalR connection:', error);
      }
    }
  }

  /**
   * Subscribe to detection results for a specific project
   */
  async subscribeToProject(projectId: string): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      console.warn('Cannot subscribe to project: SignalR not connected');
      return;
    }

    try {
      await this.hubConnection.invoke('SubscribeToProject', projectId);
      console.log(`Subscribed to project: ${projectId}`);
    } catch (error) {
      console.error('Error subscribing to project:', error);
      throw error;
    }
  }

  /**
   * Unsubscribe from detection results for a specific project
   */
  async unsubscribeFromProject(projectId: string): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      console.warn('Cannot unsubscribe from project: SignalR not connected');
      return;
    }

    try {
      await this.hubConnection.invoke('UnsubscribeFromProject', projectId);
      console.log(`Unsubscribed from project: ${projectId}`);
    } catch (error) {
      console.error('Error unsubscribing from project:', error);
      throw error;
    }
  }

  /**
   * Subscribe to all detection results (admin/global monitoring)
   */
  async subscribeToAllResults(): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      console.warn('Cannot subscribe to all results: SignalR not connected');
      return;
    }

    try {
      await this.hubConnection.invoke('SubscribeToAllResults');
      console.log('Subscribed to all detection results');
    } catch (error) {
      console.error('Error subscribing to all results:', error);
      throw error;
    }
  }

  /**
   * Unsubscribe from all detection results
   */
  async unsubscribeFromAllResults(): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      console.warn('Cannot unsubscribe from all results: SignalR not connected');
      return;
    }

    try {
      await this.hubConnection.invoke('UnsubscribeFromAllResults');
      console.log('Unsubscribed from all detection results');
    } catch (error) {
      console.error('Error unsubscribing from all results:', error);
      throw error;
    }
  }

  /**
   * Register SignalR event handlers
   */
  private registerEventHandlers(): void {
    if (!this.hubConnection) return;

    // Handle new detection result
    this.hubConnection.on('ReceiveNewDetectionResult', (result: AnomalyDetectionResult) => {
      console.log('New detection result received:', result);
      this.newDetectionResult$.next(result);
    });

    // Handle detection result update
    this.hubConnection.on('ReceiveDetectionResultUpdate', (result: AnomalyDetectionResult) => {
      console.log('Detection result updated:', result);
      this.detectionResultUpdated$.next(result);
    });

    // Handle detection result deletion
    this.hubConnection.on('ReceiveDetectionResultDeletion', (resultId: string) => {
      console.log('Detection result deleted:', resultId);
      this.detectionResultDeleted$.next(resultId);
    });

    // Handle batch updates (for high-throughput scenarios)
    this.hubConnection.on('ReceiveBatchDetectionResults', (results: AnomalyDetectionResult[]) => {
      console.log(`Batch of ${results.length} detection results received`);
      // Emit each result individually to maintain consistency
      results.forEach(result => {
        this.newDetectionResult$.next(result);
      });
    });
  }

  /**
   * Get the current connection ID (useful for debugging)
   */
  get connectionId(): string | null {
    return this.hubConnection?.connectionId || null;
  }

  /**
   * Check if the connection is active
   */
  get isConnected(): boolean {
    return this.hubConnection?.state === signalR.HubConnectionState.Connected;
  }

  ngOnDestroy(): void {
    this.stopConnection();
    this.connectionState$.complete();
    this.newDetectionResult$.complete();
    this.detectionResultUpdated$.complete();
    this.detectionResultDeleted$.complete();
  }
}
