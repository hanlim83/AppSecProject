using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Services
{
    internal interface IScopedUpdatingService
    {
        Task DoWorkAsync();
    }
}
