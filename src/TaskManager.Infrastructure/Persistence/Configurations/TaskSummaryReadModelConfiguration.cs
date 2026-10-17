
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Infrastructure.ReadModels;

namespace TaskManager.Infrastructure.Persistence.Configurations;

public sealed class TaskSummaryReadModelConfiguration
    : IEntityTypeConfiguration<TaskSummaryReadModel>
{
    public void Configure(
        EntityTypeBuilder<TaskSummaryReadModel> builder)
    {
        builder.HasNoKey();

        builder.ToView("vw_TaskSummary", "dbo");
    }
}
