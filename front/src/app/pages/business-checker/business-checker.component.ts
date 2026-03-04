import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, CheckCircle, AlertTriangle, XCircle, Info, Sparkles } from 'lucide-angular';

type ComplianceStatus = 'compliant' | 'needs-review' | 'non-compliant';

interface CheckDetail {
  category: string;
  status: ComplianceStatus;
  description: string;
  requirements: string[];
  references: string;
}

interface CheckResult {
  status: ComplianceStatus;
  title: string;
  summary: string;
  details: CheckDetail[];
  nextSteps: string[];
}

@Component({
  selector: 'app-business-checker',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  templateUrl: './business-checker.component.html'
})
export class BusinessCheckerComponent {
  readonly icons = { CheckCircle, AlertTriangle, XCircle, Info, Sparkles };

  formData = {
    businessType: '',
    initiative: '',
    industry: '',
    employees: ''
  };

  isChecking = signal(false);
  result = signal<CheckResult | null>(null);

  onSubmit(): void {
    this.isChecking.set(true);

    setTimeout(() => {
      this.result.set({
        status: 'needs-review',
        title: 'Compliance Assessment Complete',
        summary: 'Your business initiative is generally compliant with Uzbekistan law, but requires additional licenses and registrations before launch.',
        details: [
          {
            category: 'Business Registration',
            status: 'compliant',
            description: 'Your chosen business structure is appropriate for the selected industry.',
            requirements: [
              'Register as the chosen entity type',
              'Minimum authorized capital requirements',
              'Electronic registration through Single Window system',
            ],
            references: 'Law on Limited Liability Companies (No. 223-I)',
          },
          {
            category: 'Industry Licensing',
            status: 'needs-review',
            description: 'Your industry may require specific registrations and compliance with regulations.',
            requirements: [
              'Obtain required industry-specific licenses',
              'Register on relevant government platforms',
              'Implement data protection measures per Personal Data Law',
              'Display company registration details publicly',
            ],
            references: 'Presidential Decree PD-5611',
          },
          {
            category: 'Tax Compliance',
            status: 'compliant',
            description: 'Your business qualifies for simplified taxation if revenue stays under threshold.',
            requirements: [
              'Register with tax authorities (automatic with business registration)',
              'Choose tax regime: Simplified (4%) or General (15% + VAT)',
              'Install fiscal cash register for payments',
              'Submit quarterly tax declarations',
            ],
            references: 'Tax Code Chapter 14, Articles 346-352',
          },
          {
            category: 'Payment Processing',
            status: 'needs-review',
            description: 'Payment processing requires partnership with licensed payment operators.',
            requirements: [
              'Contract with licensed payment gateway (Payme, Click, Uzum)',
              'Implement PCI DSS compliance for card processing',
              'Enable UzCard and Humo card acceptance',
              'Set up merchant account with local bank',
            ],
            references: 'Central Bank Regulation No. 2845',
          },
          {
            category: 'Employment & Labor',
            status: 'compliant',
            description: 'Standard employment regulations apply for the specified number of employees.',
            requirements: [
              'Register employment contracts with tax authority',
              'Pay social tax (12% of wages)',
              'Ensure contracts comply with Labor Code',
              'Provide mandatory insurance coverage',
            ],
            references: 'Labor Code of Uzbekistan, Articles 77-90',
          },
        ],
        nextSteps: [
          'Complete business registration through the Single Window portal',
          'Register on required national platforms',
          'Obtain electronic signature certificate from authorized provider',
          'Set up contracts with licensed payment processors',
          'Implement personal data protection measures and privacy policy',
          'Install certified fiscal cash register',
          'Consult with tax advisor to choose optimal tax regime',
        ],
      });
      this.isChecking.set(false);
    }, 3000);
  }

  getStatusIcon(status: ComplianceStatus): typeof CheckCircle {
    switch (status) {
      case 'compliant': return CheckCircle;
      case 'needs-review': return AlertTriangle;
      case 'non-compliant': return XCircle;
    }
  }

  getStatusColor(status: ComplianceStatus): string {
    switch (status) {
      case 'compliant': return 'text-green-600';
      case 'needs-review': return 'text-amber-600';
      case 'non-compliant': return 'text-red-600';
    }
  }

  getStatusBg(status: ComplianceStatus): string {
    switch (status) {
      case 'compliant': return 'bg-green-50 border-green-200 text-green-700';
      case 'needs-review': return 'bg-amber-50 border-amber-200 text-amber-700';
      case 'non-compliant': return 'bg-red-50 border-red-200 text-red-700';
    }
  }

  getStatusLabel(status: ComplianceStatus): string {
    return status.replace('-', ' ');
  }
}
