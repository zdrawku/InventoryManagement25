import { Component, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { RegisterRequest, UserRole } from '../../models/user.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  username = signal('');
  email = signal('');
  password = signal('');
  confirmPassword = signal('');
  role = signal<UserRole>(UserRole.User);
  errorMessage = signal('');
  isLoading = signal(false);

  UserRole = UserRole;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit(): void {
    this.errorMessage.set('');

    if (this.password() !== this.confirmPassword()) {
      this.errorMessage.set('Passwords do not match');
      return;
    }

    this.isLoading.set(true);

    const request: RegisterRequest = {
      username: this.username(),
      email: this.email(),
      password: this.password(),
      role: this.role()
    };

    this.authService.register(request).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.error?.message || 'Registration failed. Please try again.');
      }
    });
  }
}
