import { Injectable } from '@angular/core'
import { HttpClient } from '@angular/common/http';;

@Injectable({
  providedIn: 'root'
})
export class HttpService {
  public apiUrl: string;

  constructor(private http: HttpClient) {
    const azureUrl = 'http://20.55.51.145:5000/api/HTTP/';
    const localUrl = 'http://localhost:50765/api/HTTP/'
    this.apiUrl = azureUrl;
  }

  startFileUpload(data: FileUploadAjax) {
    return this.http.post(this.apiUrl + 'StartUpload', data);
  }

  uploadFilePieceArray(pieceData: PieceDataArrayAjax) {
    return this.http.post(this.apiUrl + 'UploadFilePieceArray', pieceData);
  }

  uploadFilePieceForm(formData: FormData) {
    return this.http.post(this.apiUrl + 'UploadFilePieceForm', formData)
  }

  uploadFilePieceBase64(pieceData: PieceDataBase64Ajax) {
    return this.http.post(this.apiUrl + 'UploadFilePieceBase64', pieceData);
  }

  finnishUpload(data: FileUploadAjax) {
    return this.http.post(this.apiUrl + 'FinishUpload', data);
  }


}

interface PieceDataArrayAjax {
  fileName: string;
  pieceData: number[];
  pieceNumber: number;
}

interface PieceDataBase64Ajax {
  fileName: string;
  pieceData: string;
  pieceNumber: number;
}

interface FileUploadAjax {
  fileName: string;
}
