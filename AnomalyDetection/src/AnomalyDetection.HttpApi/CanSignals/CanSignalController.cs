using System;
using System.Threading.Tasks;
using AnomalyDetection.CanSignals.Dtos;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace AnomalyDetection.CanSignals;

[Area("app")]
[RemoteService(Name = "Default")]
[Route("api/app/can-signals")]
public class CanSignalController : AbpControllerBase, ICanSignalAppService
{
    private readonly ICanSignalAppService _canSignalAppService;

    public CanSignalController(ICanSignalAppService canSignalAppService)
    {
        _canSignalAppService = canSignalAppService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<CanSignalDto>> GetListAsync(GetCanSignalsInput input)
    {
        return _canSignalAppService.GetListAsync(input);
    }

    [HttpGet]
    [Route("{id}")]
    public virtual Task<CanSignalDto> GetAsync(Guid id)
    {
        return _canSignalAppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<CanSignalDto> CreateAsync(CreateCanSignalDto input)
    {
        return _canSignalAppService.CreateAsync(input);
    }

    [HttpPut]
    [Route("{id}")]
    public virtual Task<CanSignalDto> UpdateAsync(Guid id, UpdateCanSignalDto input)
    {
        return _canSignalAppService.UpdateAsync(id, input);
    }

    [HttpDelete]
    [Route("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return _canSignalAppService.DeleteAsync(id);
    }

    [HttpGet]
    [Route("{id}/can-delete")]
    public virtual Task<bool> CanDeleteAsync(Guid id)
    {
        return _canSignalAppService.CanDeleteAsync(id);
    }

    [HttpGet]
    [Route("by-system-type/{systemType}")]
    public virtual Task<ListResultDto<CanSignalDto>> GetBySystemTypeAsync(CanSystemType systemType)
    {
        return _canSignalAppService.GetBySystemTypeAsync(systemType);
    }

    [HttpGet]
    [Route("by-oem-code/{oemCode}")]
    public virtual Task<ListResultDto<CanSignalDto>> GetByOemCodeAsync(string oemCode)
    {
        return _canSignalAppService.GetByOemCodeAsync(oemCode);
    }

    [HttpGet]
    [Route("search")]
    public virtual Task<ListResultDto<CanSignalDto>> SearchAsync([FromQuery] string searchTerm)
    {
        return _canSignalAppService.SearchAsync(searchTerm);
    }

    [HttpGet]
    [Route("check-can-id-conflicts")]
    public virtual Task<ListResultDto<CanSignalDto>> CheckCanIdConflictsAsync(
        [FromQuery] string canId,
        [FromQuery] Guid? excludeId = null)
    {
        return _canSignalAppService.CheckCanIdConflictsAsync(canId, excludeId);
    }

    [HttpGet]
    [Route("{signalId}/compatible-signals")]
    public virtual Task<ListResultDto<CanSignalDto>> GetCompatibleSignalsAsync(Guid signalId)
    {
        return _canSignalAppService.GetCompatibleSignalsAsync(signalId);
    }

    [HttpPost]
    [Route("{id}/mark-as-standard")]
    public virtual Task MarkAsStandardAsync(Guid id)
    {
        return _canSignalAppService.MarkAsStandardAsync(id);
    }

    [HttpPost]
    [Route("{id}/remove-standard-status")]
    public virtual Task RemoveStandardStatusAsync(Guid id)
    {
        return _canSignalAppService.RemoveStandardStatusAsync(id);
    }

    [HttpPost]
    [Route("{id}/activate")]
    public virtual Task ActivateAsync(Guid id)
    {
        return _canSignalAppService.ActivateAsync(id);
    }

    [HttpPost]
    [Route("{id}/deactivate")]
    public virtual Task DeactivateAsync(Guid id)
    {
        return _canSignalAppService.DeactivateAsync(id);
    }

    [HttpPost]
    [Route("{id}/deprecate")]
    public virtual Task DeprecateAsync(Guid id, [FromBody] string reason)
    {
        return _canSignalAppService.DeprecateAsync(id, reason);
    }

    [HttpGet]
    [Route("{id}/convert-raw-to-physical")]
    public virtual Task<double> ConvertRawToPhysicalAsync(Guid id, [FromQuery] double rawValue)
    {
        return _canSignalAppService.ConvertRawToPhysicalAsync(id, rawValue);
    }

    [HttpGet]
    [Route("{id}/convert-physical-to-raw")]
    public virtual Task<double> ConvertPhysicalToRawAsync(Guid id, [FromQuery] double physicalValue)
    {
        return _canSignalAppService.ConvertPhysicalToRawAsync(id, physicalValue);
    }

    [HttpPost]
    [Route("import")]
    public virtual Task<ListResultDto<CanSignalDto>> ImportFromFileAsync([FromBody] byte[] fileContent, [FromQuery] string fileName)
    {
        return _canSignalAppService.ImportFromFileAsync(fileContent, fileName);
    }

    [HttpGet]
    [Route("export")]
    public virtual Task<byte[]> ExportToFileAsync([FromQuery] GetCanSignalsInput input, [FromQuery] string format)
    {
        return _canSignalAppService.ExportToFileAsync(input, format);
    }
}
