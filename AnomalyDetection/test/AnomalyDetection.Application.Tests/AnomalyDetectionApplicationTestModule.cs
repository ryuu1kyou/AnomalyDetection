using System;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.AuditLogging;
using AnomalyDetection.OemTraceability;
using AnomalyDetection.DetectionTemplates;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.AuditLogging;
using AnomalyDetection.Shared.Export;
using AnomalyDetection.OemTraceability.Services;
using AnomalyDetection.MultiTenancy;
using NSubstitute;
using System.Collections.Generic;
using Volo.Abp.Identity;
using Microsoft.AspNetCore.Http;
using Volo.Abp.Users;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Volo.Abp.Authorization.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Emailing;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Timing;
using Volo.Abp.Settings;
using Volo.Abp.Features;
using AnomalyDetection.CanSpecification;
using AnomalyDetection.CanSignals;
using AnomalyDetection.KnowledgeBase;
using AnomalyDetection.CanSignals.Mappers;
using AnomalyDetection.Services;
using Volo.Abp.AutoMapper;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.TenantManagement;

namespace AnomalyDetection;

[DependsOn(
    typeof(AnomalyDetectionApplicationModule),
    typeof(AnomalyDetectionDomainTestModule),
    typeof(AbpAutoMapperModule)
)]
public class AnomalyDetectionApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register mock repositories for testing
        context.Services.AddTransient(provider => Substitute.For<IOemCustomizationRepository>());
        context.Services.AddTransient(provider => Substitute.For<IOemApprovalRepository>());
        // context.Services.AddTransient(provider => Substitute.For<IAnomalyDetectionAuditLogRepository>()); // Removed duplicate

        // Fix: Add missing repositories causing DependencyResolutionException
        context.Services.AddTransient(provider =>
        {
            var repo = Substitute.For<IRepository<CompatibilityAnalysis, Guid>>();
            repo.GetListAsync(Arg.Any<Expression<Func<CompatibilityAnalysis, bool>>>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
                .Returns(Task.FromResult(new List<CompatibilityAnalysis>()));
            return repo;
        });
        context.Services.AddTransient(provider =>
        {
            var repo = Substitute.For<IRepository<CanSignal, Guid>>();
            repo.GetListAsync(Arg.Any<Expression<Func<CanSignal, bool>>>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
                .Returns(Task.FromResult(new List<CanSignal>()));
            repo.GetListAsync(Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
               .Returns(Task.FromResult(new List<CanSignal>()));
            // Fix: Add FindAsync/GetAsync default returns if needed, though null return for specific ID is standard for 'not found'.
            return repo;
        });
        context.Services.AddTransient(provider =>
        {
            var repo = Substitute.For<IRepository<KnowledgeArticle, Guid>>();
            repo.GetListAsync(Arg.Any<Expression<Func<KnowledgeArticle, bool>>>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
                .Returns(Task.FromResult(new List<KnowledgeArticle>()));
            return repo;
        });
        context.Services.AddTransient(provider =>
        {
            var repo = Substitute.For<IRepository<KnowledgeArticleComment, Guid>>();
            repo.GetListAsync(Arg.Any<Expression<Func<KnowledgeArticleComment, bool>>>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
               .Returns(Task.FromResult(new List<KnowledgeArticleComment>()));
            return repo;
        });
        context.Services.AddTransient(provider => Substitute.For<IRepository<CanSpecImport, Guid>>());

        // Fix: Ensure AnomalyDetectionResult repository returns empty list by default to prevent NullReferenceException
        // Register repositories
        context.Services.AddSingleton(provider =>
        {
            var repo = Substitute.For<IRepository<CanAnomalyDetectionLogic, Guid>>();
            repo.GetListAsync(Arg.Any<Expression<Func<CanAnomalyDetectionLogic, bool>>>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
                .Returns(Task.FromResult(new List<CanAnomalyDetectionLogic>()));
            return repo;
        });
        context.Services.AddSingleton(provider =>
        {
            var repo = Substitute.For<IRepository<AnomalyDetectionResult, Guid>>();
            repo.GetListAsync(Arg.Any<Expression<Func<AnomalyDetectionResult, bool>>>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
                .Returns(Task.FromResult(new List<AnomalyDetectionResult>()));
            return repo;
        });
        // context.Services.AddSingleton(provider => Substitute.For<IRepository<CanSignal, Guid>>()); // REMOVED DUPLICATE
        context.Services.AddSingleton(provider => Substitute.For<IAnomalyDetectionAuditLogRepository>());
        context.Services.AddSingleton(provider => Substitute.For<Volo.Abp.AuditLogging.IAuditLogRepository>()); // Required for AuditingStore
        context.Services.AddSingleton(provider => Substitute.For<IRepository<IdentityUser, Guid>>());

        // Register multi-tenancy repositories
        context.Services.AddSingleton(provider => Substitute.For<ITenantRepository>());
        context.Services.AddSingleton(provider => Substitute.For<IExtendedTenantRepository>());
        context.Services.AddSingleton(provider => Substitute.For<IOemMasterRepository>());

        // Register mock services for testing (using interfaces where possible)
        // context.Services.AddSingleton(provider => Substitute.For<IHttpContextAccessor>()); // Let real one work
        context.Services.AddSingleton(provider =>
        {
            var user = Substitute.For<ICurrentUser>();
            user.Id.Returns(Guid.NewGuid()); // Fix: Return a valid GUID
            user.IsAuthenticated.Returns(true);
            return user;
        });
        // context.Services.AddSingleton(provider => Substitute.For<ICurrentTenant>()); // Let ABP provide real one so Change() works
        context.Services.AddSingleton(provider => Substitute.For<IDataSeeder>());
        context.Services.AddSingleton(provider => Substitute.For<IUnitOfWorkManager>());
        context.Services.AddSingleton(provider => Substitute.For<IPermissionChecker>());
        context.Services.AddSingleton(provider => Substitute.For<IAuthorizationService>());
        context.Services.AddSingleton(provider => Substitute.For<IEmailSender>());
        context.Services.AddSingleton(provider => Substitute.For<IBackgroundJobManager>());
        // context.Services.AddSingleton(provider => Substitute.For<ITimeZoneConverter>());
        context.Services.AddSingleton(provider => Substitute.For<ISettingProvider>());
        context.Services.AddSingleton(provider => Substitute.For<IFeatureChecker>());
        context.Services.AddSingleton(provider => Substitute.For<ICanSpecificationParser>());
        context.Services.AddSingleton(provider => Substitute.For<ExportService>()); // ExportService mocks.AddAlwaysAllowAuthorization();

        // Concrete Service Implementations
        context.Services.AddTransient<CanSignalMapper>();
        context.Services.AddTransient<DbcParser>();
        context.Services.AddTransient<CompatibilityAnalyzer>();
        context.Services.AddTransient<CanSpecDiffService>();
        context.Services.AddTransient<ExportService>();
        context.Services.AddTransient<TraceabilityQueryService>();

        // Ensure auto-mapper and other configs
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AnomalyDetectionApplicationModule>();
        });

        context.Services.AddHttpContextAccessor();
        context.Services.AddAlwaysAllowAuthorization();
    }
}
