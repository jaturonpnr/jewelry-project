export interface Worker {
  id: string;
  employeeCode: string;
  fullName: string;
  phone: string | null;
  email: string | null;
  department: string | null;
  position: string;            // backend returns enum as string name
  wageType: string;
  wageRate: number;
  wageCurrency: string;
  hiredDate: string;
  terminatedDate: string | null;
  isActive: boolean;
  skills: string | null;
  notes: string | null;
  userId: string | null;
  createdAt: string;
}
