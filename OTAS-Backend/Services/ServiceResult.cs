namespace OTAS.Services
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int? Id { get; set; } // id is used in case you need to return id of a recently added object
    }
}
