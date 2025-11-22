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
using AnomalyDetection.ValueObjects;
using AnomalyDetection.AuditLogging;
using AnomalyDetection.KnowledgeBase;
using AnomalyDetection.Safety;
using System.Text.Json;

namespace AnomalyDetection.EntityFrameworkCore;

public static class AnomalyDetectionDbContextModelCreatingExtensions
{
    public static void ConfigureAnomalyDetection(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        /* Configure your own tables/entities inside here */

        // Ignore owned types to prevent ConfigureByConvention from discovering them as entities
        // Multi-tenancy value objects
        builder.Ignore<OemFeature>();
        builder.Ignore<TenantFeature>();

        // CAN Signal value objects
        builder.Ignore<SignalIdentifier>();
        builder.Ignore<SignalSpecification>();
        builder.Ignore<PhysicalValueConversion>();
        builder.Ignore<SignalTiming>();
        builder.Ignore<SignalVersion>();
        builder.Ignore<SystemCategoryConfiguration>();

        // Detection Logic value objects
        builder.Ignore<DetectionLogicIdentity>();
        builder.Ignore<LogicVersion>();
        builder.Ignore<DetectionLogicSpecification>();
        builder.Ignore<LogicImplementation>();
        builder.Ignore<SafetyClassification>();
        builder.Ignore<ParameterConstraints>();
        builder.Ignore<SignalMappingConfiguration>();

        // Detection Result value objects
        builder.Ignore<DetectionInputData>();
        builder.Ignore<DetectionDetails>();

        // Project value objects
        builder.Ignore<ProjectConfiguration>();
        builder.Ignore<NotificationSettings>();
        builder.Ignore<MilestoneConfiguration>();
        builder.Ignore<MemberConfiguration>();

        // ============================================================================
        // 1. マルチテナント管理
        // ============================================================================

        #region OemMaster (Aggregate Root)

        builder.Entity<OemMaster>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "OemMasters", AnomalyDetectionConsts.DbSchema);

            // Owned Collection: Features (configure before ConfigureByConvention)
            b.OwnsMany(x => x.Features, features =>
            {
                features.ToTable(AnomalyDetectionConsts.DbTablePrefix + "OemFeatures", AnomalyDetectionConsts.DbSchema);
                features.WithOwner().HasForeignKey("OemMasterId");
                features.Property<Guid>("OemMasterId");
                features.HasKey("OemMasterId", "FeatureName");

                features.Property(x => x.FeatureName).IsRequired().HasMaxLength(100);
                features.Property(x => x.FeatureValue).IsRequired().HasMaxLength(500);
                features.Property(x => x.IsEnabled).IsRequired();
                features.Property(x => x.CreatedAt).IsRequired();
                features.Property(x => x.UpdatedAt);
            });

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
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.EstablishedDate);
        });

        #endregion

        #region ExtendedTenant (Aggregate Root)

        builder.Entity<ExtendedTenant>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "ExtendedTenants", AnomalyDetectionConsts.DbSchema);

            // Owned Collection: Features (configure before ConfigureByConvention)
            b.OwnsMany(x => x.Features, features =>
            {
                features.ToTable(AnomalyDetectionConsts.DbTablePrefix + "TenantFeatures", AnomalyDetectionConsts.DbSchema);
                features.WithOwner().HasForeignKey("ExtendedTenantId");
                features.Property<Guid>("ExtendedTenantId");
                features.HasKey("ExtendedTenantId", "FeatureName");

                features.Property(x => x.FeatureName).IsRequired().HasMaxLength(100);
                features.Property(x => x.FeatureValue).IsRequired().HasMaxLength(500);
                features.Property(x => x.IsEnabled).IsRequired();
                features.Property(x => x.CreatedAt).IsRequired();
                features.Property(x => x.UpdatedAt);
                features.Property(x => x.CreatedBy).HasMaxLength(256);
                features.Property(x => x.UpdatedBy).HasMaxLength(256);
            });

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
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.ActivationDate);
            b.Property(x => x.ExpirationDate);

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
            b.Property(x => x.IsStandard).IsRequired();
            b.Property(x => x.EffectiveDate);

            // Indexes - Performance Optimized
            b.HasIndex(x => x.SystemType);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.IsStandard);
            b.HasIndex(x => x.EffectiveDate);

            // Composite indexes for common query patterns
            b.HasIndex(x => new { x.SystemType, x.Status });
            b.HasIndex(x => new { x.IsStandard, x.Status });
            b.HasIndex(x => new { x.SystemType, x.IsStandard, x.Status });

            // Note: Indexes on owned type properties are created through their configuration above

            // Composite index for frequent filtering
            b.HasIndex(x => new { x.TenantId, x.SystemType, x.Status });
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
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>())
                    .HasColumnName("CustomSettings");
            });

            // Properties
            b.Property(x => x.SystemType).IsRequired();
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Description).IsRequired().HasMaxLength(1000);
            b.Property(x => x.Icon).IsRequired().HasMaxLength(100);
            b.Property(x => x.Color).IsRequired().HasMaxLength(20);
            b.Property(x => x.DisplayOrder).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.SignalCount).IsRequired();
            b.Property(x => x.ActiveSignalCount).IsRequired();
            b.Property(x => x.LastSignalUpdate);

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
            b.Property(x => x.SourceLogicId);
            b.Property(x => x.VehiclePhaseId);
            b.Property(x => x.ApprovedAt);
            b.Property(x => x.ApprovedBy);
            b.Property(x => x.ApprovalNotes).HasMaxLength(2000);
            b.Property(x => x.ExecutionCount).IsRequired();
            b.Property(x => x.LastExecutedAt);
            b.Property(x => x.LastExecutionTimeMs);

            // Relationships with child entities
            b.HasMany<DetectionParameter>()
                .WithOne()
                .HasForeignKey("DetectionLogicId")
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany<CanSignalMapping>()
                .WithOne()
                .HasForeignKey("DetectionLogicId")
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes - Performance Optimized
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.SharingLevel);
            b.HasIndex(x => x.SourceLogicId);
            b.HasIndex(x => x.VehiclePhaseId);
            b.HasIndex(x => x.ApprovedAt);

            // Composite indexes for common query patterns
            b.HasIndex(x => new { x.Status, x.SharingLevel });
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.SharingLevel });

            // Note: Indexes on owned type properties (Identity, Specification, Safety) 
            // are created through their respective OwnsOne configurations

            // Performance indexes for execution tracking
            b.HasIndex(x => x.ExecutionCount);
            b.HasIndex(x => x.LastExecutedAt);
            b.HasIndex(x => new { x.LastExecutedAt, x.ExecutionCount });
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
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<ParameterConstraints>(v, (System.Text.Json.JsonSerializerOptions?)null)!)
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

            // Foreign Key (shadow property)
            b.Property<Guid>("DetectionLogicId");

            // Composite Primary Key
            b.HasKey("DetectionLogicId", nameof(CanSignalMapping.CanSignalId));

            // Properties
            b.Property(x => x.CanSignalId).IsRequired();
            b.Property(x => x.SignalRole).IsRequired().HasMaxLength(50);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.IsRequired).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt);

            // Value Object: SignalMappingConfiguration (stored as JSON)
            b.Property(x => x.Configuration)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<SignalMappingConfiguration>(v, (System.Text.Json.JsonSerializerOptions?)null)!)
                .HasColumnName("Configuration");

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
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null)!)
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
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null)!)
                    .HasColumnName("DetectionParameters");
            });

            // Properties
            b.Property(x => x.DetectionLogicId).IsRequired();
            b.Property(x => x.CanSignalId).IsRequired();
            b.Property(x => x.DetectedAt).IsRequired();
            b.Property(x => x.AnomalyLevel).IsRequired();
            b.Property(x => x.ConfidenceScore).IsRequired();
            b.Property(x => x.Description).IsRequired().HasMaxLength(1000);
            b.Property(x => x.DetectionDuration).IsRequired();
            b.Property(x => x.AnomalyType).IsRequired();
            b.Property(x => x.DetectionCondition).IsRequired().HasMaxLength(500);
            b.Property(x => x.IsValidated).IsRequired();
            b.Property(x => x.IsFalsePositiveFlag).IsRequired();
            b.Property(x => x.ValidationNotes).HasMaxLength(4000);
            b.Property(x => x.ResolutionStatus).IsRequired();
            b.Property(x => x.ResolvedAt);
            b.Property(x => x.ResolvedBy);
            b.Property(x => x.ResolutionNotes).HasMaxLength(2000);
            b.Property(x => x.SharingLevel).IsRequired();
            b.Property(x => x.IsShared).IsRequired();
            b.Property(x => x.SharedAt);
            b.Property(x => x.SharedBy);

            // Relationships
            b.HasOne<CanAnomalyDetectionLogic>()
                .WithMany()
                .HasForeignKey(x => x.DetectionLogicId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<CanSignal>()
                .WithMany()
                .HasForeignKey(x => x.CanSignalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes - Performance Optimized
            b.HasIndex(x => x.DetectedAt);
            b.HasIndex(x => x.AnomalyLevel);
            b.HasIndex(x => x.AnomalyType);
            b.HasIndex(x => x.IsValidated);
            b.HasIndex(x => x.IsFalsePositiveFlag);
            b.HasIndex(x => x.ResolutionStatus);
            b.HasIndex(x => x.ResolvedAt);
            b.HasIndex(x => x.DetectionLogicId);
            b.HasIndex(x => x.CanSignalId);
            b.HasIndex(x => x.IsShared);
            b.HasIndex(x => x.SharedAt);

            // Composite indexes for common query patterns
            b.HasIndex(x => new { x.DetectedAt, x.AnomalyLevel });
            b.HasIndex(x => new { x.CanSignalId, x.DetectedAt });
            b.HasIndex(x => new { x.DetectionLogicId, x.DetectedAt });
            b.HasIndex(x => new { x.AnomalyLevel, x.ResolutionStatus });
            b.HasIndex(x => new { x.TenantId, x.DetectedAt, x.AnomalyLevel });

            // Covering indexes for frequent queries
            b.HasIndex(x => new { x.CanSignalId, x.DetectedAt, x.AnomalyLevel, x.ConfidenceScore });
            b.HasIndex(x => new { x.DetectionLogicId, x.DetectedAt, x.AnomalyLevel, x.ResolutionStatus });

            // Indexes for analytics and reporting
            b.HasIndex(x => new { x.AnomalyType, x.DetectedAt });
            b.HasIndex(x => new { x.IsValidated, x.IsFalsePositiveFlag, x.DetectedAt });
        });

        #endregion

        // ============================================================================
        // 4b. Knowledge Base
        // ============================================================================

        #region KnowledgeArticle (Aggregate Root)

        builder.Entity<KnowledgeArticle>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "KnowledgeArticles", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.Content).IsRequired();
            b.Property(x => x.Summary).HasMaxLength(1000);
            b.Property(x => x.Symptom).HasMaxLength(1000);
            b.Property(x => x.Cause).HasMaxLength(1000);
            b.Property(x => x.Countermeasure).HasMaxLength(1000);
            b.Property(x => x.Category).IsRequired();
            b.Property(x => x.ViewCount).IsRequired();
            b.Property(x => x.UsefulCount).IsRequired();
            b.Property(x => x.IsPublished).IsRequired();
            b.Property(x => x.PublishedAt);
            b.Property(x => x.DetectionLogicId);
            b.Property(x => x.CanSignalId);
            b.Property(x => x.AnomalyType).HasMaxLength(200);
            b.Property(x => x.SignalName).HasMaxLength(200);
            b.Property(x => x.HasSolution).IsRequired();
            b.Property(x => x.SolutionSteps).HasMaxLength(2000);
            b.Property(x => x.PreventionMeasures).HasMaxLength(2000);
            b.Property(x => x.AverageRating).HasPrecision(5, 2);
            b.Property(x => x.RatingCount).IsRequired();

            b.Property(x => x.Tags)
                .HasConversion(
                    v => string.Join("|", v),
                    v => v.Split('|', StringSplitOptions.RemoveEmptyEntries)
                        .Select(tag => tag.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList())
                .HasColumnName("Tags");

            b.Property(x => x.Metadata)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>())
                .HasColumnName("Metadata");

            b.HasMany(x => x.Comments)
                .WithOne()
                .HasForeignKey(comment => comment.KnowledgeArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.IsPublished);
            b.HasIndex(x => x.Category);
            b.HasIndex(x => x.DetectionLogicId);
            b.HasIndex(x => x.CanSignalId);
            b.HasIndex(x => x.AnomalyType);
            b.HasIndex(x => x.SignalName);
        });

        #endregion

        #region KnowledgeArticleComment (Entity)

        builder.Entity<KnowledgeArticleComment>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "KnowledgeArticleComments", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.KnowledgeArticleId).IsRequired();
            b.Property(x => x.AuthorUserId);
            b.Property(x => x.AuthorName).HasMaxLength(200);
            b.Property(x => x.Content).IsRequired().HasMaxLength(2000);
            b.Property(x => x.Rating).IsRequired();

            b.HasIndex(x => x.KnowledgeArticleId);
            b.HasIndex(x => x.CreationTime);
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
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null)!)
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
            b.Property(x => x.StartDate).IsRequired();
            b.Property(x => x.EndDate);
            b.Property(x => x.ActualEndDate);
            b.Property(x => x.ProjectManagerId).IsRequired();
            b.Property(x => x.ProgressPercentage).IsRequired();
            b.Property(x => x.TotalTasks).IsRequired();
            b.Property(x => x.CompletedTasks).IsRequired();
            b.Property(x => x.LastProgressUpdate);

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
            b.HasIndex(x => x.ActualEndDate);
            b.HasIndex(x => x.ProjectManagerId);
            b.HasIndex(x => x.PrimarySystem);
        });

        #endregion

        #region ProjectMilestone (Entity - Child of AnomalyDetectionProject)

        builder.Entity<ProjectMilestone>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "ProjectMilestones", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            // Foreign Key (shadow property)
            b.Property<Guid>("ProjectId");

            // Composite Primary Key
            b.HasKey("ProjectId", nameof(ProjectMilestone.Name));

            // Properties
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.DueDate).IsRequired();
            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.CompletedDate);
            b.Property(x => x.CompletedBy);
            b.Property(x => x.DisplayOrder).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt);

            // Value Object: MilestoneConfiguration (stored as JSON)
            b.Property(x => x.Configuration)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<MilestoneConfiguration>(v, (System.Text.Json.JsonSerializerOptions?)null)!)
                .HasColumnName("Configuration");

            // Indexes
            b.HasIndex("ProjectId");
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.DueDate);
            b.HasIndex(x => x.CompletedDate);
            b.HasIndex(x => x.DisplayOrder);
        });

        #endregion

        #region ProjectMember (Entity - Child of AnomalyDetectionProject)

        builder.Entity<ProjectMember>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "ProjectMembers", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            // Foreign Key (shadow property)
            b.Property<Guid>("ProjectId");

            // Composite Primary Key
            b.HasKey("ProjectId", nameof(ProjectMember.UserId));

            // Properties
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.Role).IsRequired();
            b.Property(x => x.JoinedDate).IsRequired();
            b.Property(x => x.LeftDate);
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.Notes).HasMaxLength(500);

            // Value Object: MemberConfiguration (stored as JSON)
            b.Property(x => x.Configuration)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<MemberConfiguration>(v, (System.Text.Json.JsonSerializerOptions?)null)!)
                .HasColumnName("Configuration");

            // Indexes
            b.HasIndex("ProjectId");
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.Role);
            b.HasIndex(x => x.IsActive);
            b.HasIndex(x => x.JoinedDate);
            b.HasIndex(x => x.LeftDate);
        });

        #endregion

        // ============================================================================
        // 6. 安全トレーサビリティ管理
        // ============================================================================

        #region SafetyTraceRecord (Aggregate Root)

        builder.Entity<SafetyTraceRecord>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "SafetyTraceRecords", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.RequirementId).IsRequired().HasMaxLength(200);
            b.Property(x => x.SafetyGoalId).HasMaxLength(200);
            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(2000);
            b.Property(x => x.BaselineId).HasMaxLength(200);
            b.Property(x => x.Version).IsRequired();

            b.Property(x => x.RelatedDocuments)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
                .HasColumnName("RelatedDocumentsJson");

            b.Property(x => x.Verifications)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<VerificationRecord>>(v, (JsonSerializerOptions?)null) ?? new List<VerificationRecord>())
                .HasColumnName("VerificationsJson");

            b.Property(x => x.Validations)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<ValidationRecord>>(v, (JsonSerializerOptions?)null) ?? new List<ValidationRecord>())
                .HasColumnName("ValidationsJson");

            b.Property(x => x.AuditTrail)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<AuditEntry>>(v, (JsonSerializerOptions?)null) ?? new List<AuditEntry>())
                .HasColumnName("AuditTrailJson");

            b.Property(x => x.TraceabilityLinks)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<TraceabilityLinkRecord>>(v, (JsonSerializerOptions?)null) ?? new List<TraceabilityLinkRecord>())
                .HasColumnName("TraceabilityLinksJson");

            b.Property(x => x.LifecycleEvents)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<LifecycleEvent>>(v, (JsonSerializerOptions?)null) ?? new List<LifecycleEvent>())
                .HasColumnName("LifecycleEventsJson");

            b.Property(x => x.ChangeRequests)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<ChangeRequestRecord>>(v, (JsonSerializerOptions?)null) ?? new List<ChangeRequestRecord>())
                .HasColumnName("ChangeRequestsJson");

            b.HasIndex(x => x.AsilLevel);
            b.HasIndex(x => x.ApprovalStatus);
            b.HasIndex(x => x.ProjectId);
        });

        #endregion

        #region SafetyTraceLink (Aggregate Root) & History

        builder.Entity<SafetyTraceLink>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "SafetyTraceLinks", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.SourceRecordId).IsRequired();
            b.Property(x => x.TargetRecordId).IsRequired();
            b.Property(x => x.LinkType).IsRequired().HasMaxLength(100);
            b.Property(x => x.Relation).HasMaxLength(100);

            b.HasIndex(x => new { x.SourceRecordId, x.TargetRecordId }).IsUnique();
            b.HasIndex(x => x.LinkType);
            b.HasIndex(x => x.CreationTime);
        });

        builder.Entity<SafetyTraceLinkHistory>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "SafetyTraceLinkHistories", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.LinkId).IsRequired();
            b.Property(x => x.ChangeType).IsRequired().HasMaxLength(50);
            b.Property(x => x.OldLinkType).HasMaxLength(100);
            b.Property(x => x.NewLinkType).HasMaxLength(100);
            b.Property(x => x.Notes).HasMaxLength(1000);
            b.Property(x => x.ChangeTime).IsRequired();

            b.HasIndex(x => x.LinkId);
            b.HasIndex(x => x.ChangeType);
            b.HasIndex(x => x.ChangeTime);
        });

        #endregion

        // ============================================================================
        // 7. OEM Traceability (追加機能)
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
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null)!)
                .IsRequired();
            b.Property(x => x.OriginalParameters)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null)!)
                .IsRequired();
            b.Property(x => x.CustomizationReason).IsRequired().HasMaxLength(1000);
            b.Property(x => x.ApprovedBy);
            b.Property(x => x.ApprovedAt);
            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.ApprovalNotes).HasMaxLength(2000);

            // Indexes - Performance Optimized
            b.HasIndex(x => new { x.EntityType, x.EntityId });
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.ApprovedAt);
            b.HasIndex(x => x.Type);

            // Composite indexes for OEM traceability queries
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.HasIndex(x => new { x.EntityType, x.Status });
            // Note: Index on OemCode.Code is defined in the OemCode OwnsOne configuration
            b.HasIndex(x => new { x.Type, x.Status, x.ApprovedAt });

            // Covering index for customization listing
            b.HasIndex(x => new { x.TenantId, x.EntityType, x.Status, x.CreationTime });
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
            b.Property(x => x.RequestedBy).IsRequired();
            b.Property(x => x.RequestedAt).IsRequired();
            b.Property(x => x.ApprovedBy);
            b.Property(x => x.ApprovedAt);
            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.ApprovalReason).IsRequired().HasMaxLength(1000);
            b.Property(x => x.ApprovalNotes).HasMaxLength(2000);
            b.Property(x => x.ApprovalData)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null)!)
                .IsRequired();
            b.Property(x => x.DueDate);
            b.Property(x => x.Priority).IsRequired();

            // Indexes - Performance Optimized
            b.HasIndex(x => new { x.EntityType, x.EntityId });
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.RequestedAt);
            b.HasIndex(x => x.ApprovedAt);
            b.HasIndex(x => x.DueDate);
            b.HasIndex(x => x.Priority);
            b.HasIndex(x => x.Type);

            // Composite indexes for approval workflow queries
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.HasIndex(x => new { x.Status, x.DueDate });
            b.HasIndex(x => new { x.Status, x.Priority });
            // Note: Index on OemCode.Code is defined in the OemCode OwnsOne configuration
            b.HasIndex(x => new { x.Type, x.Status, x.RequestedAt });

            // Covering indexes for approval dashboard
            b.HasIndex(x => new { x.TenantId, x.Status, x.DueDate, x.Priority });
            b.HasIndex(x => new { x.RequestedBy, x.Status, x.RequestedAt });
        });

        #endregion

        // ============================================================================
        // 8. Audit Logging
        // ============================================================================

        #region AnomalyDetectionAuditLog (Aggregate Root)

        builder.Entity<AnomalyDetectionAuditLog>(b =>
        {
            b.ToTable(AnomalyDetectionConsts.DbTablePrefix + "AuditLogs", AnomalyDetectionConsts.DbSchema);
            b.ConfigureByConvention();

            // Properties
            b.Property(x => x.EntityId);
            b.Property(x => x.EntityType).IsRequired().HasMaxLength(100);
            b.Property(x => x.Action).IsRequired();
            b.Property(x => x.Level).IsRequired();
            b.Property(x => x.Description).IsRequired().HasMaxLength(2000);
            b.Property(x => x.OldValues).HasMaxLength(8000);
            b.Property(x => x.NewValues).HasMaxLength(8000);
            b.Property(x => x.Metadata)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null)!)
                .IsRequired();
            b.Property(x => x.IpAddress).HasMaxLength(45); // IPv6 support
            b.Property(x => x.UserAgent).HasMaxLength(500);
            b.Property(x => x.SessionId).HasMaxLength(100);
            b.Property(x => x.ExecutionDuration);
            b.Property(x => x.Exception).HasMaxLength(4000);

            // Indexes
            b.HasIndex(x => x.EntityId);
            b.HasIndex(x => x.EntityType);
            b.HasIndex(x => x.Action);
            b.HasIndex(x => x.Level);
            b.HasIndex(x => x.CreationTime);
            b.HasIndex(x => x.CreatorId);
            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.EntityType, x.EntityId });
            b.HasIndex(x => new { x.Action, x.CreationTime });
            b.HasIndex(x => new { x.Level, x.CreationTime });
        });

        #endregion
    }
}
