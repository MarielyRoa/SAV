namespace SAV.application.Services
{
    using Microsoft.Extensions.Configuration;
    using SAV.application.Interfaces;
    using SAV.application.Repository;
    using SAV.application.Resultado;

    public class DataWarehouseService : IDataWarehouseService
    {
        private readonly IDwRepository _dwhRepository;
        private readonly IConfiguration _configuration;

        public DataWarehouseService(IDwRepository dwhRepository,
                                    IConfiguration configuration)
        {
            _dwhRepository = dwhRepository;
            _configuration = configuration;
        }

        public async Task<Result> ProcessDimensionsLoadAsync()
        {
            Result serviceResult = new Result();

            try
            {
                serviceResult = await _dwhRepository.LoadDimsDataAsync();
            }
            catch (Exception ex)
            {
                serviceResult.IsSuccess = false;
                serviceResult.Message = ex.Message;
            }

            return serviceResult;
        }
    }
}