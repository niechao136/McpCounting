# ---------- 基础运行镜像 ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# ---------- 构建阶段 ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

# 设置工作目录
WORKDIR /src

# 复制项目文件并还原依赖
COPY ["McpCounting/McpCounting.csproj", "McpCounting/"]
RUN dotnet restore "McpCounting/McpCounting.csproj"

# 拷贝完整源码
COPY . .

# 进入项目根目录
WORKDIR /src/McpCounting

# 编译项目（可选，但建议先 build 再 publish）
RUN dotnet build "McpCounting.csproj" -c $BUILD_CONFIGURATION --no-restore

# 设置 PATH 正确方式（Docker 中 ENV 不会解析 ${PATH}）
ENV DOTNET_CLI_HOME=/tmp
ENV PATH=/root/.dotnet/tools:$PATH

# 安装 Playwright CLI 工具
RUN dotnet tool install --global Microsoft.Playwright.CLI

# 安装 Playwright 浏览器依赖（必须在 build 后执行）
RUN /root/.dotnet/tools/playwright install --with-deps

# 发布项目到发布目录（不使用 AppHost 以简化容器）
RUN dotnet publish "McpCounting.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false


# ---------- 最终运行阶段 ----------
FROM base AS final
WORKDIR /app

# 拷贝发布后的代码
COPY --from=build /app/publish .

# 拷贝 Playwright 浏览器缓存（浏览器二进制）
COPY --from=build /root/.cache/ms-playwright /root/.cache/ms-playwright

# 设置 Playwright 浏览器缓存路径
ENV PLAYWRIGHT_BROWSERS_PATH=/root/.cache/ms-playwright

# 启动入口
ENTRYPOINT ["dotnet", "McpCounting.dll"]
