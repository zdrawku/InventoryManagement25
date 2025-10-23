export interface EquipmentRequest {
  id: number;
  equipmentId: number;
  equipmentName?: string;
  userId: number;
  userName?: string;
  requestDate: Date;
  requestedUntil: Date;
  status: RequestStatus;
  notes?: string;
}

export enum RequestStatus {
  Pending = 'Pending',
  Approved = 'Approved',
  Rejected = 'Rejected',
  Returned = 'Returned'
}

export interface CreateRequestDto {
  equipmentId: number;
  requestDate: Date;
  requestedUntil: Date;
  notes?: string;
}

export interface ApproveRequestDto {
  approved: boolean;
  notes?: string;
}

export interface ReturnRequestDto {
  condition: string;
  notes?: string;
}
