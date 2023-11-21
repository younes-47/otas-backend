using OTAS.Models;

namespace OTAS.DTO.Get
{
    public class StatusHistoryDTO
    {
        //public int Id { get; set; }

        //public int? AvanceVoyageId { get; set; }

        //public int? AvanceCaisseId { get; set; }

        //public int? DepenseCaisseId { get; set; }

        //public int? OrdreMissionId { get; set; }

        //public int? LiquidationId { get; set; }

        public int Status { get; set; }

        public int? DeciderUserId { get; set; }

        public string? DeciderComment { get; set; }

        public DateTime CreateDate { get; set; }

    }
}
