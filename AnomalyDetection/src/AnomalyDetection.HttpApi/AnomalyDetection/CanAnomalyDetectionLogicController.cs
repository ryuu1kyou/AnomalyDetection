using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection.Dtos;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace AnomalyDetection.AnomalyDetection;

[Area("app")]
[RemoteService(Name = "Default")]
[Route("api/app/can-anomaly-detection-logics")]
public class CanAnomalyDetectionLogicController : AbpControllerBase, ICanAnomalyDetectionLogicAppService
{
    private readonly ICanAnomalyDetectionLogicAppService _detectionLogicAppService;

    public CanAnomalyDetectionLogicController(ICanAnomalyDetectionLogicAppService detectionLogicAppService)
    {
        _detectionLogicAppService = detectionLogicAppService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<CanAnomalyDetectionLogicDto>> GetListAsync(GetDetectionLogicsInput input)
    {
        return _detectionLogicAppService.GetListAsync(input);
    }

    [HttpGet]
    [Route("{id}")]
    public virtual Task<CanAnomalyDetectionLogicDto> GetAsync(Guid id)
    {
        return _detectionLogicAppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<CanAnomalyDetectionLogicDto> CreateAsync(CreateDetectionLogicDto input)
    {
        return _detectionLogicAppService.CreateAsync(input);
    }

    [HttpPut]
    [Route("{id}")]
    public virtual Task<CanAnomalyDetectionLogicDto> UpdateAsync(Guid id, UpdateDetectionLogicDto input)
    {
        return _detectionLogicAppService.UpdateAsync(id, input);
    }

    [HttpDelete]
    [Route("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return _detectionLogicAppService.DeleteAsync(id);
    }

    [HttpGet]
    [Route("{id}/can-delete")]
    public virtual Task<bool> CanDeleteAsync(Guid id)
    {
        return _detectionLogicAppService.CanDeleteAsync(id);
    }

    [HttpGet]
    [Route("by-detection-type/{detectionType}")]
    public virtual Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByDetectionTypeAsync(DetectionType detectionType)
    {
        return _detectionLogicAppService.GetByDetectionTypeAsync(detectionType);
    }

    [HttpGet]
    [Route("by-share-level/{sharingLevel}")]
    public virtual Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByShareLevelAsync(SharingLevel sharingLevel)
    {
        return _detectionLogicAppService.GetByShareLevelAsync(sharingLevel);
    }

    [HttpGet]
    [Route("by-asil-level/{asilLevel}")]
    public virtual Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByAsilLevelAsync(AsilLevel asilLevel)
    {
        return _detectionLogicAppService.GetByAsilLevelAsync(asilLevel);
    }

    [HttpPost]
    [Route("{id}/submit-for-approval")]
    public virtual Task SubmitForApprovalAsync(Guid id)
    {
        return _detectionLogicAppService.SubmitForApprovalAsync(id);
    }

    [HttpPost]
    [Route("{id}/approve")]
    public virtual Task ApproveAsync(Guid id, [FromQuery] string? notes = null)
    {
        return _detectionLogicAppService.ApproveAsync(id, notes);
    }

    [HttpPost]
    [Route("{id}/reject")]
    public virtual Task RejectAsync(Guid id, [FromBody] string reason)
    {
        return _detectionLogicAppService.RejectAsync(id, reason);
    }

    [HttpPost]
    [Route("{id}/deprecate")]
    public virtual Task DeprecateAsync(Guid id, [FromBody] string reason)
    {
        return _detectionLogicAppService.DeprecateAsync(id, reason);
    }

    [HttpPut]
    [Route("{id}/sharing-level")]
    public virtual Task UpdateSharingLevelAsync(Guid id, [FromBody] SharingLevel sharingLevel)
    {
        return _detectionLogicAppService.UpdateSharingLevelAsync(id, sharingLevel);
    }

    [HttpPost]
    [Route("{id}/test-execution")]
    public virtual Task<Dictionary<string, object>> TestExecutionAsync(Guid id, [FromBody] Dictionary<string, object> testData)
    {
        return _detectionLogicAppService.TestExecutionAsync(id, testData);
    }

    [HttpGet]
    [Route("{id}/validate-implementation")]
    public virtual Task<List<string>> ValidateImplementationAsync(Guid id)
    {
        return _detectionLogicAppService.ValidateImplementationAsync(id);
    }

    [HttpGet]
    [Route("{id}/execution-statistics")]
    public virtual Task<Dictionary<string, object>> GetExecutionStatisticsAsync(Guid id)
    {
        return _detectionLogicAppService.GetExecutionStatisticsAsync(id);
    }

    [HttpPost]
    [Route("{id}/clone")]
    public virtual Task<CanAnomalyDetectionLogicDto> CloneAsync(Guid id, [FromQuery] string newName)
    {
        return _detectionLogicAppService.CloneAsync(id, newName);
    }

    [HttpPost]
    [Route("create-from-template")]
    public virtual Task<CanAnomalyDetectionLogicDto> CreateFromTemplateAsync(
        [FromQuery] DetectionType detectionType,
        [FromBody] Dictionary<string, object> parameters)
    {
        return _detectionLogicAppService.CreateFromTemplateAsync(detectionType, parameters);
    }

    [HttpGet]
    [Route("templates")]
    public virtual Task<List<Dictionary<string, object>>> GetTemplatesAsync([FromQuery] DetectionType detectionType)
    {
        return _detectionLogicAppService.GetTemplatesAsync(detectionType);
    }

    [HttpGet]
    [Route("{id}/export")]
    public virtual Task<byte[]> ExportAsync(Guid id, [FromQuery] string format)
    {
        return _detectionLogicAppService.ExportAsync(id, format);
    }

    [HttpPost]
    [Route("import")]
    public virtual Task<CanAnomalyDetectionLogicDto> ImportAsync([FromBody] byte[] fileContent, [FromQuery] string fileName)
    {
        return _detectionLogicAppService.ImportAsync(fileContent, fileName);
    }

    [HttpGet]
    [Route("by-can-signal/{canSignalId}")]
    public virtual Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByCanSignalAsync(Guid canSignalId)
    {
        return _detectionLogicAppService.GetByCanSignalAsync(canSignalId);
    }

    [HttpGet]
    [Route("by-vehicle-phase/{vehiclePhaseId}")]
    public virtual Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByVehiclePhaseAsync(Guid vehiclePhaseId)
    {
        return _detectionLogicAppService.GetByVehiclePhaseAsync(vehiclePhaseId);
    }
}
