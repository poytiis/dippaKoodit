import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import  { WebsocketMessage, WebsocketService }  from '../../services/websocket.service';
import { HttpService} from '../../services/http.service';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

@Component({
  selector: 'app-upload-view',
  templateUrl: './upload-view.component.html',
  styleUrls: ['./upload-view.component.scss']
})
export class UploadViewComponent implements OnInit {

  constructor(private webSockeService: WebsocketService, public httpService: HttpService) { }

  public selectedProtocol: 'notSelected' | 'WebSocket' | 'HTTP' = 'notSelected';
  public fileReader: FileReader = new FileReader();
  public fileReadingIndex: number = 0;
  private fileSize: number = 0;
  public uploadStatus: string = 'notUploading';
  public uploadFilePieceNumber: number = 0;
  public selectedFileName: string = 'No file selected';
  private filePieceSize: number = 10485760;
  public httpSelectedDataUploadFormat: 'notSelected' | 'base64' | 'numberArray' | 'formData' = 'notSelected';
  public websocketSelectedDataUploadFormat: 'notSelected' | 'string' | 'binary' = 'notSelected';
  public httpBodySize: 'notSelected' | '5000' | '50000' | '500000' = 'notSelected';
  public uploadTime: number = 0;
  private uploadStartTime: Date  = new Date();
  private WebsocketInitprotocol: 'json' | 'binary' = 'json'
  private firstUpload  = true;
   
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  ngOnInit(): void {
    this.openWebSocketConnection();

    this.fileReader.onloadend  = async (e) => {
      console.log('filereader');
      if (this.fileReader.readyState !== FileReader.DONE ) {
        console.log('reader not ready');
        return;
      }

      const results =  new Uint8Array(this.fileReader.result as ArrayBuffer);    

      if (this.selectedProtocol === 'WebSocket') {
        const message: WebsocketMessage = {
          FileName: this.selectedFileName,
          MessageType: 'notSelected',
          PieceData: [],
          PieceNumber: 1
        }
        // if(!this.firstUpload) {
        //   this.webSockeService.close();
        //   await new Promise(r => setTimeout(r, 2000));
        //   this.webSockeService.connect();
        //   await new Promise(r => setTimeout(r, 2000));
        // }
        
        if (this.uploadStatus === 'notUploading') {
          this.firstUpload = false;
          message.MessageType = 'startUpload'
          this.uploadStatus = 'uploading'

          this.uploadStartTime = new Date();
          this.webSockeService.sendData(message);
        } 
        
        if (this.uploadStatus === 'uploading') {
         
          if(this.fileReadingIndex < this.fileSize + this.filePieceSize) {
            console.log('piece!!!!')

            message.MessageType = 'uploadPiece';
            message.PieceData = Array.from(results);
            message.PieceNumber = this.uploadFilePieceNumber;

          } else {
            message.MessageType = 'finishUpload';
            this.uploadStatus = 'notUploading'; 
          }

        }

        if(this.websocketSelectedDataUploadFormat === 'string' || message.MessageType === 'startUpload') {
          await this.webSockeService.sendData(message);
        } else if(this.websocketSelectedDataUploadFormat === 'binary') {
          if(this.WebsocketInitprotocol === 'json') {
            // this.webSockeService.reOpenConnection('binary');
            // this.WebsocketInitprotocol = 'binary';
            // await new Promise(r => setTimeout(r, 2000));

          }
          console.log('upload binary websocket')

          await this.webSockeService.sendDataBinary(this.fileReader.result as ArrayBuffer)
        }
        
        if( this.uploadStatus === 'notUploading' && this.fileReadingIndex > this.fileSize) {
          this.handleUploadEnding();
          // this.webSockeService.close();   
        } else {
          this.uploadRecursive();
          this.uploadFilePieceNumber++;
        }

      } else {
        if (this.uploadStatus === 'notUploading') {
          var postData = {
            fileName: this.selectedFileName
          }

          try {
            this.uploadStartTime = new Date();
            await this.httpService.startFileUpload(postData).toPromise();
            this.uploadStatus = 'uploading';
          } catch(ex) {
            console.log('failed to start upload')
          }
            
        } 

        if (this.uploadStatus === 'uploading') {
         
          console.log('uploadiing')

          if(this.fileReadingIndex < this.fileSize + this.filePieceSize) {
            if(this.httpSelectedDataUploadFormat === 'numberArray') {              
              const postBody = {
                fileName: this.selectedFileName,
                pieceData: Array.from(results),
                pieceNumber: this.uploadFilePieceNumber,
              };

              console.log(postBody)

              this.httpService.uploadFilePieceArray(postBody).subscribe(res => {
                console.log(res);
                this.uploadFilePieceNumber++;
                this.uploadRecursive();
              })

            } else if (this.httpSelectedDataUploadFormat === 'formData') {
              const formData = new FormData();
              formData.append('FileName', this.selectedFileName);
              formData.append('PieceData', new Blob([results]));
              formData.append('PieceNumber', this.uploadFilePieceNumber.toString());

              this.httpService.uploadFilePieceForm(formData).subscribe(res => {
                console.log(res);
                this.uploadFilePieceNumber++;
                this.uploadRecursive();
              });

            } else if(this.httpSelectedDataUploadFormat === 'base64') {
              console.log('base64')
              let base64Str = '';
              
              const resultLenght = results.length;
              for(var i = 0; i < resultLenght; i++) {
                base64Str += String.fromCharCode( results[ i ] );
              }
              const postBody = {
                fileName: this.selectedFileName,
                pieceData: btoa(base64Str),
                pieceNumber: this.uploadFilePieceNumber,
              };

              try {
                await  this.httpService.uploadFilePieceBase64(postBody).toPromise();
                this.uploadFilePieceNumber++;
                this.uploadRecursive();
              } catch (ex) {
                console.log('failed to send file piece')
              }

            } else {
              console.log('Upload format not selected');
            }

            

          } else {
            var postData = {
              fileName: this.selectedFileName
            }

            this.httpService.finnishUpload(postData).subscribe(res => {
              this.handleUploadEnding();
            });
            
          }
         
        }
      }
    }
   
  }

  handleUploadEnding = () => {
    const uploadEndTime = new Date();
    const uploadTimeMilliseconds = uploadEndTime.getTime() - this.uploadStartTime.getTime();
    this.uploadTime = uploadTimeMilliseconds / 1000;
    this.uploadStatus = 'notUploading';
    this.fileReadingIndex = 0;
    this.uploadFilePieceNumber = 0;

    // this.webSockeService.close();
  }

  uploadRecursive() {
    console.log('upload recursive')
    const fileList = this.fileInput.nativeElement.files;

    if (fileList) {
      if(this.httpBodySize != 'notSelected') {
        this.filePieceSize = parseInt(this.httpBodySize);
      }
      const nextFilePiece = fileList[0].slice(this.fileReadingIndex, this.fileReadingIndex + this.filePieceSize)
      this.fileReadingIndex += this.filePieceSize;
      this.fileReader.readAsArrayBuffer(nextFilePiece);
    }

  }


  handleFileSelectClick() {
    this.fileInput.nativeElement.click();
  }

  handleFileSelectChange($event: any) {
    this.selectedFileName = $event.target.files[0].name;
    this.fileSize = $event.target.files[0].size;
    console.log(this.fileSize)
  }

  async handleFileUploadStart() {
    var apiUrl = this.httpService.apiUrl.split('/api')[0]
    // this.webSockeService.websocketHTTPUrl = apiUrl;
    if(this.WebsocketInitprotocol === 'json' && this.websocketSelectedDataUploadFormat === 'binary') {
      this.webSockeService.close();
      await new Promise(r => setTimeout(r, 2000));
      this.webSockeService.connect('binary');
      await new Promise(r => setTimeout(r, 2000));
      this.WebsocketInitprotocol = 'binary';
    }
    this.uploadRecursive();
  }


  openWebSocketConnection(protocol: 'json' | 'binary' = 'json') {
    this.webSockeService.connect(protocol);
  }

}
