using Microsoft.Playwright;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpCounting.Tools;

[McpServerToolType]
public class PdfTool(IWebHostEnvironment env, IHttpContextAccessor httpContext)
{
    [McpServerTool, Description("根据 URL 将页面转换为 PDF 文件，最后返回文件链接。")]
    public async Task<string> UrlToPdf([Description("需要转换为 PDF 的 URL")] string url)
    {
        using IPlaywright playwright = await Playwright.CreateAsync();

        await using IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true, // 无头浏览器
        });

        IBrowserContext context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            // 保证页面尺寸适合 PDF 导出
            ViewportSize = new ViewportSize { Width = 1440, Height = 810 }
        });

        IPage page = await context.NewPageAsync();

        // 导航到页面并等待网络静止（等数据加载完成）
        await page.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 60000 // 最多等待 60 秒
        });

        await page.WaitForTimeoutAsync(2000); // 等待额外的 2 秒

        // 构造 PDF 路径
        var fileName = $"pdf_{Guid.NewGuid():N}.pdf";
        var pdfDir = Path.Combine(env.WebRootPath, "pdf");
        Directory.CreateDirectory(pdfDir);
        var filePath = Path.Combine(pdfDir, fileName);

        // 输出 PDF
        await page.PdfAsync(new PagePdfOptions
        {
            Path = filePath,
            Format = "A4",
            PrintBackground = true
        });

        // 构造访问 URL
        HttpRequest? request = httpContext.HttpContext?.Request;
        if (request == null)
            return "Error: Unable to determine request context.";

        var baseUrl = $"{request.Scheme}://{request.Host}";
        var publicUrl = $"{baseUrl}/pdf/{fileName}";
        return publicUrl;
    }

}
