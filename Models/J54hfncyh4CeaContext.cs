using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CEA_API.Models;

public partial class J54hfncyh4CeaContext : DbContext
{
    public J54hfncyh4CeaContext()
    {
    }

    public J54hfncyh4CeaContext(DbContextOptions<J54hfncyh4CeaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CeaKindToken> CeaKindTokens { get; set; }

    public virtual DbSet<CeaMovementTraceability> CeaMovementTraceabilities { get; set; }

    public virtual DbSet<CeaPermission> CeaPermissions { get; set; }

    public virtual DbSet<CeaPermissionsGranted> CeaPermissionsGranteds { get; set; }

    public virtual DbSet<CeaRolesUser> CeaRolesUsers { get; set; }

    public virtual DbSet<CeaSystem> CeaSystems { get; set; }

    public virtual DbSet<CeaToken> CeaTokens { get; set; }

    public virtual DbSet<CeaTraceability> CeaTraceabilities { get; set; }

    public virtual DbSet<CeaUser> CeaUsers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Conexión a la base de datos");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CeaKindToken>(entity =>
        {
            entity.HasKey(e => e.IdKindToken).HasName("PK__CEA_KIND__8B30E34C7E6EB655");

            entity.ToTable("CEA_KIND_TOKEN");

            entity.Property(e => e.IdKindToken).HasColumnName("id_kind_token");
            entity.Property(e => e.NameKindToken)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name_kind_token");
        });

        modelBuilder.Entity<CeaMovementTraceability>(entity =>
        {
            entity.HasKey(e => e.IdMovement).HasName("PK__CEA_MOVE__465F1AE47B98D5C5");

            entity.ToTable("CEA_MOVEMENT_TRACEABILITY");

            entity.Property(e => e.IdMovement).HasColumnName("id_movement");
            entity.Property(e => e.NameMovement)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name_movement");
        });

        modelBuilder.Entity<CeaPermission>(entity =>
        {
            entity.HasKey(e => e.IdPermission).HasName("PK__CEA_PERM__5180B3BF258905CD");

            entity.ToTable("CEA_PERMISSIONS");

            entity.Property(e => e.IdPermission).HasColumnName("id_permission");
            entity.Property(e => e.CodePermission)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("code_permission");
            entity.Property(e => e.IdSystemPermission).HasColumnName("id_system_permission");

            entity.HasOne(d => d.IdSystemPermissionNavigation).WithMany(p => p.CeaPermissions)
                .HasForeignKey(d => d.IdSystemPermission)
                .HasConstraintName("FK_System_Permission");
        });

        modelBuilder.Entity<CeaPermissionsGranted>(entity =>
        {
            entity.HasKey(e => e.IdPermissionGranted).HasName("PK__CEA_PERM__1F450471A54E5873");

            entity.ToTable("CEA_PERMISSIONS_GRANTED");

            entity.Property(e => e.IdPermissionGranted).HasColumnName("id_permission_granted");
            entity.Property(e => e.IdPermissionGrantedPermission).HasColumnName("id_permission_granted_permission");
            entity.Property(e => e.IdPermissionGrantedUser).HasColumnName("id_permission_granted_user");

            entity.HasOne(d => d.IdPermissionGrantedPermissionNavigation).WithMany(p => p.CeaPermissionsGranteds)
                .HasForeignKey(d => d.IdPermissionGrantedPermission)
                .HasConstraintName("FK_Permission_Granted_Permission");

            entity.HasOne(d => d.IdPermissionGrantedUserNavigation).WithMany(p => p.CeaPermissionsGranteds)
                .HasForeignKey(d => d.IdPermissionGrantedUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Permission_Granted_User");
        });

        modelBuilder.Entity<CeaRolesUser>(entity =>
        {
            entity.HasKey(e => e.IdRolUser).HasName("PK__CEA_ROLE__8861CF8E33D338A0");

            entity.ToTable("CEA_ROLES_USER");

            entity.Property(e => e.IdRolUser).HasColumnName("id_rol_user");
            entity.Property(e => e.NameRolUser)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name_rol_user");
        });

        modelBuilder.Entity<CeaSystem>(entity =>
        {
            entity.HasKey(e => e.IdSystem).HasName("PK__CEA_SYST__FB4F30AF4E7D764B");

            entity.ToTable("CEA_SYSTEMS");

            entity.Property(e => e.IdSystem).HasColumnName("id_system");
            entity.Property(e => e.LinkSystems).HasColumnName("link_systems");
            entity.Property(e => e.NameSystem)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name_system");
        });

        modelBuilder.Entity<CeaToken>(entity =>
        {
            entity.HasKey(e => e.IdToken).HasName("PK__CEA_TOKE__3C2FA9C45D41D7BE");

            entity.ToTable("CEA_TOKENS");

            entity.Property(e => e.IdToken).HasColumnName("id_token");
            entity.Property(e => e.IdTokenKindToken).HasColumnName("id_token_kind_token");
            entity.Property(e => e.IdTokenUser).HasColumnName("id_token_user");
            entity.Property(e => e.Token).HasColumnName("token");

            entity.HasOne(d => d.IdTokenKindTokenNavigation).WithMany(p => p.CeaTokens)
                .HasForeignKey(d => d.IdTokenKindToken)
                .HasConstraintName("FK_Tokens_Kind_Token");

            entity.HasOne(d => d.IdTokenUserNavigation).WithMany(p => p.CeaTokens)
                .HasForeignKey(d => d.IdTokenUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tokens_User");
        });

        modelBuilder.Entity<CeaTraceability>(entity =>
        {
            entity.HasKey(e => e.IdTraceability).HasName("PK__CEA_TRAC__3F3B45AA04D0D13A");

            entity.ToTable("CEA_TRACEABILITY");

            entity.Property(e => e.IdTraceability).HasColumnName("id_traceability");
            entity.Property(e => e.IdTraceabilityMovementTraceability).HasColumnName("id_traceability_movement_traceability");
            entity.Property(e => e.IdTraceabilityUser).HasColumnName("id_traceability_user");
            entity.Property(e => e.TraceabilityDate).HasColumnName("traceability_date");
            entity.Property(e => e.TraceabilityTime).HasColumnName("traceability_time");

            entity.HasOne(d => d.IdTraceabilityMovementTraceabilityNavigation).WithMany(p => p.CeaTraceabilities)
                .HasForeignKey(d => d.IdTraceabilityMovementTraceability)
                .HasConstraintName("FK_Traceability_Movement_Traceability");
        });

        modelBuilder.Entity<CeaUser>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PK__CEA_USER__D2D14637612215EC");

            entity.ToTable("CEA_USERS", tb => tb.HasTrigger("trg_delete_users"));

            entity.HasIndex(e => e.EmailUser, "UQ_CEA_USERS_email_user").IsUnique();

            entity.HasIndex(e => e.NameUser, "UQ__CEA_USER__B32D8039DEC7A307").IsUnique();

            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EmailState).HasDefaultValue(false);
            entity.Property(e => e.EmailUser)
                .HasMaxLength(100)
                .HasColumnName("email_user");
            entity.Property(e => e.IdUsersRolUser).HasColumnName("id_users_rol_user");
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.NameUser)
                .HasMaxLength(100)
                .HasColumnName("name_user");
            entity.Property(e => e.PassUser)
                .HasMaxLength(256)
                .HasColumnName("pass_user");

            entity.HasOne(d => d.IdUsersRolUserNavigation).WithMany(p => p.CeaUsers)
                .HasForeignKey(d => d.IdUsersRolUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
