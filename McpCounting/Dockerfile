# ---------- 基础运行镜像 ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# ---------- 构建阶段：使用 Playwright 官方带依赖的镜像 ----------
FROM mcr.microsoft.com/playwright/dotnet:v1.50.0-noble AS build
ARG BUILD_CONFIGURATION=Release

# 设置工作目录
WORKDIR /src

# 拷贝项目文件并还原依赖
COPY ["McpCounting/McpCounting.csproj", "McpCounting/"]
RUN dotnet restore "McpCounting/McpCounting.csproj"

# 拷贝完整源码
COPY . .

# 进入项目目录
WORKDIR /src/McpCounting

# 编译
RUN dotnet build "McpCounting.csproj" -c $BUILD_CONFIGURATION --no-restore

# 设置 PATH 正确方式（Docker 中 ENV 不会解析 ${PATH}）
ENV DOTNET_CLI_HOME=/tmp
ENV PATH=/tmp/.dotnet/tools:$PATH

# 安装 Playwright 浏览器（不再需要 --with-deps）
RUN dotnet tool install --global Microsoft.Playwright.CLI && \
    playwright install

# 发布项目
RUN dotnet publish "McpCounting.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false


# ---------- 最终运行阶段 ----------
FROM base AS final
WORKDIR /app

# 拷贝发布后的应用
COPY --from=build /app/publish .

# 拷贝 Playwright 浏览器缓存（可选，但更推荐设置共享路径）
COPY --from=build /home/pwuser/.cache/ms-playwright /home/pwuser/.cache/ms-playwright

# 设置浏览器缓存路径（告诉 Playwright 不重新下载）
ENV PLAYWRIGHT_BROWSERS_PATH=/home/pwuser/.cache/ms-playwright

# 启动服务
ENTRYPOINT ["dotnet", "McpCounting.dll"]
