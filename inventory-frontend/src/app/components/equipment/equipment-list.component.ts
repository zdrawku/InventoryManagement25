import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { EquipmentService } from '../../services/equipment.service';
import { AuthService } from '../../services/auth.service';
import { Equipment } from '../../models/equipment.model';

@Component({
  selector: 'app-equipment-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './equipment-list.component.html',
  styleUrl: './equipment-list.component.css'
})
export class EquipmentListComponent implements OnInit {
  equipment = signal<Equipment[]>([]);
  filteredEquipment = signal<Equipment[]>([]);
  isLoading = signal(true);
  errorMessage = signal('');

  // Filter fields
  searchName = signal('');
  searchType = signal('');
  searchStatus = signal('');
  searchCondition = signal('');
  searchLocation = signal('');

  constructor(
    private equipmentService: EquipmentService,
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadEquipment();
  }

  loadEquipment(): void {
    this.isLoading.set(true);
    this.equipmentService.getAll().subscribe({
      next: (data) => {
        this.equipment.set(data);
        this.applyFilters();
        this.isLoading.set(false);
      },
      error: (error) => {
        this.errorMessage.set('Failed to load equipment');
        this.isLoading.set(false);
      }
    });
  }

  applyFilters(): void {
    let filtered = this.equipment();

    if (this.searchName()) {
      filtered = filtered.filter(e => 
        e.name.toLowerCase().includes(this.searchName().toLowerCase())
      );
    }
    if (this.searchType()) {
      filtered = filtered.filter(e => 
        e.type.toLowerCase().includes(this.searchType().toLowerCase())
      );
    }
    if (this.searchStatus()) {
      filtered = filtered.filter(e => e.status === this.searchStatus());
    }
    if (this.searchCondition()) {
      filtered = filtered.filter(e => e.condition === this.searchCondition());
    }
    if (this.searchLocation()) {
      filtered = filtered.filter(e => 
        e.location.toLowerCase().includes(this.searchLocation().toLowerCase())
      );
    }

    this.filteredEquipment.set(filtered);
  }

  clearFilters(): void {
    this.searchName.set('');
    this.searchType.set('');
    this.searchStatus.set('');
    this.searchCondition.set('');
    this.searchLocation.set('');
    this.applyFilters();
  }

  viewDetails(id: number): void {
    this.router.navigate(['/equipment', id]);
  }

  editEquipment(id: number): void {
    this.router.navigate(['/equipment', id, 'edit']);
  }

  deleteEquipment(id: number, name: string): void {
    if (confirm(`Are you sure you want to delete ${name}?`)) {
      this.equipmentService.delete(id).subscribe({
        next: () => {
          this.loadEquipment();
        },
        error: (error) => {
          this.errorMessage.set('Failed to delete equipment');
        }
      });
    }
  }

  createRequest(id: number): void {
    this.router.navigate(['/requests/new'], { queryParams: { equipmentId: id } });
  }
}
