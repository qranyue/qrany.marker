namespace Marker.WebApi.Models;

public record RS<T>(
    T Data,
    bool Success = true
);

public record RE(
    string Message,
    bool Success = false
);

public record RL<T>(
    T Data,
    long Count,
    bool Success = true
) where T : System.Collections.IEnumerable;