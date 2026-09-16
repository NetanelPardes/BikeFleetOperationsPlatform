using Consumer.Models;

namespace Consumer.Services;

public interface IVehicleTypesHandler
{
    Task HandleAsync( VehicleType vehicleType,CancellationToken cancellationToken = default);
}