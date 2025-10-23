import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { EquipmentService } from '../../services/equipment.service';
import { AuthService } from '../../services/auth.service';
import { Equipment } from '../../models/equipment.model';

@Component({
  selector: 'app-equipment-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './equipment-detail.component.html',
  styleUrl: './equipment-detail.component.css'
})
export class EquipmentDetailComponent implements OnInit {
  equipment = signal<Equipment | null>(null);
  isLoading = signal(true);
  errorMessage = signal('');

  constructor(
    private equipmentService: EquipmentService,
    public authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadEquipment(parseInt(id));
    }
  }

  loadEquipment(id: number): void {
    this.isLoading.set(true);
    this.equipmentService.getById(id).subscribe({
      next: (data) => {
        this.equipment.set(data);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.errorMessage.set('Failed to load equipment');
        this.isLoading.set(false);
      }
    });
  }

  createRequest(): void {
    const eq = this.equipment();
    if (eq) {
      this.router.navigate(['/requests/new'], { queryParams: { equipmentId: eq.equipmentId } });
    }
  }

  editEquipment(): void {
    const eq = this.equipment();
    if (eq) {
      this.router.navigate(['/equipment', eq.equipmentId, 'edit']);
    }
  }

  deleteEquipment(): void {
    const eq = this.equipment();
    if (eq && confirm(`Are you sure you want to delete ${eq.name}?`)) {
      this.equipmentService.delete(eq.equipmentId).subscribe({
        next: () => {
          this.router.navigate(['/equipment']);
        },
        error: (error) => {
          this.errorMessage.set('Failed to delete equipment');
        }
      });
    }
  }
}
