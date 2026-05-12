export const MaterialCategory = {
  Metal: 1,
  PreciousStone: 2,
  SemiPreciousStone: 3,
  Finding: 4,
  Consumable: 5,
} as const;
export type MaterialCategoryValue = typeof MaterialCategory[keyof typeof MaterialCategory];

export const MATERIAL_CATEGORY_LABELS: Record<MaterialCategoryValue, string> = {
  1: 'Metal',
  2: 'Precious Stone',
  3: 'Semi-Precious Stone',
  4: 'Finding',
  5: 'Consumable',
};

export const StoneTrackingType = { Individual: 1, Parcel: 2 } as const;
export type StoneTrackingTypeValue = typeof StoneTrackingType[keyof typeof StoneTrackingType];

export const ProductionStage = {
  WaxModel: 1, Casting: 2, FilingCleaning: 3, StoneSetting: 4,
  Polishing: 5, Plating: 6, Assembly: 7, FinalQc: 8, Packaging: 9,
} as const;
export type ProductionStageValue = typeof ProductionStage[keyof typeof ProductionStage];

export const STAGE_LABELS: Record<ProductionStageValue, string> = {
  1: 'Wax Model', 2: 'Casting', 3: 'Filing/Cleaning', 4: 'Stone Setting',
  5: 'Polishing', 6: 'Plating', 7: 'Assembly', 8: 'Final QC', 9: 'Packaging',
};

// ── Response types ────────────────────────────────────────────────────────────

export interface BomCostSummary {
  materialCostThb: number;
  stoneCostThb: number;
  laborCostThb: number;
  subtotalThb: number;
  overheadPercent: number;
  overheadCostThb: number;
  totalCostPerPieceThb: number;
}

export interface BomMaterialLineResponse {
  id: string;
  category: MaterialCategoryValue;
  materialDescription: string;
  karat: number | null;
  purityFraction: number | null;
  quantityGrams: number;
  expectedLossPercent: number;
  unitCostThbPerGram: number;
  lineCostThb: number;
  sortOrder: number;
}

export interface BomStoneLineResponse {
  id: string;
  stoneType: string;
  stoneShape: string;
  sizeDescription: string;
  caratPerStone: number;
  quantity: number;
  totalCaratWeight: number;
  trackingType: StoneTrackingTypeValue;
  unitCostThbPerCarat: number;
  lineCostThb: number;
  sortOrder: number;
}

export interface BomLaborLineResponse {
  id: string;
  stage: ProductionStageValue;
  stageName: string;
  estimatedHours: number;
  hourlyRateThb: number;
  laborCostThb: number;
}

export interface BomTemplateResponse {
  id: string;
  designCode: string;
  designName: string;
  description: string | null;
  isActive: boolean;
  overheadPercent: number;
  materialLines: BomMaterialLineResponse[];
  stoneLines: BomStoneLineResponse[];
  laborLines: BomLaborLineResponse[];
  costSummary: BomCostSummary;
  createdAt: string;
  updatedAt: string | null;
}

export interface BomTemplateSummary {
  id: string;
  designCode: string;
  designName: string;
  isActive: boolean;
  materialLineCount: number;
  stoneLineCount: number;
  laborLineCount: number;
  totalCostPerPieceThb: number;
  createdAt: string;
}

// ── Request DTOs ──────────────────────────────────────────────────────────────

export interface BomMaterialLineDto {
  id?: string | null;
  category: MaterialCategoryValue;
  materialDescription: string;
  karat?: number | null;
  purityFraction?: number | null;
  quantityGrams: number;
  expectedLossPercent: number;
  unitCostThbPerGram: number;
  sortOrder: number;
}

export interface BomStoneLineDto {
  id?: string | null;
  stoneType: string;
  stoneShape: string;
  sizeDescription: string;
  caratPerStone: number;
  quantity: number;
  trackingType: StoneTrackingTypeValue;
  unitCostThbPerCarat: number;
  sortOrder: number;
}

export interface BomLaborLineDto {
  id?: string | null;
  stage: ProductionStageValue;
  estimatedHours: number;
  hourlyRateThb: number;
}

export interface CreateBomTemplateDto {
  designCode: string;
  designName: string;
  description?: string | null;
  overheadPercent: number;
  materialLines: BomMaterialLineDto[];
  stoneLines: BomStoneLineDto[];
  laborLines: BomLaborLineDto[];
}

export interface UpdateBomTemplateDto extends CreateBomTemplateDto {
  isActive: boolean;
}
