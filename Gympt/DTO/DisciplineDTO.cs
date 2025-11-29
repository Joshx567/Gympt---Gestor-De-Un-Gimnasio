namespace Gympt.DTO
{
    public class DisciplineDTO
    {
        public short Id { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; } = true;
        public long? IdUser { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModification { get; set; }
    }
}