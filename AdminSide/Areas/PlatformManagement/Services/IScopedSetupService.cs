using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Services
{
    internal interface IScopedSetupService
    {
        Task DoWorkAsync();
    }
}
