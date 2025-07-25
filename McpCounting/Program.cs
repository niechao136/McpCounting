
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddHttpContextAccessor();

builder.Services.AddMcpServer().WithHttpTransport().WithToolsFromAssembly();

WebApplication app = builder.Build();

// ✅ 启用静态文件服务
app.UseStaticFiles();

// ✅ MCP 路由（如 http://localhost:5000/mcp）
app.MapMcp("/mcp");

app.Run();
