using System.ComponentModel;

using ModelContextProtocol.Server;

namespace McpCounting.Tools;

[McpServerToolType]
public class DateTool
{
    [McpServerTool, Description("根据指定时区（格式为 ±HH:mm）获取当前日期，常用于没有具体日期时以本地时间为参考")]
    public async Task<string> GetToday([Description("时区偏移量，格式为 ±HH:mm，如 +08:00 或 -05:00")] string offset)
    {
        // 去掉正号前缀（TimeSpan.TryParse 不接受前置 +）
        var offsetForParse = offset.StartsWith('+') ? offset[1..] : offset;

        if (!TimeSpan.TryParse(offsetForParse, out TimeSpan timeZoneOffset))
        {
            throw new ArgumentException("无效的时区格式，应为 ±HH:mm，例如 +08:00");
        }

        DateTime utcNow = DateTime.UtcNow;
        DateTime localTime = utcNow + timeZoneOffset;

        var today = localTime.Date.ToString("yyyy-MM-dd");
        return await Task.FromResult(today);
    }
}
