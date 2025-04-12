namespace Marker.WebApi.Collections;

public record CTag(
    long Id,
    string Name,
    bool Share = true
);
