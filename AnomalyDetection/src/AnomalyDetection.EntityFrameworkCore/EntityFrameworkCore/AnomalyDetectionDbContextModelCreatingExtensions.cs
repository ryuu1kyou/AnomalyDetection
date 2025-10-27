using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;
using AnomalyDetection.MultiTenancy;
using AnomalyDetection.CanSignals;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.Projects;
using AnomalyDetection.OemTraceability;

namespace AnomalyDetection.EntityFrameworkCore;

public static class AnomalyDetectionDbContextModelCreatingExtensions
{
    public static void ConfigureAnomalyDetection(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        /* Configure your own tables/entities inside here */

        // ============================================================================
        // 1. マルチテナント管理
        // ============================================================================

        #region OemMaster (Aggregate Root)

        builder.Entity<OemMaster>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "OemMasters", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            // Value Object: OemCode
            b.OwnsOne(x => x.OemCode, oemCode =>
            {
                oemCode.Property(x => x.Code)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("OemCode");

                oemCode.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("OemName");
            });

            // Properties
            b.Property(x => x.CompanyName).IsRequired().HasMaxLength(200);
            b.Property(x => x.Country).IsRequired().HasMaxLength(100);
            b.Property(x => x.ContactEmail).HasMaxLength(256);
            b.Property(x => x.ContactPhone).HasMaxLength(50);
            b.Property(x => x.Description).HasMaxLength(1000);

            // Owned Collection: Features
            b.OwnsMany(x => x.Features, features =>
            {
                features.ToTable(AnomalyDetectionConsts.DbTablePrefix + "OemFeatures", AnomalyDetectionConsts.DbSchema);
                features.WithOwner().HasForeignKey("OemMasterId");
                features.Property<Guid>("OemMasterId");
                features.HasKey("OemMasterId", "FeatureName");

                features.Property(x => x.FeatureName).IsRequired().HasMaxLength(100);
                features.Property(x => x.FeatureValue).HasMaxLength(500);
            });
        });

        #endregion

        #region ExtendedTenant (Aggregate Root)

        builder.Entity<ExtendedTenant>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "ExtendedTenants", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            // Value Object: OemCode
            b.OwnsOne(x => x.OemCode, oemCode =>
            {
                oemCode.Property(x => x.Code)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("OemCode");

                oemCode.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("OemName");
            });

            // Properties
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.DatabaseConnectionString).HasMaxLength(1000);
            b.Property(x => x.Description).HasMaxLength(1000);

            // Owned Collection: Features
            b.OwnsMany(x => x.Features, features =>
            {
                features.ToTable(AnomalyDetectionConsts.DbTablePrefix + "TenantFeatures", AnomalyDetectionConsts.DbSchema);
                features.WithOwner().HasForeignKey("ExtendedTenantId");
                features.Property<Guid>("ExtendedTenantId");
                features.HasKey("ExtendedTenantId", "FeatureName");

                features.Property(x => x.FeatureName).IsRequired().HasMaxLength(100);
                features.Property(x => x.FeatureValue).HasMaxLength(500);
                features.Property(x => x.CreatedBy).HasMaxLength(256);
                features.Property(x => x.UpdatedBy).HasMaxLength(256);
            });

            // Relationships
            b.HasOne<OemMaster>()
                .WithMany()
                .HasForeignKey(x => x.OemMasterId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            b.HasIndex(x => x.Name).IsUnique();
            b.HasIndex(x => x.IsActive);
            b.HasIndex(x => x.ExpirationDate);
        });

        #endregion

        // ============================================================================
        // 2. CAN信号管理
        // ============================================================================

        #region CanSignal (Aggregate Root)

        builder.Entity<CanSignal>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "CanSignals", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            // Value Object: SignalIdentifier
            b.OwnsOne(x => x.Identifier, identifier =>
            {
                identifier.Property(x => x.SignalName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("SignalName");

                identifier.Property(x => x.CanId)
                    .IsRequired()
                    .HasMaxLength(8)
                    .HasColumnName("CanId");
            });

            // Value Object: SignalSpecification (with nested ValueRange)
            b.OwnsOne(x => x.Specification, spec =>
            {
                spec.Property(x => x.StartBit).HasColumnName("StartBit");
                spec.Property(x => x.Length).HasColumnName("Length");
                spec.Property(x => x.DataType).HasColumnName("DataType");
                spec.Property(x => x.ByteOrder).HasColumnName("ByteOrder");

                // Nested Value Object: ValueRange
                spec.OwnsOne(x => x.ValueRange, range =>
                {
                    range.Property(x => x.MinValue).HasColumnName("MinValue");
                    range.Property(x => x.MaxValue).HasColumnName("MaxValue");
                });
            });

            // Value Object: PhysicalValueConversion
            b.OwnsOne(x => x.Conversion, conversion =>
            {
                conversion.Property(x => x.Factor).HasColumnName("Factor");
                conversion.Property(x => x.Offset).HasColumnName("Offset");
                conversion.Property(x => x.Unit).HasMaxLength(50).HasColumnName("Unit");
            });

            // Value Object: SignalTiming
            b.OwnsOne(x => x.Timing, timing =>
            {
                timing.Property(x => x.CycleTimeMs).HasColumnName("CycleTimeMs");
                timing.Property(x => x.TimeoutMs).HasColumnName("TimeoutMs");
                timing.Property(x => x.SendType).HasColumnName("SendType");
            });

            // Value Object: OemCode
            b.OwnsOne(x => x.OemCode, oemCode =>
            {
                oemCode.Property(x => x.Code)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("OemCode");

                oemCode.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("OemName");
            });

            // Value Object: SignalVersion
            b.OwnsOne(x => x.Version, version =>
            {
                version.Property(x => x.Major).HasColumnName("VersionMajor");
                version.Property(x => x.Minor).HasColumnName("VersionMinor");
            });

            // Properties
            b.Property(x => x.SystemType).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.SourceDocument).HasMaxLength(500);
            b.Property(x => x.Notes).HasMaxLength(2000);

            // Indexes
            b.HasIndex(x => x.SystemType);
            b.HasIndex(x => x.Status);
        });

        #endregion

        #region CanSystemCategory (Aggregate Root)

        builder.Entity<CanSystemCategory>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "CanSystemCategories", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            // Value Object: SystemCategoryConfiguration
            b.OwnsOne(x => x.Configuration, config =>
            {
                config.Property(x => x.Priority).HasColumnName("Priority");
                config.Property(x => x.IsSafetyRelevant).HasColumnName("IsSafetyRelevant");
                config.Property(x => x.RequiresRealTimeMonitoring).HasColumnName("RequiresRealTimeMonitoring");
                config.Property(x => x.DefaultTimeoutMs).HasColumnName("DefaultTimeoutMs");
                config.Property(x => x.MaxSignalsPerCategory).HasColumnName("MaxSignalsPerCategory");

                config.Property(x => x.CustomSettings)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions)null))
                    .HasColumnName("CustomSettings");
            });

            // Properties
            b.Property(x => x.SystemType).IsRequired();
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.Icon).HasMaxLength(100);
            b.Property(x => x.Color).HasMaxLength(20);

            // Indexes
            b.HasIndex(x => x.SystemType).IsUnique();
            b.HasIndex(x => x.DisplayOrder);
            b.HasIndex(x => x.IsActive);
        });

        #endregion

        // ============================================================================
        // 3. 異常検出ロジック管理
        // ============================================================================

        #region CanAnomalyDetectionLogic (Aggregate Root)

        builder.Entity<CanAnomalyDetectionLogic>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "CanAnomalyDetectionLogics", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            // Value Object: DetectionLogicIdentity (with nested Version and OemCode)
            b.OwnsOne(x => x.Identity, identity =>
            {
                identity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("Name");

                // Nested Value Object: DetectionLogicVersion
                identity.OwnsOne(x => x.Version, version =>
                {
                    version.Property(x => x.Major).HasColumnName("VersionMajor");
                    version.Property(x => x.Minor).HasColumnName("VersionMinor");
                    version.Property(x => x.Patch).HasColumnName("VersionPatch");
                });

                // Nested Value Object: OemCode
                identity.OwnsOne(x => x.OemCode, oemCode =>
                {
                    oemCode.Property(x => x.Code)
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnName("OemCode");

                    oemCode.Property(x => x.Name)
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnName("OemName");
                });
            });

            // Value Object: DetectionLogicSpecification
            b.OwnsOne(x => x.Specification, spec =>
            {
                spec.Property(x => x.DetectionType).HasColumnName("DetectionType");
                spec.Property(x => x.Description)
                    .IsRequired()
                    .HasMaxLength(1000)
                    .HasColumnName("Description");
                spec.Property(x => x.TargetSystemType).HasColumnName("TargetSystemType");
                spec.Property(x => x.Complexity).HasColumnName("Complexity");
                spec.Property(x => x.Requirements).HasMaxLength(2000).HasColumnName("Requirements");
            });

            // Value Object: LogicImplementation
            b.OwnsOne(x => x.Implementation, impl =>
            {
                impl.Property(x => x.Type).HasColumnName("ImplementationType");
                impl.Property(x => x.Content).HasColumnName("ImplementationContent");
                impl.Property(x => x.Language).HasMaxLength(50).HasColumnName("ImplementationLanguage");
                impl.Property(x => x.EntryPoint).HasMaxLength(200).HasColumnName("ImplementationEntryPoint");
                impl.Property(x => x.CreatedAt).HasColumnName("ImplementationCreatedAt");
                impl.Property(x => x.CreatedBy).HasMaxLength(256).HasColumnName("ImplementationCreatedBy");
            });

            // Value Object: SafetyClassification
            b.OwnsOne(x => x.Safety, safety =>
            {
                safety.Property(x => x.AsilLevel).HasColumnName("AsilLevel");
                safety.Property(x => x.SafetyRequirementId).HasMaxLength(100).HasColumnName("SafetyRequirementId");
                safety.Property(x => x.SafetyGoalId).HasMaxLength(100).HasColumnName("SafetyGoalId");
                safety.Property(x => x.HazardAnalysisId).HasMaxLength(100).HasColumnName("HazardAnalysisId");
            });

            // Properties
            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.SharingLevel).IsRequired();
            b.Property(x => x.ApprovalNotes).HasMaxLength(2000);

            // Relationships with child entities
            b.HasMany<DetectionParameter>()
                .WithOne()
                .HasForeignKey("DetectionLogicId")
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany<CanSignalMapping>()
                .WithOne()
                .HasForeignKey("DetectionLogicId")
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.SharingLevel);
        });

        #endregion

        #region DetectionParameter (Entity - Child of CanAnomalyDetectionLogic)

        builder.Entity<DetectionParameter>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "DetectionParameters", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            // Properties
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.DataType).IsRequired();
            b.Property(x => x.Value).HasMaxLength(1000);
            b.Property(x => x.DefaultValue).HasMaxLength(1000);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.Unit).HasMaxLength(20);

            // Value Object: ParameterConstraints (stored as JSON)
            b.Property(x => x.Constraints)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<ParameterConstraints>(v, (System.Text.Json.JsonSerializerOptions)null))
                .HasColumnName("Constraints");

            // Foreign Key (shadow property)
            b.Property<Guid>("DetectionLogicId");

            // Indexes
            b.HasIndex("DetectionLogicId");
        });

        #endregion

        #region CanSignalMapping (Entity - Child of CanAnomalyDetectionLogic)

        builder.Entity<CanSignalMapping>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "CanSignalMappings", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            // Properties
            b.Property(x => x.CanSignalId).IsRequired();
            b.Property(x => x.SignalRole).IsRequired().HasMaxLength(50);
            b.Property(x => x.Description).HasMaxLength(500);

            // Value Object: SignalMappingConfiguration (stored as JSON)
            b.Property(x => x.Configuration)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<SignalMappingConfiguration>(v, (System.Text.Json.JsonSerializerOptions)null))
                .HasColumnName("Configuration");

            // Foreign Key (shadow property)
            b.Property<Guid>("DetectionLogicId");

            // Indexes
            b.HasIndex("DetectionLogicId");
            b.HasIndex(x => x.CanSignalId);
        });

        #endregion

        // ============================================================================
        // 4. 異常検出結果管理
        // ============================================================================

        #region AnomalyDetectionResult (Aggregate Root)

        builder.Entity<AnomalyDetectionResult>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "AnomalyDetectionResults", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            // Value Object: DetectionInputData
            b.OwnsOne(x => x.InputData, input =>
            {
                input.Property(x => x.SignalValue).HasColumnName("InputSignalValue");
                input.Property(x => x.Timestamp).HasColumnName("InputTimestamp");
                input.Property(x => x.AdditionalData)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions)null))
                    .HasColumnName("InputAdditionalData");
            });

            // Value Object: DetectionDetails
            b.OwnsOne(x => x.Details, details =>
            {
                details.Property(x => x.DetectionType).HasColumnName("DetectionType");
                details.Property(x => x.TriggerCondition)
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasColumnName("TriggerCondition");
                details.Property(x => x.ExecutionTimeMs).HasColumnName("ExecutionTimeMs");
                details.Property(x => x.Parameters)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions)null))
                    .HasColumnName("DetectionParameters");
            });

            // Properties
            b.Property(x => x.DetectionLogicId).IsRequired();
            b.Property(x => x.CanSignalId).IsRequired();
            b.Property(x => x.DetectedAt).IsRequired();
            b.Property(x => x.AnomalyLevel).IsRequired();
            b.Property(x => x.ConfidenceScore).IsRequired();
            b.Property(x => x.Description).IsRequired().HasMaxLength(1000);
            b.Property(x => x.ResolutionStatus).IsRequired();
            b.Property(x => x.ResolutionNotes).HasMaxLength(2000);
            b.Property(x => x.SharingLevel).IsRequired();

            // Relationships
            b.HasOne<CanAnomalyDetectionLogic>()
                .WithMany()
                .HasForeignKey(x => x.DetectionLogicId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<CanSignal>()
                .WithMany()
                .HasForeignKey(x => x.CanSignalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            b.HasIndex(x => x.DetectedAt);
            b.HasIndex(x => x.AnomalyLevel);
            b.HasIndex(x => x.ResolutionStatus);
            b.HasIndex(x => x.DetectionLogicId);
            b.HasIndex(x => x.CanSignalId);
        });

        #endregion

        // ============================================================================
        // 5. プロジェクト管理
        // ============================================================================

        #region AnomalyDetectionProject (Aggregate Root)

        builder.Entity<AnomalyDetectionProject>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "AnomalyDetectionProjects", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            // Value Object: OemCode
            b.OwnsOne(x => x.OemCode, oemCode =>
            {
                oemCode.Property(x => x.Code)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("OemCode");

                oemCode.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("OemName");
            });

            // Value Object: ProjectConfiguration (with nested NotificationSettings)
            b.OwnsOne(x => x.Configuration, config =>
            {
                config.Property(x => x.Priority).HasColumnName("Priority");
                config.Property(x => x.IsConfidential).HasColumnName("IsConfidential");
                config.Property(x => x.Tags)
                    .HasConversion(
                        v => string.Join(",", v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                    .HasColumnName("Tags");
                config.Property(x => x.CustomSettings)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions)null))
                    .HasColumnName("CustomSettings");
                config.Property(x => x.Notes)
                    .HasConversion(
                        v => string.Join("|", v),
                        v => v.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList())
                    .HasColumnName("Notes");

                // Nested Value Object: NotificationSettings
                config.OwnsOne(x => x.NotificationSettings, notif =>
                {
                    notif.Property(x => x.EnableMilestoneNotifications).HasColumnName("EnableMilestoneNotifications");
                    notif.Property(x => x.EnableProgressNotifications).HasColumnName("EnableProgressNotifications");
                    notif.Property(x => x.EnableOverdueNotifications).HasColumnName("EnableOverdueNotifications");
                    notif.Property(x => x.NotificationFrequencyHours).HasColumnName("NotificationFrequencyHours");
                    notif.Property(x => x.NotificationChannels)
                        .HasConversion(
                            v => string.Join(",", v),
                            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                        .HasColumnName("NotificationChannels");
                });
            });

            // Properties
            b.Property(x => x.ProjectCode).IsRequired().HasMaxLength(20);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(2000);
            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.VehicleModel).IsRequired().HasMaxLength(100);
            b.Property(x => x.ModelYear).IsRequired().HasMaxLength(4);
            b.Property(x => x.PrimarySystem).IsRequired();
            b.Property(x => x.ProjectManagerId).IsRequired();

            // Relationships with child entities
            b.HasMany<ProjectMilestone>()
                .WithOne()
                .HasForeignKey("ProjectId")
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany<ProjectMember>()
                .WithOne()
                .HasForeignKey("ProjectId")
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            b.HasIndex(x => x.ProjectCode).IsUnique();
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.StartDate);
            b.HasIndex(x => x.EndDate);
        });

        #endregion

        #region ProjectMilestone (Entity - Child of AnomalyDetectionProject)

        builder.Entity<ProjectMilestone>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "ProjectMilestones", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            // Properties
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.DueDate).IsRequired();
            b.Property(x => x.Status).IsRequired();

            // Value Object: MilestoneConfiguration (stored as JSON)
            b.Property(x => x.Configuration)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<MilestoneConfiguration>(v, (System.Text.Json.JsonSerializerOptions)null))
                .HasColumnName("Configuration");

            // Foreign Key (shadow property)
            b.Property<Guid>("ProjectId");

            // Indexes
            b.HasIndex("ProjectId");
        });

        #endregion

        #region ProjectMember (Entity - Child of AnomalyDetectionProject)

        builder.Entity<ProjectMember>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "ProjectMembers", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            // Properties
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.Role).IsRequired();
            b.Property(x => x.Notes).HasMaxLength(500);

            // Value Object: MemberConfiguration (stored as JSON)
            b.Property(x => x.Configuration)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<MemberConfiguration>(v, (System.Text.Json.JsonSerializerOptions)null))
                .HasColumnName("Configuration");

            // Foreign Key (shadow property)
            b.Property<Guid>("ProjectId");

            // Indexes
            b.HasIndex("ProjectId");
            b.HasIndex(x => x.UserId);
        });

        #endregion

        // ============================================================================
        // 6. OEM Traceability (追加機能)
        // ============================================================================

        #region OemCustomization (Aggregate Root)

        builder.Entity<OemCustomization>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "OemCustomizations", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            // Value Object: OemCode
            b.OwnsOne(x => x.OemCode, oemCode =>
            {
                oemCode.Property(x => x.Code)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("OemCode");

                oemCode.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("OemName");
            });

            // Properties
            b.Property(x => x.EntityType).IsRequired().HasMaxLength(100);
            b.Property(x => x.EntityId).IsRequired();
            b.Property(x => x.Type).IsRequired();
            b.Property(x => x.CustomParameters)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions)null))
                .IsRequired();
            b.Property(x => x.OriginalParameters)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions)null))
                .IsRequired();
            b.Property(x => x.CustomizationReason).HasMaxLength(1000);
            b.Property(x => x.Status).IsRequired();

            // Indexes
            b.HasIndex(x => new { x.EntityType, x.EntityId });
            b.HasIndex(x => x.Status);
        });

        #endregion

        #region OemApproval (Aggregate Root)

        builder.Entity<OemApproval>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "OemApprovals", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            // Value Object: OemCode
            b.OwnsOne(x => x.OemCode, oemCode =>
            {
                oemCode.Property(x => x.Code)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("OemCode");

                oemCode.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("OemName");
            });

            // Properties
            b.Property(x => x.EntityType).IsRequired().HasMaxLength(100);
            b.Property(x => x.EntityId).IsRequired();
            b.Property(x => x.Type).IsRequired();
            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.ApprovalReason).HasMaxLength(1000);
            b.Property(x => x.ApprovalData)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions)null))
                .IsRequired();

            // Indexes
            b.HasIndex(x => new { x.EntityType, x.EntityId });
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.ApprovedAt);
        });

        #endregion
    }
}
