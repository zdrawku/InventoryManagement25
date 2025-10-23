import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Equipment } from '../models/equipment.model';

@Injectable({
  providedIn: 'root'
})
export class EquipmentService {
  private apiUrl = `${environment.apiUrl}/equipment`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Equipment[]> {
    return this.http.get<Equipment[]>(this.apiUrl);
  }

  getById(id: number): Observable<Equipment> {
    return this.http.get<Equipment>(`${this.apiUrl}/${id}`);
  }

  create(equipment: Equipment): Observable<Equipment> {
    return this.http.post<Equipment>(this.apiUrl, equipment);
  }

  update(id: number, equipment: Equipment): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, equipment);
  }

  updateStatus(id: number, status: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/status`, { status });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  search(filters: {
    name?: string;
    type?: string;
    status?: string;
    condition?: string;
    location?: string;
  }): Observable<Equipment[]> {
    let params = new HttpParams();
    
    Object.entries(filters).forEach(([key, value]) => {
      if (value) {
        params = params.set(key, value);
      }
    });

    return this.http.get<Equipment[]>(this.apiUrl, { params });
  }
}
