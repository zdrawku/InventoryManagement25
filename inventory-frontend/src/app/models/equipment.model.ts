export interface Equipment {
  equipmentId: number;
  name: string;
  type: string;
  serialNumber: string;
  condition: string;
  status: string;
  location: string;
  photoUrl: string;
}

export enum EquipmentCondition {
  Excellent = 'Excellent',
  Good = 'Good',
  Fair = 'Fair',
  Damaged = 'Damaged'
}

export enum EquipmentStatus {
  Available = 'Available',
  CheckedOut = 'CheckedOut',
  UnderRepair = 'UnderRepair',
  Retired = 'Retired'
}
