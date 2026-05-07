import { AddressDto } from '../customers/customer.types';

export const SupplierType = {
  Gold: 1,
  Silver: 2,
  PreciousStone: 3,
  Findings: 4,
  Equipment: 5,
  Consumable: 6,
  Other: 99,
} as const;

export interface Supplier {
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
  isActive: boolean;
  address: AddressDto;
  certifications?: string | null;
  notes?: string | null;
  createdAt: string;
}
