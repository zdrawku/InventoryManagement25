import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportService } from '../../services/report.service';
import { UsageReport, HistoryLog } from '../../models/report.model';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.css'
})
export class ReportsComponent implements OnInit {
  activeTab = signal<'usage' | 'history'>('usage');
  usageReports = signal<UsageReport[]>([]);
  historyLogs = signal<HistoryLog[]>([]);
  isLoading = signal(false);
  errorMessage = signal('');

  constructor(private reportService: ReportService) {}

  ngOnInit(): void {
    this.loadUsageReport();
  }

  setActiveTab(tab: 'usage' | 'history'): void {
    this.activeTab.set(tab);
    if (tab === 'usage') {
      this.loadUsageReport();
    } else {
      this.loadHistory();
    }
  }

  loadUsageReport(): void {
    this.isLoading.set(true);
    this.reportService.getUsageReport().subscribe({
      next: (data) => {
        this.usageReports.set(data);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.errorMessage.set('Failed to load usage report');
        this.isLoading.set(false);
      }
    });
  }

  loadHistory(): void {
    this.isLoading.set(true);
    this.reportService.getHistory().subscribe({
      next: (data) => {
        this.historyLogs.set(data);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.errorMessage.set('Failed to load history');
        this.isLoading.set(false);
      }
    });
  }

  exportReport(format: 'csv' | 'pdf'): void {
    this.reportService.export({
      format,
      reportType: this.activeTab()
    }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${this.activeTab()}-report.${format}`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        this.errorMessage.set('Failed to export report');
      }
    });
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleString();
  }
}
