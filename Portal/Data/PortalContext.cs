using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Portal.Models;

namespace Portal.Data
{
    public partial class PortalContext : DbContext
    {
        public PortalContext()
        {
        }

        public PortalContext(DbContextOptions<PortalContext> options)
            : base(options)
        {
        }

        public virtual DbSet<WheelSection> WheelSections { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WheelSection>(entity =>
            {
                entity.HasKey(e => e.PkWheelId);

                entity.HasIndex(e => e.FkParentWheelId, "IX_WheelSections_FkParentWheelId");

                entity.Property(e => e.Colour).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("('2025-08-20T19:00:20.6613896+05:00')");

                entity.Property(e => e.Name).HasMaxLength(255);


                entity.HasOne(d => d.FkParentWheel)
                      .WithMany(p => p.Children)
                      .HasForeignKey(d => d.FkParentWheelId);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
