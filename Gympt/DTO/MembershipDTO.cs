namespace Gympt.DTO
{
    public class MembershipDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public short MonthlySessions { get; set; }
        public bool IsActive { get; set; }  

        // Campos de auditoría
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? LastModification { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
