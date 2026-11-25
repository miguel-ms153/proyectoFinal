using EcoScanHogar.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoScanHogar.Api.Data;

public sealed class EcoScanDbContext(DbContextOptions<EcoScanDbContext> options) : DbContext(options)
{
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<PasswordResetEntity> PasswordResetCodes => Set<PasswordResetEntity>();
}
