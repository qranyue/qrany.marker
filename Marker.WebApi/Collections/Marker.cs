namespace Marker.WebApi.Collections;

public record CMarker(
    long Id,
    string OpenId,
    double Latitude,
    double Longitude,
    string Content,
    long TagId,
    string Tag,
    List<string> Images,
    bool Share = true
);
