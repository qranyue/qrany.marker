namespace Marker.WebApi.Models;

/// <summary>
/// 标记
/// </summary>
/// <param name="Id">主键</param>
/// <param name="Latitude">经度</param>
/// <param name="Longitude">纬度</param>
/// <param name="Content">内容</param>
/// <param name="TagId">标签主键</param>
/// <param name="Tag">标签</param>
public record MarkerResult(
    long Id,
    double Latitude,
    double Longitude,
    string Content,
    long TagId,
    string Tag
);

/// <summary>
/// 标记详情
/// </summary>
/// <param name="Latitude">经度</param>
/// <param name="Longitude">纬度</param>
/// <param name="Content">内容</param>
/// <param name="Tag">标签</param>
/// <param name="Images">图片</param>
/// <param name="Share">公开</param>
/// <param name="Edit">编辑</param>
public record InfoResult(
    double Latitude,
    double Longitude,
    string Content,
    string Tag,
    List<string> Images,
    bool Share = true,
    bool Edit = false
);

/// <summary>
/// 编辑表单
/// </summary>
/// <param name="Id">主键</param>
/// <param name="Latitude">经度</param>
/// <param name="Longitude">纬度</param>
/// <param name="Content">内容</param>
/// <param name="Tag">标签</param>
/// <param name="Images">图片</param>
/// <param name="Share">公开</param>
public record EditForm(
    long? Id,
    double Latitude,
    double Longitude,
    string Content,
    string Tag,
    List<string> Images,
    bool Share = true
);