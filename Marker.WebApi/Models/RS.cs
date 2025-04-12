namespace Marker.WebApi.Models;

/// <summary>
/// 返回
/// </summary>
/// <typeparam name="T">返回类型</typeparam>
/// <param name="Data">返回值</param>
/// <param name="Success">成功</param>
public record RS<T>(
    T Data,
    bool Success = true
);

/// <summary>
/// 返回
/// </summary>
/// <param name="Message">错误消息</param>
/// <param name="Success">失败</param>
public record RE(
    string Message,
    bool Success = false
);

/// <summary>
/// 分页返回
/// </summary>
/// <typeparam name="T">页类型</typeparam>
/// <param name="Data">返回值</param>
/// <param name="Count">总条数</param>
/// <param name="Success">成功</param>
public record RL<T>(
    T Data,
    long Count,
    bool Success = true
) where T : System.Collections.IEnumerable;