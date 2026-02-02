using System.Threading;
using System.Threading.Tasks;

namespace CarDealership.Services.CarValuation
{
    public interface ICarValuationService
    {
        Task<CarValuationResult> GetValuationAsync(
            CarValuationRequest request,
            CancellationToken ct = default);
    }
}
