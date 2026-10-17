
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Infrastructure.ReadModels;

namespace TaskManager.Infrastructure.Persistence.Configurations;

public sealed class TeamTaskReportReadModelConfiguration
    : IEntityTypeConfiguration<TeamTaskReportReadModel>
{
    public void Configure(
        EntityTypeBuilder<TeamTaskReportReadModel> builder)
    {
        builder.HasNoKey();
        builder.ToTable("TeamTaskReportReadModel", table => table.ExcludeFromMigrations());
    }
}
