import { Component, signal } from '@angular/core';
import { LucideAngularModule, Upload, FileText, AlertTriangle, CheckCircle, Info, Download, Loader, ShieldCheck, CloudUpload } from 'lucide-angular';
import { DocumentService } from '../../services/document.service';
import { QuestionService } from '../../services/question.service';
import { ReportService } from '../../services/report.service';

type RiskLevel = 'high' | 'medium' | 'low';
type AnalysisPhase = 'idle' | 'uploading' | 'uploaded' | 'analyzing';

interface AnalysisIssue {
  clause: string;
  risk: RiskLevel;
  description: string;
  reference: string;
}

interface AnalysisResult {
  fileName: string;
  riskLevel: RiskLevel;
  summary: string;
  issues: AnalysisIssue[];
  recommendations: string[];
}

@Component({
  selector: 'app-contract-scanner',
  standalone: true,
  imports: [LucideAngularModule],
  templateUrl: './contract-scanner.component.html'
})
export class ContractScannerComponent {
  readonly icons = { Upload, FileText, AlertTriangle, CheckCircle, Info, Download, Loader, ShieldCheck, CloudUpload };

  selectedFile = signal<File | null>(null);
  phase = signal<AnalysisPhase>('idle');
  analysisResult = signal<AnalysisResult | null>(null);
  isDownloading = signal(false);
  downloadError = signal<string | null>(null);

  constructor(
    private docService: DocumentService,
    private questionService: QuestionService,
    private reportService: ReportService
  ) {}

  onFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.selectedFile.set(input.files[0]);
      this.analysisResult.set(null);
      this.phase.set('idle');
      this.downloadError.set(null);
    }
  }

  analyze(): void {
    const file = this.selectedFile();
    if (!file) return;

    this.phase.set('uploading');
    this.analysisResult.set(null);
    this.downloadError.set(null);

    const scannerPrompt = `Analyze this legal document thoroughly for compliance risks under Uzbekistan law. You MUST respond with valid JSON only (no markdown, no code fences). Use this exact structure:
{
  "riskLevel": "high" or "medium" or "low",
  "summary": "A detailed 2-4 sentence overview of the document and its overall compliance status",
  "issues": [
    {
      "clause": "The specific clause or article from the document (e.g. 'Article 5.2 - Overtime Compensation')",
      "risk": "high" or "medium" or "low",
      "description": "A detailed explanation of the compliance issue, why it's problematic, and what the practical legal consequences are",
      "reference": "The specific Uzbekistan law, code, and article number that applies (e.g. 'Labor Code of Uzbekistan, Article 169')"
    }
  ],
  "recommendations": [
    "Specific, actionable recommendation with reference to the applicable law"
  ]
}

Be thorough: identify ALL compliance issues, not just the obvious ones. For each issue, provide a detailed explanation of the legal risk and cite the specific applicable law. Include at least 3-5 detailed recommendations.`;

    this.docService.upload(file).subscribe({
      next: (doc) => {
        this.phase.set('uploaded');

        setTimeout(() => {
          this.phase.set('analyzing');
        }, 800);

        this.questionService.ask({
          documentId: doc.id,
          questionText: scannerPrompt
        }).subscribe({
          next: (res) => {
            this.phase.set('idle');
            this.parseAnalysisResponse(file.name, res.answer);
          },
          error: (err) => {
            this.phase.set('idle');
            if (err.status === 402) {
              this.setFallbackResult(file.name, 'You have used all your tokens for today. To get more tokens, please upgrade your plan or wait until tomorrow.');
            } else {
              this.setFallbackResult(file.name, 'Analysis failed. Please try again.');
            }
          }
        });
      },
      error: () => {
        this.phase.set('idle');
        this.setFallbackResult(file.name, 'Failed to upload document. Please try again.');
      }
    });
  }

  clear(): void {
    this.selectedFile.set(null);
    this.analysisResult.set(null);
    this.phase.set('idle');
    this.downloadError.set(null);
  }

  downloadReport(): void {
    const result = this.analysisResult();
    if (!result || this.isDownloading()) return;

    this.isDownloading.set(true);
    this.downloadError.set(null);

    const payload = {
      fileName: result.fileName,
      riskLevel: result.riskLevel,
      summary: result.summary,
      issues: result.issues.map((i) => ({
        clause: i.clause,
        risk: i.risk,
        description: i.description,
        reference: i.reference,
      })),
      recommendations: result.recommendations,
    };

    this.reportService.downloadContractScannerPdf(payload).subscribe({
      next: (blob) => {
        this.isDownloading.set(false);
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `LegalGuard_Report_${result.fileName.replace(/[^a-z0-9._-]/gi, '_')}.pdf`;
        a.style.display = 'none';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
      },
      error: () => {
        this.isDownloading.set(false);
        this.downloadError.set('Failed to generate the report. Please try again.');
      },
    });
  }

  getFileSizeKB(): string {
    const file = this.selectedFile();
    return file ? (file.size / 1024).toFixed(2) : '0';
  }

  getRiskColor(risk: RiskLevel): { bg: string; text: string; border: string } {
    switch (risk) {
      case 'high': return { bg: 'bg-red-50', text: 'text-red-700', border: 'border-red-200' };
      case 'medium': return { bg: 'bg-amber-50', text: 'text-amber-700', border: 'border-amber-200' };
      case 'low': return { bg: 'bg-green-50', text: 'text-green-700', border: 'border-green-200' };
    }
  }

  isProcessing(): boolean {
    return this.phase() !== 'idle';
  }

  private parseAnalysisResponse(fileName: string, answer: string): void {
    try {
      let jsonStr = answer.trim();
      const fenceStart = jsonStr.indexOf('```');
      if (fenceStart !== -1) {
        const afterFence = jsonStr.indexOf('\n', fenceStart);
        const fenceEnd = jsonStr.lastIndexOf('```');
        if (afterFence !== -1 && fenceEnd > afterFence) {
          jsonStr = jsonStr.substring(afterFence + 1, fenceEnd).trim();
        }
      }

      const parsed = JSON.parse(jsonStr);

      const validRisk = (r: string): RiskLevel =>
        ['high', 'medium', 'low'].includes(r) ? r as RiskLevel : 'medium';

      this.analysisResult.set({
        fileName,
        riskLevel: validRisk(parsed.riskLevel),
        summary: parsed.summary || 'Analysis complete.',
        issues: (parsed.issues || []).map((i: any) => ({
          clause: i.clause || 'Unknown clause',
          risk: validRisk(i.risk),
          description: i.description || '',
          reference: i.reference || ''
        })),
        recommendations: parsed.recommendations || []
      });
    } catch {
      this.setFallbackResult(fileName, answer);
    }
  }

  private setFallbackResult(fileName: string, rawAnswer: string): void {
    this.analysisResult.set({
      fileName,
      riskLevel: 'medium',
      summary: rawAnswer.length > 500 ? rawAnswer.substring(0, 500) + '...' : rawAnswer,
      issues: [],
      recommendations: ['The AI response could not be parsed into structured results. Review the summary above for details.']
    });
  }
}
