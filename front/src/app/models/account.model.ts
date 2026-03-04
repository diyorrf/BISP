export interface AccountDto {
  email: string;
  fullName: string | null;
  tokensRemaining: number;
  plan: string;
  emailConfirmed: boolean;
}
