export interface UsageReport {
  equipmentId: number;
  equipmentName: string;
  totalRequests: number;
  totalDays: number;
  currentStatus: string;
}

export interface HistoryLog {
  id: number;
  equipmentId: number;
  equipmentName: string;
  userId: number;
  userName: string;
  action: string;
  timestamp: Date;
  notes?: string;
}

export interface ExportOptions {
  format: 'csv' | 'pdf';
  reportType: 'usage' | 'history';
  startDate?: Date;
  endDate?: Date;
}
