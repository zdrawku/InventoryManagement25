import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { RequestService } from '../../services/request.service';
import { EquipmentService } from '../../services/equipment.service';
import { Equipment } from '../../models/equipment.model';
import { CreateRequestDto } from '../../models/request.model';

@Component({
  selector: 'app-request-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './request-form.component.html',
  styleUrl: './request-form.component.css'
})
export class RequestFormComponent implements OnInit {
  equipment = signal<Equipment[]>([]);
  selectedEquipmentId = signal<number | null>(null);
  requestDate = signal<string>(new Date().toISOString().split('T')[0]);
  requestedUntil = signal<string>('');
  notes = signal('');
  
  isLoading = signal(false);
  errorMessage = signal('');

  constructor(
    private requestService: RequestService,
    private equipmentService: EquipmentService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.loadEquipment();
    
    const equipmentId = this.route.snapshot.queryParamMap.get('equipmentId');
    if (equipmentId) {
      this.selectedEquipmentId.set(parseInt(equipmentId));
    }
  }

  loadEquipment(): void {
    this.equipmentService.getAll().subscribe({
      next: (data) => {
        this.equipment.set(data.filter(e => e.status === 'Available'));
      },
      error: (error) => {
        this.errorMessage.set('Failed to load equipment');
      }
    });
  }

  onSubmit(): void {
    if (!this.selectedEquipmentId() || !this.requestedUntil()) {
      this.errorMessage.set('Please fill in all required fields');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    const request: CreateRequestDto = {
      equipmentId: this.selectedEquipmentId()!,
      requestDate: new Date(this.requestDate()),
      requestedUntil: new Date(this.requestedUntil()),
      notes: this.notes() || undefined
    };

    this.requestService.create(request).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/requests']);
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.error?.message || 'Failed to create request');
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/requests']);
  }
}
