namespace Marker.WebApi.Collections;

/// <summary>
/// 标记
/// </summary>
/// <param name="Id">主键</param>
/// <param name="OpenId">openid</param>
/// <param name="Latitude">经度</param>
/// <param name="Longitude">纬度</param>
/// <param name="Content">内容</param>
/// <param name="TagId">标签主键</param>
/// <param name="Tag">标签</param>
/// <param name="Images">图片</param>
/// <param name="Share">公开</param>
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
