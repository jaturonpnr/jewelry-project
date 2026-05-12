export const QcInspectionType = { Incoming: 1, InProcess: 2, Final: 3 } as const;
export type QcInspectionTypeValue = typeof QcInspectionType[keyof typeof QcInspectionType];

export const QC_TYPE_LABELS: Record<QcInspectionTypeValue, string> = {
  1: 'Incoming', 2: 'In-Process', 3: 'Final',
};

export const QcResult = { Pass: 1, Rework: 2, Fail: 3 } as const;
export type QcResultValue = typeof QcResult[keyof typeof QcResult];

export const QC_RESULT_LABELS: Record<QcResultValue, string> = {
  1: 'Pass', 2: 'Rework', 3: 'Fail',
};

export const DefectSeverity = { Minor: 1, Major: 2, Critical: 3 } as const;
export type DefectSeverityValue = typeof DefectSeverity[keyof typeof DefectSeverity];

export const DEFECT_SEVERITY_LABELS: Record<DefectSeverityValue, string> = {
  1: 'Minor', 2: 'Major', 3: 'Critical',
};

export const ProductionStage = {
  WaxModel: 1, Casting: 2, FilingCleaning: 3, StoneSetting: 4,
  Polishing: 5, Plating: 6, Assembly: 7, FinalQc: 8, Packaging: 9,
} as const;
export type ProductionStageValue = typeof ProductionStage[keyof typeof ProductionStage];
export const STAGE_LABELS: Record<ProductionStageValue, string> = {
  1: 'Wax Model', 2: 'Casting', 3: 'Filing/Cleaning', 4: 'Stone Setting',
  5: 'Polishing', 6: 'Plating', 7: 'Assembly', 8: 'Final QC', 9: 'Packaging',
};

export const DEFECT_TYPE_PRESETS = [
  'Scratch', 'Porosity', 'Crack', 'Loose Stone', 'Missing Stone',
  'Wrong Size', 'Wrong Karat', 'Surface Pit', 'Plating Defect',
  'Solder Mark', 'Sharp Edge', 'Weight Deviation',
];

// ── Response types ────────────────────────────────────────────────────────────

export interface QcDefectResponse {
  id: string;
  defectType: string;
  severity: DefectSeverityValue;
  description: string | null;
  quantity: number;
}

export interface QcInspectionResponse {
  id: string;
  inspectionType: QcInspectionTypeValue;
  inspectionDate: string;
  workOrderId: string | null;
  workOrderNumber: string | null;
  workOrderStageId: string | null;
  workOrderStageName: string | null;
  rawMaterialItemId: string | null;
  rawMaterialDescription: string | null;
  inspectorId: string | null;
  inspectorName: string;
  result: QcResultValue;
  notes: string | null;
  reworkStage: ProductionStageValue | null;
  actualWeightGrams: number | null;
  expectedWeightGrams: number | null;
  defectCount: number;
  defects: QcDefectResponse[];
  createdAt: string;
}

export interface QcInspectionSummary {
  id: string;
  inspectionType: QcInspectionTypeValue;
  inspectionDate: string;
  workOrderNumber: string | null;
  rawMaterialDescription: string | null;
  inspectorName: string;
  result: QcResultValue;
  defectCount: number;
  createdAt: string;
}

// ── Request DTOs ──────────────────────────────────────────────────────────────

export interface QcDefectDto {
  defectType: string;
  severity: DefectSeverityValue;
  description?: string | null;
  quantity: number;
}

export interface CreateQcInspectionDto {
  inspectionType: QcInspectionTypeValue;
  inspectionDate: string;
  workOrderId?: string | null;
  workOrderStageId?: string | null;
  rawMaterialItemId?: string | null;
  inspectorId?: string | null;
  inspectorName: string;
  result: QcResultValue;
  notes?: string | null;
  reworkStage?: ProductionStageValue | null;
  actualWeightGrams?: number | null;
  expectedWeightGrams?: number | null;
  defects: QcDefectDto[];
}
