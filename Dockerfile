# ==========================================
# Dockerfile cho triển khai ASP.NET Core trên Render
# ==========================================

# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src

# Copy file project và restore dependencies
COPY ["QuanLyThucTap.csproj", "./"]
RUN dotnet restore "QuanLyThucTap.csproj"

# Copy toàn bộ mã nguồn và biên dịch Release
COPY . .
RUN dotnet publish "QuanLyThucTap.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render tự động cấu hình port hoặc lắng nghe cổng 10000
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "QuanLyThucTap.dll"]
