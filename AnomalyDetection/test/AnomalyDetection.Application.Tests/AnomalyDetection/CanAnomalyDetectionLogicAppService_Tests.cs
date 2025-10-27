using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.AnomalyDetection.Dtos;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Xunit;

namespace AnomalyDetection.AnomalyDetection;

public class CanAnomalyDetectionLogicAppService_Tests : AnomalyDetectionApplicationTestBase<AnomalyDetectionApplicationTestModule>
{
    private readonly ICanAnomalyDetectionLogicAppService _detectionLogicAppService;

    public CanAnomalyDetectionLogicAppService_Tests()
    {
        _detectionLogicAppService = GetRequiredService<ICanAnomalyDetectionLogicAppService>();
    }

    [Fact]
    public async Task GetListAsync_Should_Return_Paged_Results()
    {
        // Arrange
        var input = new GetDetectionLogicsInput
        {
            MaxResultCount = 10,
            SkipCount = 0
        };

        // Act
        var result = await _detectionLogicAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<PagedResultDto<CanAnomalyDetectionLogicDto>>();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(0);
        result.Items.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetListAsync_With_Filter_Should_Return_Filtered_Results()
    {
        // Arrange
        var input = new GetDetectionLogicsInput
        {
            Filter = "Range",
            MaxResultCount = 10,
            SkipCount = 0
        };

        // Act
        var result = await _detectionLogicAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateAsync_Should_Create_New_DetectionLogic()
    {
        // Arrange
        var input = new CreateDetectionLogicDto
        {
            Name = "TestLogic_" + Guid.NewGuid().ToString("N")[..8],
            Description = "Test detection logic for unit testing",
            DetectionType = DetectionType.OutOfRange,
            SharingLevel = SharingLevel.Private,
            AsilLevel = AsilLevel.QM,
            LogicContent = "{ \"type\": \"range\", \"min\": 0, \"max\": 100 }",
            Parameters = new List<CreateDetectionParameterDto>
            {
                new CreateDetectionParameterDto
                {
                    Name = "MinThreshold",
                    DataType = ParameterDataType.Double,
                    DefaultValue = "0.0",
                    Description = "Minimum threshold value",
                    IsRequired = true
                },
                new CreateDetectionParameterDto
                {
                    Name = "MaxThreshold",
                    DataType = ParameterDataType.Double,
                    DefaultValue = "100.0",
                    Description = "Maximum threshold value",
                    IsRequired = true
                }
            }
        };

        // Act
        var result = await _detectionLogicAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe(input.Name);
        result.Description.ShouldBe(input.Description);
        result.DetectionType.ShouldBe(input.DetectionType);
        result.Status.ShouldBe(DetectionLogicStatus.Draft);
    }

    [Fact]
    public async Task GetAsync_Should_Return_Existing_DetectionLogic()
    {
        // Arrange - Create a logic first
        var createInput = new CreateDetectionLogicDto
        {
            Name = "GetTestLogic",
            Description = "Logic for get test",
            DetectionType = DetectionType.OutOfRange,
            SharingLevel = SharingLevel.Private,
            AsilLevel = AsilLevel.QM,
            LogicContent = "{ \"type\": \"range\", \"min\": 0, \"max\": 100 }",
            Parameters = new List<CreateDetectionParameterDto>()
        };

        var created = await _detectionLogicAppService.CreateAsync(createInput);

        // Act
        var result = await _detectionLogicAppService.GetAsync(created.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(created.Id);
        result.Name.ShouldBe(createInput.Name);
        result.Description.ShouldBe(createInput.Description);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Existing_DetectionLogic()
    {
        // Arrange - Create a logic first
        var createInput = new CreateDetectionLogicDto
        {
            Name = "UpdateTestLogic",
            Description = "Logic for update test",
            DetectionType = DetectionType.OutOfRange,
            SharingLevel = SharingLevel.Private,
            AsilLevel = AsilLevel.QM,
            LogicContent = "{ \"type\": \"range\", \"min\": 0, \"max\": 100 }",
            Parameters = new List<CreateDetectionParameterDto>()
        };

        var created = await _detectionLogicAppService.CreateAsync(createInput);

        var updateInput = new UpdateDetectionLogicDto
        {
            Name = "UpdatedLogicName",
            Description = "Updated description",
            DetectionType = DetectionType.RateOfChange,
            SharingLevel = SharingLevel.OemPartner,
            LogicContent = "{ \"type\": \"rate\", \"threshold\": 10 }",
            Parameters = new List<UpdateDetectionParameterDto>()
        };

        // Act
        var result = await _detectionLogicAppService.UpdateAsync(created.Id, updateInput);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(created.Id);
        result.Name.ShouldBe(updateInput.Name);
        result.Description.ShouldBe(updateInput.Description);
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Existing_DetectionLogic()
    {
        // Arrange - Create a logic first
        var createInput = new CreateDetectionLogicDto
        {
            Name = "DeleteTestLogic",
            Description = "Logic for delete test",
            DetectionType = DetectionType.OutOfRange,
            SharingLevel = SharingLevel.Private,
            AsilLevel = AsilLevel.QM,
            LogicContent = "{ \"type\": \"range\", \"min\": 0, \"max\": 100 }",
            Parameters = new List<CreateDetectionParameterDto>()
        };

        var created = await _detectionLogicAppService.CreateAsync(createInput);

        // Act
        await _detectionLogicAppService.DeleteAsync(created.Id);

        // Assert - Try to get the deleted logic should throw exception
        await Should.ThrowAsync<Exception>(async () =>
        {
            await _detectionLogicAppService.GetAsync(created.Id);
        });
    }

    [Fact]
    public async Task GetByDetectionTypeAsync_Should_Return_Logics_Of_Specified_Type()
    {
        // Arrange
        var detectionType = DetectionType.OutOfRange;

        // Act
        var result = await _detectionLogicAppService.GetByDetectionTypeAsync(detectionType);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        
        // All returned items should have the specified detection type
        foreach (var item in result.Items)
        {
            item.DetectionType.ShouldBe(detectionType);
        }
    }

    [Fact]
    public async Task GetByShareLevelAsync_Should_Return_Logics_Of_Specified_Level()
    {
        // Arrange
        var sharingLevel = SharingLevel.Private;

        // Act
        var result = await _detectionLogicAppService.GetByShareLevelAsync(sharingLevel);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        
        // All returned items should have the specified sharing level
        foreach (var item in result.Items)
        {
            item.SharingLevel.ShouldBe(sharingLevel);
        }
    }

    [Fact]
    public async Task GetByAsilLevelAsync_Should_Return_Logics_Of_Specified_Level()
    {
        // Arrange
        var asilLevel = AsilLevel.QM;

        // Act
        var result = await _detectionLogicAppService.GetByAsilLevelAsync(asilLevel);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        
        // All returned items should have the specified ASIL level
        foreach (var item in result.Items)
        {
            item.AsilLevel.ShouldBe(asilLevel);
        }
    }

    [Fact]
    public async Task SubmitForApprovalAsync_Should_Change_Status_To_PendingApproval()
    {
        // Arrange - Create a logic first
        var createInput = new CreateDetectionLogicDto
        {
            Name = "ApprovalTestLogic",
            Description = "Logic for approval test",
            DetectionType = DetectionType.OutOfRange,
            SharingLevel = SharingLevel.Private,
            AsilLevel = AsilLevel.B, // Higher ASIL level requires approval
            LogicContent = "{ \"type\": \"range\", \"min\": 0, \"max\": 100 }",
            Parameters = new List<CreateDetectionParameterDto>()
        };

        var created = await _detectionLogicAppService.CreateAsync(createInput);

        // Act
        await _detectionLogicAppService.SubmitForApprovalAsync(created.Id);

        // Assert
        var updated = await _detectionLogicAppService.GetAsync(created.Id);
        updated.Status.ShouldBe(DetectionLogicStatus.PendingApproval);
    }

    [Fact]
    public async Task ApproveAsync_Should_Change_Status_To_Approved()
    {
        // Arrange - Create and submit a logic first
        var createInput = new CreateDetectionLogicDto
        {
            Name = "ApproveTestLogic",
            Description = "Logic for approve test",
            DetectionType = DetectionType.OutOfRange,
            SharingLevel = SharingLevel.Private,
            AsilLevel = AsilLevel.B,
            LogicContent = "{ \"type\": \"range\", \"min\": 0, \"max\": 100 }",
            Parameters = new List<CreateDetectionParameterDto>()
        };

        var created = await _detectionLogicAppService.CreateAsync(createInput);
        await _detectionLogicAppService.SubmitForApprovalAsync(created.Id);

        // Act
        await _detectionLogicAppService.ApproveAsync(created.Id, "Approved for testing");

        // Assert
        var updated = await _detectionLogicAppService.GetAsync(created.Id);
        updated.Status.ShouldBe(DetectionLogicStatus.Approved);
    }

    [Fact]
    public async Task RejectAsync_Should_Change_Status_To_Rejected()
    {
        // Arrange - Create and submit a logic first
        var createInput = new CreateDetectionLogicDto
        {
            Name = "RejectTestLogic",
            Description = "Logic for reject test",
            DetectionType = DetectionType.OutOfRange,
            SharingLevel = SharingLevel.Private,
            AsilLevel = AsilLevel.B,
            LogicContent = "{ \"type\": \"range\", \"min\": 0, \"max\": 100 }",
            Parameters = new List<CreateDetectionParameterDto>()
        };

        var created = await _detectionLogicAppService.CreateAsync(createInput);
        await _detectionLogicAppService.SubmitForApprovalAsync(created.Id);

        // Act
        await _detectionLogicAppService.RejectAsync(created.Id, "Rejected for testing");

        // Assert
        var updated = await _detectionLogicAppService.GetAsync(created.Id);
        updated.Status.ShouldBe(DetectionLogicStatus.Rejected);
    }

    [Fact]
    public async Task UpdateSharingLevelAsync_Should_Update_Sharing_Level()
    {
        // Arrange - Create a logic first
        var createInput = new CreateDetectionLogicDto
        {
            Name = "SharingTestLogic",
            Description = "Logic for sharing test",
            DetectionType = DetectionType.OutOfRange,
            SharingLevel = SharingLevel.Private,
            AsilLevel = AsilLevel.QM,
            LogicContent = "{ \"type\": \"range\", \"min\": 0, \"max\": 100 }",
            Parameters = new List<CreateDetectionParameterDto>()
        };

        var created = await _detectionLogicAppService.CreateAsync(createInput);
        var newSharingLevel = SharingLevel.Industry;

        // Act
        await _detectionLogicAppService.UpdateSharingLevelAsync(created.Id, newSharingLevel);

        // Assert
        var updated = await _detectionLogicAppService.GetAsync(created.Id);
        updated.SharingLevel.ShouldBe(newSharingLevel);
    }

    [Fact]
    public async Task TestExecutionAsync_Should_Return_Test_Results()
    {
        // Arrange - Create an approved logic first
        var createInput = new CreateDetectionLogicDto
        {
            Name = "ExecutionTestLogic",
            Description = "Logic for execution test",
            DetectionType = DetectionType.OutOfRange,
            SharingLevel = SharingLevel.Private,
            AsilLevel = AsilLevel.QM,
            LogicContent = "{ \"type\": \"range\", \"min\": 0, \"max\": 100 }",
            Parameters = new List<CreateDetectionParameterDto>()
        };

        var created = await _detectionLogicAppService.CreateAsync(createInput);
        
        var testData = new Dictionary<string, object>
        {
            ["inputValue"] = 50.0,
            ["timestamp"] = DateTime.UtcNow
        };

        // Act
        var result = await _detectionLogicAppService.TestExecutionAsync(created.Id, testData);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Dictionary<string, object>>();
    }

    [Fact]
    public async Task GetExecutionStatisticsAsync_Should_Return_Statistics()
    {
        // Arrange - Create a logic first
        var createInput = new CreateDetectionLogicDto
        {
            Name = "StatsTestLogic",
            Description = "Logic for statistics test",
            DetectionType = DetectionType.OutOfRange,
            SharingLevel = SharingLevel.Private,
            AsilLevel = AsilLevel.QM,
            LogicContent = "{ \"type\": \"range\", \"min\": 0, \"max\": 100 }",
            Parameters = new List<CreateDetectionParameterDto>()
        };

        var created = await _detectionLogicAppService.CreateAsync(createInput);

        // Act
        var result = await _detectionLogicAppService.GetExecutionStatisticsAsync(created.Id);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Dictionary<string, object>>();
        result.ShouldContainKey("ExecutionCount");
        result.ShouldContainKey("LastExecutedAt");
        result.ShouldContainKey("AverageExecutionTime");
    }

    [Fact]
    public async Task ValidateImplementationAsync_Should_Return_Validation_Results()
    {
        // Arrange - Create a logic first
        var createInput = new CreateDetectionLogicDto
        {
            Name = "ValidationTestLogic",
            Description = "Logic for validation test",
            DetectionType = DetectionType.OutOfRange,
            SharingLevel = SharingLevel.Private,
            AsilLevel = AsilLevel.QM,
            LogicContent = "{ \"type\": \"range\", \"min\": 0, \"max\": 100 }",
            Parameters = new List<CreateDetectionParameterDto>()
        };

        var created = await _detectionLogicAppService.CreateAsync(createInput);

        // Act
        var result = await _detectionLogicAppService.ValidateImplementationAsync(created.Id);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<List<string>>();
    }
}