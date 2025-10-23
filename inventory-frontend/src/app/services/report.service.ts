import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { UsageReport, HistoryLog, ExportOptions } from '../models/report.model';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private apiUrl = `${environment.apiUrl}/reports`;

  constructor(private http: HttpClient) {}

  getUsageReport(): Observable<UsageReport[]> {
    return this.http.get<UsageReport[]>(`${this.apiUrl}/usage`);
  }

  getHistory(equipmentId?: number, userId?: number): Observable<HistoryLog[]> {
    let params = new HttpParams();
    if (equipmentId) {
      params = params.set('equipmentId', equipmentId.toString());
    }
    if (userId) {
      params = params.set('userId', userId.toString());
    }
    return this.http.get<HistoryLog[]>(`${this.apiUrl}/history`, { params });
  }

  export(options: ExportOptions): Observable<Blob> {
    let params = new HttpParams()
      .set('format', options.format)
      .set('reportType', options.reportType);
    
    if (options.startDate) {
      params = params.set('startDate', options.startDate.toISOString());
    }
    if (options.endDate) {
      params = params.set('endDate', options.endDate.toISOString());
    }

    return this.http.get(`${this.apiUrl}/export`, { 
      params, 
      responseType: 'blob' 
    });
  }
}
