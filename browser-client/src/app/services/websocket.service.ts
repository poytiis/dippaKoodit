import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { WebSocketSubject, webSocket } from 'rxjs/webSocket';
import { MessagePackHubProtocol } from '@microsoft/signalr-protocol-msgpack';  

@Injectable({
  providedIn: 'root'
})
export class WebsocketService {

  private webSocketConnection: WebSocket | null = null;
  // private webSocketConnection: WebSocket | null = null;
  private webSocketConnectionRx: WebSocketSubject<unknown> | null = null;

  private hubConnection: HubConnection | null = null;
  private webSocketLibrary: 'signalR' | 'default' | 'RX' | 'notSelected' = 'default';

  private waitForConnectionEstablished = 5;
  // public webSocketUrl = 'dippa.test:5000';
  public webSocketUrl = 'localhost:5000';
  public websocketHTTPUrl = 'http://' + this.webSocketUrl
  constructor() { }

  connect(protocol: 'json' | 'binary' = 'json'): void {
    if (this.webSocketLibrary === 'RX') {
      this.webSocketConnectionRx = webSocket('ws://' + this.webSocketUrl);
      this.webSocketConnectionRx.subscribe(resData => {
        console.log(resData)
      })
  
      
    }  else if(this.webSocketLibrary === 'default') {
      if(this.webSocketConnection === null) {
        this.webSocketConnection = new WebSocket('ws://' + this.webSocketUrl);
      }
      
      if(this.webSocketConnection) {
        this.webSocketConnection.onmessage = (event) => {
          console.log(event.data);
        }     
      }
    } else if(this.webSocketLibrary === 'signalR') {
      const localUrl = this.websocketHTTPUrl
      if(protocol === 'binary') {
        this.hubConnection = new HubConnectionBuilder()
          .withUrl(localUrl + '/hub/upload')
          .withHubProtocol(new MessagePackHubProtocol())
          .build();
      } else {
        this.hubConnection = new HubConnectionBuilder()
        .withUrl(localUrl + '/hub/upload')
        .build();
      }
    
      this.hubConnection.on('messageReceived', message => {
       console.log(message)
      });
  
      this.hubConnection.start();
    }
   
    
  }

  async sendData(data: WebsocketMessage) {
    if (this.webSocketLibrary === 'RX') {
      this.webSocketConnectionRx?.next(data);
    } else if(this.webSocketLibrary === 'default') {
      const dataStr = JSON.stringify(data);
      while(this.waitForConnectionEstablished !== 0) {
        if (this.webSocketConnection?.readyState === this.webSocketConnection?.CONNECTING) {
          new Promise(resolve => setTimeout(resolve, 2000));
          this.waitForConnectionEstablished --;
          continue;
        } else if (this.webSocketConnection?.readyState === this.webSocketConnection?.OPEN) {
          console.log('Connection open')
          break;
        }
      }
      if (this.waitForConnectionEstablished === 0) {
        console.log('failed to connect to server');
        return;
      }    
      
      this.webSocketConnection?.send(dataStr);
    }
    else if(this.webSocketLibrary === 'signalR') {
      const dataStr = JSON.stringify(data);
      await this.hubConnection?.invoke("UploadFilePiece", dataStr);
    }
  }

  async sendDataBinary(data: ArrayBuffer) {
    console.log(data)
    if(this.webSocketLibrary === 'default') {
      this.webSocketConnection?.send(data);
    } else if(this.webSocketLibrary === 'signalR'){
      // const strData = String.fromCharCode.apply(null, new Uint16Array(data));
      var uint8View = new Uint8Array(data);
      const array = [...uint8View];
      await this.hubConnection?.invoke("UploadFilePieceArray", array);
    }
   
  }

  async sendBinaryAsync (data: ArrayBuffer){
    if(this.webSocketLibrary === 'signalR') {
      await this.hubConnection?.invoke("UploadFilePiece", data);
    }
  }


  close(): void  {
    if(this.webSocketLibrary === 'RX') {
      this.webSocketConnectionRx?.complete();
    } else if(this.webSocketLibrary === 'signalR'){
      this.hubConnection?.stop();
    } else {
      this.webSocketConnection?.close();
    }
   
  }

  reOpenConnection(protocol: 'json' | 'binary' = 'json') {
    console.log('reopen')
    this.hubConnection?.stop();
    const localUrl = this.websocketHTTPUrl
    if(protocol === 'binary') {
      this.hubConnection = new HubConnectionBuilder()
        .withUrl(localUrl + '/hub/upload')
        .withHubProtocol(new MessagePackHubProtocol())
        .build();
    } else {
      this.hubConnection = new HubConnectionBuilder()
      .withUrl(localUrl + '/hub/upload')
      .build();
    }

  }
}

export interface WebsocketMessage {
  FileName: string;
  PieceData?: number [];
  PieceNumber?: number;
  MessageType: string
}
