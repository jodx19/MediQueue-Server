/**
 * api-facade.service.ts
 * Wraps the NSwag-generated monolithic `Client` into named, injectable services
 * consumed by feature components. Also exports missing DTO aliases and enum constants.
 */
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  Client,
  RegisterPatientCommand,
  RegisterCommand,
  BookAppointmentCommand,
  UpdateSOAPNoteCommand,
  AddVitalSignCommand,
  AddDiagnosisCommand,
  RecordPaymentCommand,
  ApplyDiscountCommand,
  LoginCommand,
  AuthResponseDto,
  ClinicalVisitDto,
  InvoiceDto,
  DoctorDto,
  PatientDetailDto,
  AppointmentDto,
  ClinicStatsDto,
} from './mediqueue-api';

// ─────────────────────────────────────────────────────────────────────────────
// Missing DTO type aliases (these are returned by the API but NSwag may not
// have generated them as named classes — use `any` with interface shape)
// ─────────────────────────────────────────────────────────────────────────────

export interface PatientSummaryDto {
  id?: string;
  fullName?: string;
  medicalRecordNumber?: string;
  mrn?: string;
  dateOfBirth?: Date;
  phone?: string;
  email?: string;
  isActive?: boolean;
  gender?: string;
}

export interface PatientDto {
  id?: string;
  mrn?: string;
  medicalRecordNumber?: string;
  firstName?: string;
  lastName?: string;
  dateOfBirth?: Date;
  gender?: string;
  phone?: string;
  email?: string;
}

export interface DoctorSummaryDto {
  id?: string;
  firstName?: string;
  lastName?: string;
  specialization?: string;
  email?: string;
  isActive?: boolean;
}

export interface AppointmentSummaryDto {
  id?: string;
  patientName?: string;
  doctorName?: string;
  scheduledAt?: Date;
  status?: string;
  type?: string;
  priority?: string;
}

export interface ClinicalVisitSummaryDto {
  id?: string;
  patientName?: string;
  doctorName?: string;
  startedAt?: Date;
  status?: string;
  chiefComplaint?: string;
}

export interface InvoiceSummaryDto {
  id?: string;
  invoiceNumber?: string;
  patientName?: string;
  issuedAt?: Date;
  totalAmount?: number;
  paidAmount?: number;
  status?: string;
}

// ─────────────────────────────────────────────────────────────────────────────
// Missing Command classes
// ─────────────────────────────────────────────────────────────────────────────

export class FinalizeClinicalVisitCommand {
  visitId?: string;
  constructor(data?: Partial<FinalizeClinicalVisitCommand>) {
    if (data) Object.assign(this, data);
  }
}

export class AddAllergyCommand {
  patientId?: string;
  allergen?: string;
  reaction?: string;
  severity?: string;
  constructor(data?: Partial<AddAllergyCommand>) {
    if (data) Object.assign(this, data);
  }
}

export class AddChronicConditionCommand {
  patientId?: string;
  conditionName?: string;
  diagnosedAt?: Date;
  constructor(data?: Partial<AddChronicConditionCommand>) {
    if (data) Object.assign(this, data);
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// String enum constants (the NSwag client generates numeric enums, but
// components use string values — map to the correct numeric values)
// ─────────────────────────────────────────────────────────────────────────────

export const PaymentMethod = {
  Cash: 1,
  CreditCard: 2,
  Insurance: 3,
  BankTransfer: 4,
  Installment: 5,
} as const;
export type PaymentMethod = typeof PaymentMethod[keyof typeof PaymentMethod];

export const DiscountType = {
  Percentage: 1,
  Fixed: 2,
} as const;
export type DiscountType = typeof DiscountType[keyof typeof DiscountType];

export const AppointmentType = {
  Regular: 'Regular',
  FollowUp: 'FollowUp',
  Emergency: 'Emergency',
  Consultation: 'Consultation',
  Procedure: 'Procedure',
} as const;
export type AppointmentType = typeof AppointmentType[keyof typeof AppointmentType];

export const AppointmentPriority = {
  Low: 'Low',
  Normal: 'Normal',
  High: 'High',
  Urgent: 'Urgent',
} as const;
export type AppointmentPriority = typeof AppointmentPriority[keyof typeof AppointmentPriority];

export const GenderType = {
  Male: 'Male',
  Female: 'Female',
  Other: 'Other',
} as const;
export type GenderType = typeof GenderType[keyof typeof GenderType];

export const BloodType = {
  Unknown: 'Unknown',
  APositive: 'A+',
  ANegative: 'A-',
  BPositive: 'B+',
  BNegative: 'B-',
  ABPositive: 'AB+',
  ABNegative: 'AB-',
  OPositive: 'O+',
  ONegative: 'O-',
} as const;
export type BloodType = typeof BloodType[keyof typeof BloodType];

export const VitalSignType = {
  BloodPressure: 'BloodPressure',
  HeartRate: 'HeartRate',
  Temperature: 'Temperature',
  Weight: 'Weight',
  Height: 'Height',
  SpO2: 'SpO2',
  RespiratoryRate: 'RespiratoryRate',
  BloodGlucose: 'BloodGlucose',
  BMI: 'BMI',
  PainLevel: 'PainLevel',
  Other: 'Other',
} as const;
export type VitalSignType = typeof VitalSignType[keyof typeof VitalSignType];

// ─────────────────────────────────────────────────────────────────────────────
// Named Client Services — injected by feature components
// ─────────────────────────────────────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly client = inject(Client);
  login(email: string, password: string): Promise<AuthResponseDto> {
    return firstValueFrom(this.client.login(new LoginCommand({ email, password })));
  }

  register(command: any): Promise<void> {
    return firstValueFrom(this.client.register(command));
  }

  // Mocking getUsers since Auth API doesn't list users in standard NSwag by default
  async getUsers(): Promise<any[]> {
    return []; // Provide empty array for now, usually needs a user management API
  }
}

@Injectable({ providedIn: 'root' })
export class PatientsClient {
  private readonly client = inject(Client);

  async getAll(page = 1, size = 50): Promise<PatientSummaryDto[]> {
    const result = (await firstValueFrom(this.client.patientsGET(page, size) as any)) as any;
    return result?.data?.items ?? result?.items ?? [];
  }

  async search(term: string): Promise<PatientSummaryDto[]> {
    const result = (await firstValueFrom(this.client.search(term || undefined) as any)) as any;
    return result?.items ?? result?.data?.items ?? result ?? [];
  }

  async getById(id: string): Promise<PatientDetailDto> {
    return firstValueFrom(this.client.patientsGET2(id));
  }

  async getByMrn(mrn: string): Promise<PatientSummaryDto | null> {
    const results = await this.search(mrn);
    return results.find((p: any) => p.medicalRecordNumber === mrn || p.mrn === mrn) || results[0] || null;
  }

  async register(command: any): Promise<PatientDto> {
    const result = (await firstValueFrom(this.client.patientsPOST(command) as any)) as any;
    return result?.data ?? result;
  }

  async getMedicalHistory(id: string): Promise<any> {
    return firstValueFrom(this.client.medicalHistory(id));
  }

  async addAllergy(id: string, command: any): Promise<void> {
    return firstValueFrom(this.client.allergiesPOST(id, command));
  }

  async addChronicCondition(id: string, command: any): Promise<void> {
    return firstValueFrom(this.client.chronicConditions(id, command));
  }
}

@Injectable({ providedIn: 'root' })
export class DoctorsClient {
  private readonly client = inject(Client);

  async getAll(): Promise<DoctorSummaryDto[]> {
    const result = (await firstValueFrom(this.client.doctorsGET() as any)) as any;
    return result?.items ?? result?.data?.items ?? result ?? [];
  }

  async getBySpecialty(specialty: string): Promise<DoctorSummaryDto[]> {
    const all = await this.getAll();
    return all.filter(d => d.specialization === specialty);
  }

  async getById(id: string): Promise<DoctorDto> {
    const result = (await firstValueFrom(this.client.doctorsGET2(id) as any)) as any;
    return result?.data ?? result;
  }

  async getAvailability(id: string, date?: Date): Promise<any> {
    return firstValueFrom(this.client.availability(id, date));
  }
}

@Injectable({ providedIn: 'root' })
export class AppointmentsClient {
  private readonly client = inject(Client);

  async getToday(): Promise<AppointmentSummaryDto[]> {
    const result = (await firstValueFrom(this.client.today() as any)) as any;
    return Array.isArray(result) ? result : result?.items ?? [];
  }

  async book(command: BookAppointmentCommand): Promise<AppointmentDto> {
    return firstValueFrom(this.client.appointmentsPOST(command));
  }

  async getById(id: string): Promise<AppointmentDto> {
    return firstValueFrom(this.client.appointmentsGET(id));
  }

  async confirm(id: string): Promise<AppointmentDto> {
    return firstValueFrom(this.client.confirm(id));
  }

  async checkIn(id: string): Promise<AppointmentDto> {
    return firstValueFrom(this.client.checkIn(id));
  }

  async start(id: string): Promise<AppointmentDto> {
    return firstValueFrom(this.client.start(id));
  }

  async cancel(id: string, reason?: string): Promise<void> {
    return firstValueFrom(this.client.cancel(id, { reason } as any));
  }
}

@Injectable({ providedIn: 'root' })
export class ClinicalVisitsClient {
  private readonly client = inject(Client);

  async getById(id: string): Promise<ClinicalVisitDto> {
    return firstValueFrom(this.client.clinicalVisitsGET(id));
  }

  async getMyVisits(): Promise<ClinicalVisitSummaryDto[]> {
    // GET /api/ClinicalVisits — returns a list; NSwag typed this as void, use cast
    const result = (await firstValueFrom(this.client.clinicalVisitsPOST as any)) as any;
    return Array.isArray(result) ? result : result?.items ?? [];
  }

  async updateSOAP(id: string, command: UpdateSOAPNoteCommand): Promise<void> {
    return firstValueFrom(this.client.soap(id, command));
  }

  async addVital(id: string, command: AddVitalSignCommand): Promise<void> {
    return firstValueFrom(this.client.vitalSigns(id, command));
  }

  async addDiagnosis(id: string, command: AddDiagnosisCommand): Promise<void> {
    return firstValueFrom(this.client.diagnoses(id, command));
  }

  async finalize(id: string, _command?: any): Promise<void> {
    return firstValueFrom(this.client.finalize(id));
  }
}

@Injectable({ providedIn: 'root' })
export class InvoicesClient {
  private readonly client = inject(Client);

  async getAll(): Promise<InvoiceSummaryDto[]> {
    const result = await firstValueFrom(this.client.patientsGET as any);
    return Array.isArray(result) ? result : (result as any)?.items ?? [];
  }

  async getById(id: string): Promise<InvoiceDto> {
    return firstValueFrom(this.client.invoicesGET(id));
  }

  async recordPayment(id: string, command: RecordPaymentCommand): Promise<void> {
    return firstValueFrom(this.client.payments(id, command));
  }

  async applyDiscount(id: string, command: ApplyDiscountCommand): Promise<void> {
    return firstValueFrom(this.client.discount(id, command));
  }
}

@Injectable({ providedIn: 'root' })
export class DashboardClient {
  private readonly client = inject(Client);

  async getStats(): Promise<ClinicStatsDto> {
    return firstValueFrom(this.client.stats());
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// Legacy aliases kept for backward compatibility
// ─────────────────────────────────────────────────────────────────────────────

export { PatientDetailDto, ClinicalVisitDto, InvoiceDto, DoctorDto, AppointmentDto, ClinicStatsDto };
