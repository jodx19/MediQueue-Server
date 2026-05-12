import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthApiService } from '../../core/api/api-facade.service';
import { RegisterCommand } from '../../core/api/mediqueue-api';
import { pageEnter, fadeSlideIn } from '../../shared/animations/page-animations';

@Component({
  selector: 'app-super-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './super-admin.component.html',
  styleUrl: './super-admin.component.scss',
  animations: [pageEnter, fadeSlideIn]
})
export class SuperAdminComponent implements OnInit {
  private authApi = inject(AuthApiService);

  staffList = signal<any[]>([]);
  isLoading = signal(false);
  isModalOpen = signal(false);
  
  // New Staff Form
  newStaff = {
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    role: 'Admin'
  };

  roles = ['Admin', 'Doctor', 'Receptionist'];

  ngOnInit() {
    this.loadStaff();
  }

  async loadStaff() {
    this.isLoading.set(true);
    try {
      // Note: We'll use a mocked list or a specific endpoint if available
      // For now, let's assume we fetch from a service or display a placeholder
      this.staffList.set([
        { firstName: 'Super', lastName: 'Admin', email: 'superadmin@mediqueue.com', role: 'Admin', status: 'Active' },
      ]);
    } finally {
      this.isLoading.set(false);
    }
  }

  openModal() {
    this.newStaff = { firstName: '', lastName: '', email: '', password: 'TempPassword123!', role: 'Admin' };
    this.isModalOpen.set(true);
  }

  closeModal() {
    this.isModalOpen.set(false);
  }

  async createAccount() {
    this.isLoading.set(true);
    try {
      const command = new RegisterCommand({
        email: this.newStaff.email,
        password: this.newStaff.password,
        // NSwag generated RegisterCommand usually matches this structure
        // If the backend expects more fields (FirstName/LastName), 
        // ensure the API facade or mediqueue-api.ts supports it.
      });
      
      await this.authApi.register(command);
      
      // Add to local list for UI feedback
      this.staffList.update(list => [...list, { 
        firstName: this.newStaff.firstName, 
        lastName: this.newStaff.lastName, 
        email: this.newStaff.email, 
        role: this.newStaff.role, 
        status: 'Active' 
      }]);
      
      this.closeModal();
    } catch (err: any) {
      alert(err?.error?.detail || 'Failed to create staff account');
    } finally {
      this.isLoading.set(false);
    }
  }

  deactivateStaff(email: string) {
    if (confirm(`Are you sure you want to deactivate ${email}?`)) {
      this.staffList.update(list => list.map(s => s.email === email ? { ...s, status: 'Inactive' } : s));
    }
  }
}
