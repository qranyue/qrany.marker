namespace Marker.WebApi;

public record RS<T>(T Data, bool Success = true);

public record RE(string Message, bool Success = false);

public record RL<T>(T Data, long Count, bool Success = true) where T : System.Collections.IEnumerable;

public record CMarker(long Id, string OpenId, double Latitude, double Longitude, string Title, long TagId, string Tag, List<string> Images, bool Share = true);

public record CTag(long Id, string Name, bool Share = true);

public record MarkerResult(long Id, double Latitude, double Longitude, string Title, long TagId, string Tag);

public record InfoResult(double Latitude, double Longitude, string Title, string Tag, List<string> Images, bool Share = true, bool Edit = false);

public record EditForm(long? Id, double Latitude, double Longitude, string Title, long TagId, string Tag, List<string> Images, bool Share = true);
