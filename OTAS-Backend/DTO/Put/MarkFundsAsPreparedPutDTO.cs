using Microsoft.AspNetCore.Mvc;

namespace OTAS.DTO.Put
{
    public class MarkFundsAsPreparedPutDTO
    {
        public int RequestId { get; set; }
        public string AdvanceOption { get; set; } = null!;
    }
}
