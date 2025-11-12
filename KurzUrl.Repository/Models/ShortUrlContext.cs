using System;
using System.Collections.Generic;
using KurzUrl.Repository.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace KurzUrl.Repository.Models;

public partial class ShortUrlContext : IdentityDbContext<ApplicationUser,IdentityRole,string>
{
    //private readonly IHostEnvironment _env;
   

    public ShortUrlContext(DbContextOptions<ShortUrlContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblUrlDetail> TblUrlDetails { get; set; }

    public virtual DbSet<TblPricing> TblPricing { get; set; }
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }

    public virtual DbSet<TblPricing> GetPricings { get; set; }

    public virtual DbSet<TblUserPricingMapper> TblUserPricingMappers { get; set; }
    public virtual DbSet<TblQRDetail> TblQRDetails { get; set; }
    public virtual DbSet<TblPricingLimit> TblPricingLimits { get; set; }
    public virtual DbSet<Resource> TblResource { get; set; }
    public virtual DbSet<TblPricingPlan> TblPricingPlans { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("server=92.205.63.184;initial catalog = ShortUrl;user=urlkurz;password=123qwe!@#QWE;TrustServerCertificate=True;MultiSubnetFailover=True;");

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    string connectionString;
    //    if (_env.IsDevelopment())
    //        connectionString = "server=LAPTOP-EQPKCMK5;initial catalog = ShortUrl;user=sa;password=john1989;Trusted_Connection=True;TrustServerCertificate=True;MultiSubnetFailover=True;";

    //    else
    //        connectionString = "server=92.205.63.184;initial catalog = ShortUrl;user=sa;password=StrongPassword#2025;TrustServerCertificate=True;MultiSubnetFailover=True;";

    //        optionsBuilder.UseSqlServer(connectionString);
    //"Server=92.205.63.184;Database=ShortUrl;User Id=ShortUrlAppLogin;Password=John2#29891987;TrustServerCertificate=True;MultiSubnetFailover=True;"
    //
    //}
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail)
                .IsUnique()
                .HasDatabaseName("EmailIndex")
                .HasFilter("[NormalizedEmail] IS NOT NULL");
        });
        
        modelBuilder.Entity<TblUrlDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tbl_urlDetail");

            entity.ToTable("tbl_UrlDetail");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.MainUrl)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.ShortUrl)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        modelBuilder.Entity<TblPricing>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tbl_Pricing");

            entity.ToTable("tbl_Pricing");

            entity.Property(e => e.Weight)
                .HasMaxLength(20)
                .IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
