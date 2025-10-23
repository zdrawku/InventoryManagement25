import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { EquipmentService } from '../../services/equipment.service';
import { Equipment } from '../../models/equipment.model';

@Component({
  selector: 'app-equipment-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './equipment-form.component.html',
  styleUrl: './equipment-form.component.css'
})
export class EquipmentFormComponent implements OnInit {
  equipment = signal<Equipment>({
    equipmentId: 0,
    name: '',
    type: '',
    serialNumber: '',
    condition: 'Good',
    status: 'Available',
    location: '',
    photoUrl: ''
  });
  
  isEditMode = signal(false);
  isLoading = signal(false);
  errorMessage = signal('');

  constructor(
    private equipmentService: EquipmentService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEditMode.set(true);
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

  onSubmit(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    if (this.isEditMode()) {
      this.equipmentService.update(this.equipment().equipmentId, this.equipment()).subscribe({
        next: () => {
          this.isLoading.set(false);
          this.router.navigate(['/equipment']);
        },
        error: (error: any) => {
          this.isLoading.set(false);
          this.errorMessage.set(error.error?.message || 'Failed to save equipment');
        }
      });
    } else {
      this.equipmentService.create(this.equipment()).subscribe({
        next: () => {
          this.isLoading.set(false);
          this.router.navigate(['/equipment']);
        },
        error: (error: any) => {
          this.isLoading.set(false);
          this.errorMessage.set(error.error?.message || 'Failed to save equipment');
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/equipment']);
  }

  updateField(field: keyof Equipment, value: any): void {
    this.equipment.update(eq => ({ ...eq, [field]: value }));
  }
}
