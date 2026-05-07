namespace JewelryFactory.Domain.Enums;

/// <summary>
/// Standard 9-stage jewelry production workflow.
/// Stages are auto-created in this order when a WorkOrder is created.
/// </summary>
public enum ProductionStage
{
    WaxModel = 1,         // ทำตัวต้นแบบขี้ผึ้ง
    Casting = 2,          // หล่อทอง
    FilingCleaning = 3,   // ตะไบ + ทำความสะอาด
    StoneSetting = 4,     // ฝังหิน
    Polishing = 5,        // ขัดเงา
    Plating = 6,          // ชุบ rhodium / ทอง
    Assembly = 7,         // ประกอบ (สำหรับงานหลายชิ้น)
    FinalQc = 8,          // ตรวจคุณภาพสุดท้าย
    Packaging = 9         // แพ็ค
}

public enum WorkOrderStatus
{
    Draft = 1,
    Released = 2,         // สั่งเริ่มผลิตแล้ว
    InProgress = 3,
    OnHold = 4,           // หยุดชั่วคราว (รอวัสดุ ฯลฯ)
    Completed = 5,
    Cancelled = 99
}

public enum WorkOrderStageStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Skipped = 4,          // stage นี้ไม่ใช้สำหรับงานนี้ (เช่น ไม่มีหินจะ set)
    Failed = 5            // QC fail — ต้องส่งซ่อม
}

public enum WorkOrderPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}

/// <summary>
/// Allowed transitions per CLAUDE.md §10.9 style.
/// </summary>
public static class WorkOrderStatusFlow
{
    private static readonly Dictionary<WorkOrderStatus, WorkOrderStatus[]> Allowed = new()
    {
        [WorkOrderStatus.Draft]      = [WorkOrderStatus.Released,    WorkOrderStatus.Cancelled],
        [WorkOrderStatus.Released]   = [WorkOrderStatus.InProgress,  WorkOrderStatus.Cancelled],
        [WorkOrderStatus.InProgress] = [WorkOrderStatus.OnHold,      WorkOrderStatus.Completed, WorkOrderStatus.Cancelled],
        [WorkOrderStatus.OnHold]     = [WorkOrderStatus.InProgress,  WorkOrderStatus.Cancelled],
        [WorkOrderStatus.Completed]  = [],
        [WorkOrderStatus.Cancelled]  = [],
    };

    public static bool CanTransition(WorkOrderStatus from, WorkOrderStatus to)
        => Allowed.TryGetValue(from, out var next) && next.Contains(to);
}
