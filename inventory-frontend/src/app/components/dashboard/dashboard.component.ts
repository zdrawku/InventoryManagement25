import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { EquipmentService } from '../../services/equipment.service';
import { RequestService } from '../../services/request.service';
import { Equipment } from '../../models/equipment.model';
import { EquipmentRequest } from '../../models/request.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  stats = signal({
    totalEquipment: 0,
    availableEquipment: 0,
    myRequests: 0,
    pendingRequests: 0
  });
  
  recentEquipment = signal<Equipment[]>([]);
  recentRequests = signal<EquipmentRequest[]>([]);
  isLoading = signal(true);

  constructor(
    public authService: AuthService,
    private equipmentService: EquipmentService,
    private requestService: RequestService
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading.set(true);

    this.equipmentService.getAll().subscribe({
      next: (equipment) => {
        this.stats.update(s => ({
          ...s,
          totalEquipment: equipment.length,
          availableEquipment: equipment.filter(e => e.status === 'Available').length
        }));
        this.recentEquipment.set(equipment.slice(0, 4));
      }
    });

    const requestService$ = this.authService.isAdmin()
      ? this.requestService.getAllRequests()
      : this.requestService.getMyRequests();

    requestService$.subscribe({
      next: (requests) => {
        this.stats.update(s => ({
          ...s,
          myRequests: requests.length,
          pendingRequests: requests.filter(r => r.status === 'Pending').length
        }));
        this.recentRequests.set(requests.slice(0, 5));
        this.isLoading.set(false);
      }
    });
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString();
  }
}
