/** Mirrors backend CustomerResponseDto + supporting DTOs */

export interface AddressDto {
  line1?: string | null;
  line2?: string | null;
  city?: string | null;
  state?: string | null;
  postalCode?: string | null;
  country?: string | null;
}

export interface Customer {
  id: string;
  code: string;
  companyName: string;
  contactPerson?: string | null;
  email?: string | null;
  phone?: string | null;
  taxId?: string | null;
  type: string;
  defaultCurrency: string;
  paymentTerms: string;
  creditLimit: number;
  isActive: boolean;
  address: AddressDto;
  notes?: string | null;
  createdAt: string;
}
