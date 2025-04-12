namespace Marker.WebApi.Collections;

/// <summary>
/// 标签
/// </summary>
/// <param name="Id">主键</param>
/// <param name="Name">名称</param>
/// <param name="Share">公开</param>
public record CTag(
    long Id,
    string Name,
    bool Share = true
);
