namespace Gympt.DTO
{
    public class ClientDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string FirstLastname { get; set; }
        public string? SecondLastname { get; set; }
        public string Ci { get; set; }
        public DateTime DateBirth { get; set; }

        public int UserId { get; set; }          // ← agregar
        public int MembershipId { get; set; }    // ← agregar

        public bool IsActive { get; set; }

        // NUEVOS CAMPOS
        public string FitnessLevel { get; set; }
        public decimal InitialWeightKg { get; set; }
        public decimal CurrentWeightKg { get; set; }
        public string EmergencyContactPhone { get; set; }
        //
        // Campos obligatorios por el microservicio

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = null!;
        public DateTime? LastModification { get; set; }
        public string? LastModifiedBy { get; set; }
    }

}
