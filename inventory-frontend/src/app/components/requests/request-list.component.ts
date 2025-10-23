import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { RequestService } from '../../services/request.service';
import { AuthService } from '../../services/auth.service';
import { EquipmentRequest } from '../../models/request.model';

@Component({
  selector: 'app-request-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './request-list.component.html',
  styleUrl: './request-list.component.css'
})
export class RequestListComponent implements OnInit {
  requests = signal<EquipmentRequest[]>([]);
  isLoading = signal(true);
  errorMessage = signal('');
  successMessage = signal('');

  constructor(
    private requestService: RequestService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    this.isLoading.set(true);
    const service$ = this.authService.isAdmin()
      ? this.requestService.getAllRequests()
      : this.requestService.getMyRequests();

    service$.subscribe({
      next: (data) => {
        this.requests.set(data);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.errorMessage.set('Failed to load requests');
        this.isLoading.set(false);
      }
    });
  }

  approveRequest(id: number): void {
    this.requestService.approve(id).subscribe({
      next: () => {
        this.successMessage.set('Request approved successfully');
        this.loadRequests();
        setTimeout(() => this.successMessage.set(''), 3000);
      },
      error: (error) => {
        this.errorMessage.set('Failed to approve request');
        setTimeout(() => this.errorMessage.set(''), 3000);
      }
    });
  }

  rejectRequest(id: number): void {
    if (confirm('Are you sure you want to reject this request?')) {
      this.requestService.reject(id).subscribe({
        next: () => {
          this.successMessage.set('Request rejected');
          this.loadRequests();
          setTimeout(() => this.successMessage.set(''), 3000);
        },
        error: (error) => {
          this.errorMessage.set('Failed to reject request');
          setTimeout(() => this.errorMessage.set(''), 3000);
        }
      });
    }
  }

  returnEquipment(id: number): void {
    const condition = prompt('Enter equipment condition (Excellent, Good, Fair, Damaged):');
    if (condition) {
      this.requestService.returnEquipment(id, { condition, notes: '' }).subscribe({
        next: () => {
          this.successMessage.set('Equipment returned successfully');
          this.loadRequests();
          setTimeout(() => this.successMessage.set(''), 3000);
        },
        error: (error) => {
          this.errorMessage.set('Failed to return equipment');
          setTimeout(() => this.errorMessage.set(''), 3000);
        }
      });
    }
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString();
  }
}
