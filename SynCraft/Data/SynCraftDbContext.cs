using Microsoft.EntityFrameworkCore;
using SynCraft.Models;

namespace SynCraft.Data;

public class SynCraftDbContext : DbContext
{
    public SynCraftDbContext(DbContextOptions<SynCraftDbContext> options) : base(options) { }

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<ProcessTemplate> ProcessTemplates => Set<ProcessTemplate>();
    public DbSet<StepTemplate> StepTemplates => Set<StepTemplate>();
    public DbSet<ProcessInstance> ProcessInstances => Set<ProcessInstance>();
    public DbSet<StepInstance> StepInstances => Set<StepInstance>();
    public DbSet<StepComment> StepComments => Set<StepComment>();
    public DbSet<MilestoneTemplate> MilestoneTemplates => Set<MilestoneTemplate>();
    public DbSet<MilestoneInstance> MilestoneInstances => Set<MilestoneInstance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StepTemplate>()
            .HasOne(s => s.ProcessTemplate)
            .WithMany(p => p.Steps)
            .HasForeignKey(s => s.ProcessTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StepInstance>()
            .HasOne(s => s.ProcessInstance)
            .WithMany(p => p.Steps)
            .HasForeignKey(s => s.ProcessInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StepTemplate>()
            .HasOne(s => s.ResponsiblePerson)
            .WithMany()
            .HasForeignKey(s => s.ResponsiblePersonId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<StepInstance>()
            .HasOne(s => s.ResponsiblePerson)
            .WithMany()
            .HasForeignKey(s => s.ResponsiblePersonId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<StepComment>()
            .HasOne(c => c.StepInstance)
            .WithMany(s => s.Comments)
            .HasForeignKey(c => c.StepInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MilestoneTemplate>()
            .HasOne(m => m.ProcessTemplate)
            .WithMany(p => p.Milestones)
            .HasForeignKey(m => m.ProcessTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MilestoneInstance>()
            .HasOne(m => m.ProcessInstance)
            .WithMany(p => p.Milestones)
            .HasForeignKey(m => m.ProcessInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
