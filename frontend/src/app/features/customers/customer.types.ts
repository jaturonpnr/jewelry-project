/** Mirrors backend CustomerResponseDto + supporting DTOs */

export interface AddressDto {
  line1?: string | null;
  line2?: string | null;
  city?: string | null;
  state?: string | null;
  postalCode?: string | null;
  country?: string | null;
}

// Match backend enum int values
export const CustomerType = {
  Wholesaler: 1,
  Retailer: 2,
  CataloguePublisher: 3,
  Other: 99,
} as const;

export const Currency = {
  THB: 1,
  USD: 2,
  EUR: 3,
  GBP: 4,
  AUD: 5,
  NZD: 6,
  SGD: 7,
  JPY: 8,
} as const;

export const PaymentTerms = {
  Prepaid: 1,
  Net15: 2,
  Net30: 3,
  Net45: 4,
  Net60: 5,
  Net90: 6,
  CashOnDelivery: 7,
  LetterOfCredit: 8,
} as const;

export type CustomerTypeValue = typeof CustomerType[keyof typeof CustomerType];
export type CurrencyValue = typeof Currency[keyof typeof Currency];
export type PaymentTermsValue = typeof PaymentTerms[keyof typeof PaymentTerms];

export interface Customer {
  id: string;
  code: string;
  companyName: string;
  contactPerson?: string | null;
  email?: string | null;
  phone?: string | null;
  taxId?: string | null;
  type: string;        // server returns string name (e.g. "Wholesaler")
  defaultCurrency: string;
  paymentTerms: string;
  creditLimit: number;
  isActive: boolean;
  address: AddressDto;
  notes?: string | null;
  createdAt: string;
}

export interface CreateCustomerDto {
  code: string;
  companyName: string;
  contactPerson?: string | null;
  email?: string | null;
  phone?: string | null;
  taxId?: string | null;
  type: CustomerTypeValue;
  defaultCurrency: CurrencyValue;
  paymentTerms: PaymentTermsValue;
  creditLimit: number;
  address: AddressDto;
  notes?: string | null;
}

export interface UpdateCustomerDto extends Omit<CreateCustomerDto, 'code'> {
  isActive: boolean;
}
