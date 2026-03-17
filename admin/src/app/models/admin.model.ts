export interface AdminStats {
  totalUsers: number;
  activeUsers: number;
  totalDocuments: number;
  totalQuestions: number;
  totalAlerts: number;
  unreadAlerts: number;
  totalPayments: number;
  totalRevenue: number;
  planBreakdown: Record<string, number>;
  regulatoryUpdatesTotal: number;
  regulatoryUpdatesPending: number;
}

export interface AdminUser {
  id: number;
  email: string;
  fullName: string | null;
  isActive: boolean;
  emailConfirmed: boolean;
  plan: string;
  tokensRemaining: number;
  createdAt: string;
  lastLoginAt: string | null;
  documentsCount: number;
  questionsCount: number;
  roles: string[];
}

export interface AdminUserDetail extends AdminUser {
  updatedAt: string | null;
  lastTokenResetAt: string | null;
  documents: AdminDocument[];
  payments: AdminPayment[];
}

export interface AdminDocument {
  id: string;
  userId: number | null;
  userEmail: string | null;
  fileName: string;
  contentType: string;
  sizeInBytes: number;
  uploadedAt: string;
  questionsCount: number;
  legalReferencesCount: number;
}

export interface AdminRegulatoryUpdate {
  id: string;
  title: string;
  description: string | null;
  lawIdentifier: string;
  sourceUrl: string | null;
  effectiveDate: string | null;
  publishedAt: string | null;
  createdAt: string;
  isProcessed: boolean;
  alertsCount: number;
}

export interface CreateRegulatoryUpdate {
  title: string;
  description?: string;
  lawIdentifier: string;
  sourceUrl?: string;
  effectiveDate?: string;
  publishedAt?: string;
}

export interface ProcessResult {
  updateId: string;
  alertsCreated: number;
  message: string;
}

export interface AdminPayment {
  id: string;
  userId: number;
  userEmail: string | null;
  plan: string;
  amount: number;
  currency: string;
  status: string;
  paymentMethod: string;
  transactionId: string | null;
  createdAt: string;
  planExpiresAt: string | null;
}
