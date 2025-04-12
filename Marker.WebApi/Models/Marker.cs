namespace Marker.WebApi.Models;

public record MarkerResult(
    long Id,
    double Latitude,
    double Longitude,
    string Content,
    long TagId,
    string Tag
);

public record InfoResult(
    double Latitude,
    double Longitude,
    string Content,
    string Tag,
    List<string> Images,
    bool Share = true,
    bool Edit = false
);

public record EditForm(
    long? Id,
    double Latitude,
    double Longitude,
    string Content,
    string Tag,
    List<string> Images,
    bool Share = true
);