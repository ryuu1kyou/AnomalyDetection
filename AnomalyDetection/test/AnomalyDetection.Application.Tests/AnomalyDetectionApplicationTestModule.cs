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
using System.Linq;
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
        // ===== REPOSITORY MOCKS =====
        // Configure repositories with consistent default behaviors

        context.Services.AddSingleton(provider => CreateRepositoryMock<CanSignal, Guid>());
        context.Services.AddSingleton(provider => CreateRepositoryMock<CanSpecImport, Guid>());
        context.Services.AddSingleton(provider => CreateRepositoryMock<KnowledgeArticle, Guid>());
        context.Services.AddSingleton(provider => CreateRepositoryMock<KnowledgeArticleComment, Guid>());
        context.Services.AddSingleton(provider => CreateRepositoryMock<CompatibilityAnalysis, Guid>());
        context.Services.AddSingleton(provider => CreateRepositoryMock<CanAnomalyDetectionLogic, Guid>());
        context.Services.AddSingleton(provider => CreateRepositoryMock<AnomalyDetectionResult, Guid>());

        // OEM Traceability repositories
        context.Services.AddSingleton(provider => CreateRepositoryMock<OemCustomization, Guid>());
        context.Services.AddSingleton(provider => CreateRepositoryMock<OemApproval, Guid>());
        context.Services.AddSingleton(provider => Substitute.For<IOemCustomizationRepository>());
        context.Services.AddSingleton(provider => Substitute.For<IOemApprovalRepository>());

        // Multi-tenancy repositories
        context.Services.AddSingleton(provider => CreateRepositoryMock<IdentityUser, Guid>());
        context.Services.AddSingleton(provider => Substitute.For<ITenantRepository>());
        context.Services.AddSingleton(provider => Substitute.For<IExtendedTenantRepository>());
        context.Services.AddSingleton(provider => Substitute.For<IOemMasterRepository>());

        // Audit log repositories
        context.Services.AddSingleton(provider => Substitute.For<IAnomalyDetectionAuditLogRepository>());
        context.Services.AddSingleton(provider => Substitute.For<IAuditLogRepository>());

        // ===== SERVICE MOCKS =====

        // Current user mock with valid ID
        context.Services.AddSingleton(provider =>
        {
            var user = Substitute.For<ICurrentUser>();
            user.Id.Returns(Guid.NewGuid());
            user.IsAuthenticated.Returns(true);
            user.UserName.Returns("test-user");
            return user;
        });

        // Other framework services
        context.Services.AddSingleton(provider => Substitute.For<IDataSeeder>());
        context.Services.AddSingleton(provider => Substitute.For<IPermissionChecker>());
        context.Services.AddSingleton(provider => Substitute.For<IAuthorizationService>());
        context.Services.AddSingleton(provider => Substitute.For<IEmailSender>());
        context.Services.AddSingleton(provider => Substitute.For<IBackgroundJobManager>());
        context.Services.AddSingleton(provider => Substitute.For<ISettingProvider>());
        context.Services.AddSingleton(provider => Substitute.For<IFeatureChecker>());

        // Parser mock
        context.Services.AddSingleton(provider => Substitute.For<ICanSpecificationParser>());

        // ===== CONCRETE SERVICE IMPLEMENTATIONS =====

        context.Services.AddTransient<CanSignalMapper>();
        context.Services.AddTransient<DbcParser>();
        context.Services.AddTransient<CompatibilityAnalyzer>();
        context.Services.AddTransient<CanSpecDiffService>();
        context.Services.AddTransient<ExportService>();
        context.Services.AddTransient<TraceabilityQueryService>();

        // ===== AUTOMAPPER CONFIGURATION =====

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AnomalyDetectionApplicationModule>();
            options.AddProfile<AnomalyDetectionApplicationAutoMapperProfile>();
        });

        // ===== HTTP CONTEXT & AUTHORIZATION =====

        context.Services.AddHttpContextAccessor();
        context.Services.AddAlwaysAllowAuthorization();
    }

    private static IRepository<TEntity, TKey> CreateRepositoryMock<TEntity, TKey>()
        where TEntity : class, Volo.Abp.Domain.Entities.IEntity<TKey>
        where TKey : notnull
    {
        var store = new Dictionary<TKey, TEntity>();
        var repo = Substitute.For<IRepository<TEntity, TKey>>();

        repo.GetListAsync(Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo => Task.FromResult(store.Values.ToList()));

        repo.GetListAsync(Arg.Any<Expression<Func<TEntity, bool>>>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var predicate = callInfo.Arg<Expression<Func<TEntity, bool>>>().Compile();
                return Task.FromResult(store.Values.Where(predicate).ToList());
            });

        repo.GetAsync(Arg.Any<TKey>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var id = callInfo.Arg<TKey>();
                if (store.TryGetValue(id, out var entity)) return Task.FromResult(entity);
                throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(TEntity), id);
            });

        repo.GetAsync(Arg.Any<TKey>(), cancellationToken: Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var id = callInfo.Arg<TKey>();
                if (store.TryGetValue(id, out var entity)) return Task.FromResult(entity);
                throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(TEntity), id);
            });

        repo.FirstOrDefaultAsync(Arg.Any<Expression<Func<TEntity, bool>>>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var predicate = callInfo.Arg<Expression<Func<TEntity, bool>>>().Compile();
                return Task.FromResult(store.Values.FirstOrDefault(predicate));
            });

        repo.InsertAsync(Arg.Any<TEntity>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var entity = callInfo.Arg<TEntity>();
                store[entity.Id] = entity;
                return Task.FromResult(entity);
            });

        repo.UpdateAsync(Arg.Any<TEntity>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var entity = callInfo.Arg<TEntity>();
                store[entity.Id] = entity;
                return Task.FromResult(entity);
            });

        repo.DeleteAsync(Arg.Any<TKey>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                store.Remove(callInfo.Arg<TKey>());
                return Task.CompletedTask;
            });

        repo.DeleteAsync(Arg.Any<TEntity>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                store.Remove(callInfo.Arg<TEntity>().Id);
                return Task.CompletedTask;
            });

        return repo;
    }
}
