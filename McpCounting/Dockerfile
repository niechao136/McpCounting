# 基础运行镜像
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 安装 Playwright CLI（.NET 版）
RUN dotnet tool install --global Microsoft.Playwright.CLI

# 添加 .NET 工具路径到 PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# 复制项目文件并还原依赖
COPY ["McpCounting/McpCounting.csproj", "McpCounting/"]
RUN dotnet restore "McpCounting/McpCounting.csproj"

# 复制源码
COPY . .

# 设置工作目录为项目根
WORKDIR /src/McpCounting

# 构建项目
RUN dotnet build "McpCounting.csproj" -c $BUILD_CONFIGURATION

# 安装 Playwright 浏览器（必须在构建后）
RUN dotnet playwright install --with-deps

# 发布应用
RUN dotnet publish "McpCounting.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


# 最终运行阶段
FROM base AS final
WORKDIR /app

# 复制发布输出
COPY --from=build /app/publish .

# 复制 Playwright 浏览器缓存（来自 build 阶段）
COPY --from=build /root/.cache/ms-playwright /root/.cache/ms-playwright

# 设置 Playwright 浏览器路径
ENV PLAYWRIGHT_BROWSERS_PATH=/root/.cache/ms-playwright

ENTRYPOINT ["dotnet", "McpCounting.dll"]
