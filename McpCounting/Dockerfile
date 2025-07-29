# 基础运行镜像
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 安装 Playwright CLI
RUN dotnet tool install --global Microsoft.Playwright.CLI

# 添加 bin 到 PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# 复制项目并还原依赖
COPY ["McpCounting/McpCounting.csproj", "McpCounting/"]
RUN dotnet restore "McpCounting/McpCounting.csproj"

# 复制源码
COPY . .

# 安装 Playwright 浏览器
WORKDIR /src/McpCounting
RUN playwright install --with-deps

# 编译项目
RUN dotnet build "./McpCounting.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 发布阶段
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./McpCounting.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 最终运行阶段
FROM base AS final
WORKDIR /app

# 复制发布文件
COPY --from=publish /app/publish .

# 添加 playwright 缓存（浏览器文件）
COPY --from=publish /root/.cache/ms-playwright /root/.cache/ms-playwright

# 设置环境变量：告诉 Playwright 浏览器缓存在哪里
ENV PLAYWRIGHT_BROWSERS_PATH=/root/.cache/ms-playwright

ENTRYPOINT ["dotnet", "McpCounting.dll"]
