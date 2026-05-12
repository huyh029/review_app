namespace reviewApi.Service
{
    public interface ISetupService
    {
        Task SetBaseDataAsync();
        Task RemoveDataAsync();
    }
}
