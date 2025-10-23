import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { 
  EquipmentRequest, 
  CreateRequestDto, 
  ApproveRequestDto, 
  ReturnRequestDto 
} from '../models/request.model';

@Injectable({
  providedIn: 'root'
})
export class RequestService {
  private apiUrl = `${environment.apiUrl}/request`;
  private managerUrl = `${environment.apiUrl}/manager`;

  constructor(private http: HttpClient) {}

  create(request: CreateRequestDto): Observable<EquipmentRequest> {
    return this.http.post<EquipmentRequest>(this.apiUrl, request);
  }

  getMyRequests(): Observable<EquipmentRequest[]> {
    return this.http.get<EquipmentRequest[]>(`${this.apiUrl}s`);
  }

  getAllRequests(): Observable<EquipmentRequest[]> {
    return this.http.get<EquipmentRequest[]>(`${this.managerUrl}/requests`);
  }

  approve(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/approve`, {});
  }

  reject(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/reject`, {});
  }

  returnEquipment(id: number, data: ReturnRequestDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/return`, data);
  }
}
