import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  UpdateSOAPNoteCommand,
  AddVitalSignCommand, AddDiagnosisCommand,
} from '../../../core/api/mediqueue-api';
import {
  ClinicalVisitsClient, FinalizeClinicalVisitCommand, VitalSignType,
} from '../../../core/api/api-facade.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LoadingSkeletonComponent } from '../../../shared/components/loading-skeleton/loading-skeleton.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { tabFade, pageEnter } from '../../../shared/animations/page-animations';

type Tab = 'soap' | 'vitals' | 'diagnoses' | 'prescriptions';

@Component({
  selector: 'app-visit-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeaderComponent, LoadingSkeletonComponent, BadgeComponent],
  animations: [tabFade, pageEnter],
  template: `
    <app-page-header
      [title]="visit() ? 'Visit — ' + (visit().patientName ?? '') : 'Clinical Visit'"
      [subtitle]="visit() ? ('Dr. ' + visit().doctorName) : ''"
    >
      <button class="btn-secondary" (click)="router.navigate(['/clinical-visits'])">← Back</button>
      @if (visit()?.status !== 'Finalized') {
        <button class="btn-danger" id="btn-finalize" (click)="finalizeVisit()">Finalize Visit</button>
      }
    </app-page-header>

    @if (isLoading()) {
      <app-loading-skeleton [count]="3" />
    } @else {
      <!-- Tabs -->
      <div class="tabs" @pageEnter>
        @for (tab of tabs; track tab.key) {
          <button class="tab" [class.tab--active]="activeTab() === tab.key"
            (click)="activeTab.set(tab.key)">
            {{ tab.label }}
          </button>
        }
      </div>

      <!-- SOAP Tab -->
      @if (activeTab() === 'soap') {
        <div class="tab-panel" @tabFade>
          <div class="soap-grid">
            @for (field of soapFields; track field.key) {
              <div class="field-group">
                <label class="field-label">{{ field.label }}</label>
                <textarea class="field-input field-textarea"
                  [(ngModel)]="soap[field.key]" [name]="field.key"
                  [placeholder]="field.placeholder" rows="4"></textarea>
              </div>
            }
          </div>
          <div class="tab-actions">
            <button class="btn-primary" [disabled]="isSaving()" (click)="saveSOAP()">
              @if (isSaving()) { <span class="spinner"></span> Saving… } @else { Save SOAP Note }
            </button>
          </div>
        </div>
      }

      <!-- Vitals Tab -->
      @if (activeTab() === 'vitals') {
        <div class="tab-panel" @tabFade>
          <div class="vitals-list">
            @for (v of visit()?.vitalSigns ?? []; track v.id) {
              <div class="vital-row">
                <span class="vital-type">{{ v.type }}</span>
                <span class="vital-value">{{ v.value }} <em>{{ v.unit }}</em></span>
                <span class="vital-time">{{ v.recordedAt | date:'shortTime' }}</span>
              </div>
            }
          </div>
          <div class="add-form">
            <h4>Add Vital Sign</h4>
            <div class="form-row">
              <select class="field-input" [(ngModel)]="vital.type" name="vtype">
                @for (vt of vitalTypes; track vt) { <option [value]="vt">{{ vt }}</option> }
              </select>
              <input class="field-input" type="text" [(ngModel)]="vital.value" placeholder="Value" />
              <input class="field-input" type="text" [(ngModel)]="vital.unit" placeholder="Unit (e.g. mmHg)" />
              <button class="btn-primary" (click)="addVital()">Add</button>
            </div>
          </div>
        </div>
      }

      <!-- Diagnoses Tab -->
      @if (activeTab() === 'diagnoses') {
        <div class="tab-panel" @tabFade>
          <div class="diagnoses-list">
            @for (d of visit()?.diagnoses ?? []; track d.id) {
              <div class="diagnosis-row">
                <span class="icd-code">{{ d.icdCode }}</span>
                <span class="diagnosis-desc">{{ d.description }}</span>
              </div>
            }
          </div>
          <div class="add-form">
            <h4>Add Diagnosis</h4>
            <div class="form-row">
              <input class="field-input" type="text" [(ngModel)]="diagnosis.icdCode" placeholder="ICD-10 Code (e.g. J06.9)" />
              <input class="field-input" type="text" [(ngModel)]="diagnosis.description" placeholder="Description" style="flex:2" />
              <button class="btn-primary" (click)="addDiagnosis()">Add</button>
            </div>
          </div>
        </div>
      }

      <!-- Prescriptions Tab -->
      @if (activeTab() === 'prescriptions') {
        <div class="tab-panel" @tabFade>
          <div class="prescriptions-list">
            @for (rx of visit()?.prescriptions ?? []; track rx.id) {
              <div class="rx-row">
                <span class="rx-drug">{{ rx.medicationName }}</span>
                <span class="rx-dosage">{{ rx.dosage }} — {{ rx.frequency }}</span>
                <span class="rx-duration">{{ rx.duration }}</span>
              </div>
            }
          </div>
          @if ((visit()?.prescriptions ?? []).length === 0) {
            <p class="empty-note">No prescriptions added yet.</p>
          }
        </div>
      }
    }
  `,
  styles: [`
    .tabs { display: flex; gap: 2px; background: var(--color-surface-2); padding: 4px; border-radius: var(--radius-lg); margin-bottom: var(--space-5); }
    .tab {
      flex: 1; padding: var(--space-2) var(--space-4); border: none; background: transparent;
      border-radius: var(--radius-md); font-size: var(--text-sm); font-weight: 500;
      cursor: pointer; color: var(--color-text-secondary); font-family: var(--font-family);
      transition: all var(--duration-fast);
    }
    .tab--active { background: var(--color-surface); color: var(--color-text-primary); font-weight: 600; box-shadow: var(--shadow-sm); }
    .tab-panel { background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-lg); padding: var(--space-6); }
    .soap-grid { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-4); }
    .field-group { display: flex; flex-direction: column; }
    .field-label { font-size: var(--text-sm); font-weight: 600; color: var(--color-text-primary); margin-bottom: var(--space-2); }
    .field-input {
      padding: var(--space-3) var(--space-4); border: 1px solid var(--color-border-strong);
      border-radius: var(--radius-md); font-size: var(--text-sm); font-family: var(--font-family);
      color: var(--color-text-primary); background: var(--color-surface); outline: none;
    }
    .field-input:focus { border-color: var(--color-accent); }
    .field-textarea { resize: vertical; }
    .tab-actions { display: flex; justify-content: flex-end; padding-top: var(--space-4); border-top: 1px solid var(--color-border); margin-top: var(--space-4); }
    .btn-primary {
      display: inline-flex; align-items: center; gap: var(--space-2);
      background: var(--color-accent); color: white; border: none;
      border-radius: var(--radius-md); padding: var(--space-3) var(--space-5);
      font-size: var(--text-sm); font-weight: 600; cursor: pointer; font-family: var(--font-family);
    }
    .btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-secondary {
      background: var(--color-surface-2); color: var(--color-text-primary);
      border: 1px solid var(--color-border); border-radius: var(--radius-md);
      padding: var(--space-2) var(--space-4); font-size: var(--text-sm);
      cursor: pointer; font-family: var(--font-family);
    }
    .btn-danger {
      background: var(--color-danger); color: white; border: none;
      border-radius: var(--radius-md); padding: var(--space-2) var(--space-5);
      font-size: var(--text-sm); font-weight: 600; cursor: pointer; font-family: var(--font-family);
    }
    .spinner { width: 14px; height: 14px; border: 2px solid rgba(255,255,255,0.3); border-top-color: white; border-radius: 50%; animation: spin 0.7s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
    .add-form { margin-top: var(--space-5); padding-top: var(--space-4); border-top: 1px solid var(--color-border); }
    .add-form h4 { font-size: var(--text-sm); font-weight: 600; margin-bottom: var(--space-3); }
    .form-row { display: flex; gap: var(--space-3); align-items: center; }
    .vital-row, .diagnosis-row, .rx-row {
      display: flex; gap: var(--space-4); padding: var(--space-3) 0;
      border-bottom: 1px solid var(--color-border); align-items: center;
    }
    .vital-type { font-weight: 600; font-size: var(--text-sm); min-width: 140px; }
    .vital-value { font-size: var(--text-base); color: var(--color-text-primary); }
    .vital-value em { color: var(--color-text-tertiary); font-style: normal; font-size: var(--text-xs); }
    .vital-time { margin-left: auto; font-size: var(--text-xs); color: var(--color-text-tertiary); }
    .icd-code { font-family: var(--font-mono); font-size: var(--text-xs); background: var(--color-accent-light); color: var(--color-accent); padding: 2px 8px; border-radius: var(--radius-sm); }
    .diagnosis-desc { font-size: var(--text-sm); color: var(--color-text-primary); }
    .rx-drug { font-weight: 600; font-size: var(--text-sm); }
    .rx-dosage { font-size: var(--text-sm); color: var(--color-text-secondary); }
    .rx-duration { font-size: var(--text-xs); color: var(--color-text-tertiary); margin-left: auto; }
    .empty-note { color: var(--color-text-tertiary); font-size: var(--text-sm); text-align: center; padding: var(--space-8) 0; }
  `],
})
export class VisitDetailComponent implements OnInit {
  private readonly visitsClient = inject(ClinicalVisitsClient);
  private readonly notifications = inject(NotificationService);
  private readonly route = inject(ActivatedRoute);
  readonly router = inject(Router);

  visitId = '';
  visit = signal<any>(null);
  activeTab = signal<Tab>('soap');
  isLoading = signal(true);
  isSaving = signal(false);

  tabs = [
    { key: 'soap' as Tab, label: 'SOAP Note' },
    { key: 'vitals' as Tab, label: 'Vital Signs' },
    { key: 'diagnoses' as Tab, label: 'Diagnoses' },
    { key: 'prescriptions' as Tab, label: 'Prescriptions' },
  ];

  soapFields = [
    { key: 'subjective' as const, label: 'S — Subjective', placeholder: "Patient's complaints and symptoms…" },
    { key: 'objective' as const, label: 'O — Objective', placeholder: 'Clinical observations, exam findings…' },
    { key: 'assessment' as const, label: 'A — Assessment', placeholder: 'Clinical impression, differential…' },
    { key: 'plan' as const, label: 'P — Plan', placeholder: 'Treatment plan, follow-up, referrals…' },
  ];

  soap = { subjective: '', objective: '', assessment: '', plan: '' };
  vital = { type: VitalSignType.BloodPressure as VitalSignType, value: '', unit: '' };
  diagnosis = { icdCode: '', description: '', codeSystem: 'ICD-10' };
  vitalTypes = Object.values(VitalSignType);

  async ngOnInit() {
    this.visitId = this.route.snapshot.paramMap.get('id')!;
    await this.loadVisit();
  }

  private async loadVisit() {
    try {
      const result = await this.visitsClient.getById(this.visitId);
      this.visit.set(result);
      if ((result as any).soapNote) Object.assign(this.soap, (result as any).soapNote);
    } finally {
      this.isLoading.set(false);
    }
  }

  async saveSOAP() {
    this.isSaving.set(true);
    try {
      const command = new UpdateSOAPNoteCommand({ 
        visitId: this.visitId, 
        subjectiveNote: this.soap.subjective,
        objectiveNote: this.soap.objective,
        assessmentNote: this.soap.assessment,
        planNote: this.soap.plan,
      });
      await this.visitsClient.updateSOAP(this.visitId, command);
      this.notifications.success('SOAP note saved.');
    } catch (err: any) {
      this.notifications.error(err?.error?.detail ?? 'Failed to save SOAP note.');
    } finally {
      this.isSaving.set(false);
    }
  }

  async addVital() {
    try {
      const command = new AddVitalSignCommand({
        visitId: this.visitId, vitalSignType: this.vital.type as any,
        value: parseFloat(this.vital.value) || 0, unit: this.vital.unit,
      });
      await this.visitsClient.addVital(this.visitId, command);
      this.notifications.success('Vital sign recorded.');
      this.vital = { type: VitalSignType.BloodPressure, value: '', unit: '' };
      await this.loadVisit();
    } catch (err: any) {
      this.notifications.error(err?.error?.detail ?? 'Failed to record vital.');
    }
  }

  async addDiagnosis() {
    try {
      const command = new AddDiagnosisCommand({ visitId: this.visitId, ...this.diagnosis });
      await this.visitsClient.addDiagnosis(this.visitId, command);
      this.notifications.success('Diagnosis added.');
      this.diagnosis = { icdCode: '', description: '', codeSystem: 'ICD-10' };
      await this.loadVisit();
    } catch (err: any) {
      this.notifications.error(err?.error?.detail ?? 'Failed to add diagnosis.');
    }
  }

  async finalizeVisit() {
    if (!confirm('Finalize this visit? This will trigger invoice creation.')) return;
    try {
      const command = new FinalizeClinicalVisitCommand({ visitId: this.visitId });
      await this.visitsClient.finalize(this.visitId, command);
      this.notifications.success('Visit finalized. Invoice created automatically.');
      await this.loadVisit();
    } catch (err: any) {
      this.notifications.error(err?.error?.detail ?? 'Failed to finalize visit.');
    }
  }
}
