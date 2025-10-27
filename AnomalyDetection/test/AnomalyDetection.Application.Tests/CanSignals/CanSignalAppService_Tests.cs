using System;
using System.Threading.Tasks;
using AnomalyDetection.CanSignals;
using AnomalyDetection.CanSignals.Dtos;
using AnomalyDetection.MultiTenancy;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;
using Xunit;

namespace AnomalyDetection.CanSignals;

public class CanSignalAppService_Tests : AnomalyDetectionApplicationTestBase<AnomalyDetectionApplicationTestModule>
{
    private readonly ICanSignalAppService _canSignalAppService;

    public CanSignalAppService_Tests()
    {
        _canSignalAppService = GetRequiredService<ICanSignalAppService>();
    }

    [Fact]
    public async Task GetListAsync_Should_Return_Paged_Results()
    {
        // Arrange
        var input = new GetCanSignalsInput
        {
            MaxResultCount = 10,
            SkipCount = 0
        };

        // Act
        var result = await _canSignalAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<PagedResultDto<CanSignalDto>>();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(0);
        result.Items.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetListAsync_With_Filter_Should_Return_Filtered_Results()
    {
        // Arrange
        var input = new GetCanSignalsInput
        {
            Filter = "Engine",
            MaxResultCount = 10,
            SkipCount = 0
        };

        // Act
        var result = await _canSignalAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateAsync_Should_Create_New_CanSignal()
    {
        // Arrange
        var input = new CreateCanSignalDto
        {
            SignalName = "TestSignal_" + Guid.NewGuid().ToString("N")[..8],
            CanId = "123",
            Description = "Test signal for unit testing",
            SystemType = CanSystemType.Engine,
            StartBit = 0,
            Length = 8,
            DataType = SignalDataType.Unsigned,
            MinValue = 0,
            MaxValue = 255,
            Factor = 1.0,
            Offset = 0.0,
            Unit = "rpm",
            CycleTime = 100,
            TimeoutTime = 500,
            ByteOrder = SignalByteOrder.Motorola,
            OemCode = new OemCode("TEST", "Test OEM")
        };

        // Act
        var result = await _canSignalAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.SignalName.ShouldBe(input.SignalName);
        result.CanId.ShouldBe(input.CanId);
        result.SystemType.ShouldBe(input.SystemType);
    }

    [Fact]
    public async Task CreateAsync_With_Duplicate_CanId_Should_Throw_Exception()
    {
        // Arrange
        var canId = "DUPLICATE_" + Guid.NewGuid().ToString("N")[..6];
        
        var firstInput = new CreateCanSignalDto
        {
            SignalName = "FirstSignal",
            CanId = canId,
            Description = "First signal",
            SystemType = CanSystemType.Engine,
            StartBit = 0,
            Length = 8,
            DataType = SignalDataType.Unsigned,
            MinValue = 0,
            MaxValue = 255,
            Factor = 1.0,
            Offset = 0.0,
            Unit = "rpm",
            CycleTime = 100,
            TimeoutTime = 500,
            ByteOrder = SignalByteOrder.Motorola,
            OemCode = new OemCode("TEST", "Test OEM")
        };

        var secondInput = new CreateCanSignalDto
        {
            SignalName = "SecondSignal",
            CanId = canId, // Same CAN ID
            Description = "Second signal",
            SystemType = CanSystemType.Brake,
            StartBit = 8,
            Length = 8,
            DataType = SignalDataType.Unsigned,
            MinValue = 0,
            MaxValue = 255,
            Factor = 1.0,
            Offset = 0.0,
            Unit = "bar",
            CycleTime = 100,
            TimeoutTime = 500,
            ByteOrder = SignalByteOrder.Motorola,
            OemCode = new OemCode("TEST", "Test OEM")
        };

        // Act & Assert
        await _canSignalAppService.CreateAsync(firstInput);
        
        var exception = await Should.ThrowAsync<Exception>(async () =>
        {
            await _canSignalAppService.CreateAsync(secondInput);
        });
        
        exception.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetAsync_Should_Return_Existing_CanSignal()
    {
        // Arrange - Create a signal first
        var createInput = new CreateCanSignalDto
        {
            SignalName = "GetTestSignal",
            CanId = "GET123",
            Description = "Signal for get test",
            SystemType = CanSystemType.Engine,
            StartBit = 0,
            Length = 8,
            DataType = SignalDataType.Unsigned,
            MinValue = 0,
            MaxValue = 255,
            Factor = 1.0,
            Offset = 0.0,
            Unit = "rpm",
            CycleTime = 100,
            TimeoutTime = 500,
            ByteOrder = SignalByteOrder.Motorola,
            OemCode = new OemCode("TEST", "Test OEM")
        };

        var created = await _canSignalAppService.CreateAsync(createInput);

        // Act
        var result = await _canSignalAppService.GetAsync(created.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(created.Id);
        result.SignalName.ShouldBe(createInput.SignalName);
        result.CanId.ShouldBe(createInput.CanId);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Existing_CanSignal()
    {
        // Arrange - Create a signal first
        var createInput = new CreateCanSignalDto
        {
            SignalName = "UpdateTestSignal",
            CanId = "UPD123",
            Description = "Signal for update test",
            SystemType = CanSystemType.Engine,
            StartBit = 0,
            Length = 8,
            DataType = SignalDataType.Unsigned,
            MinValue = 0,
            MaxValue = 255,
            Factor = 1.0,
            Offset = 0.0,
            Unit = "rpm",
            CycleTime = 100,
            TimeoutTime = 500,
            ByteOrder = SignalByteOrder.Motorola,
            OemCode = new OemCode("TEST", "Test OEM")
        };

        var created = await _canSignalAppService.CreateAsync(createInput);

        var updateInput = new UpdateCanSignalDto
        {
            SignalName = "UpdatedSignalName",
            CanId = "UPD123",
            Description = "Updated description",
            SystemType = CanSystemType.Brake,
            StartBit = 0,
            Length = 16,
            DataType = SignalDataType.Signed,
            MinValue = -100,
            MaxValue = 100,
            Factor = 0.1,
            Offset = 10.0,
            Unit = "bar",
            CycleTime = 200,
            TimeoutTime = 1000,
            ByteOrder = SignalByteOrder.LittleEndian,
            IsStandard = true,
            ChangeReason = "Updated for testing"
        };

        // Act
        var result = await _canSignalAppService.UpdateAsync(created.Id, updateInput);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(created.Id);
        result.SignalName.ShouldBe(updateInput.SignalName);
        result.Description.ShouldBe(updateInput.Description);
        result.SystemType.ShouldBe(updateInput.SystemType);
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Existing_CanSignal()
    {
        // Arrange - Create a signal first
        var createInput = new CreateCanSignalDto
        {
            SignalName = "DeleteTestSignal",
            CanId = "DEL123",
            Description = "Signal for delete test",
            SystemType = CanSystemType.Engine,
            StartBit = 0,
            Length = 8,
            DataType = SignalDataType.Unsigned,
            MinValue = 0,
            MaxValue = 255,
            Factor = 1.0,
            Offset = 0.0,
            Unit = "rpm",
            CycleTime = 100,
            TimeoutTime = 500,
            ByteOrder = SignalByteOrder.Motorola,
            OemCode = new OemCode("TEST", "Test OEM")
        };

        var created = await _canSignalAppService.CreateAsync(createInput);

        // Act
        await _canSignalAppService.DeleteAsync(created.Id);

        // Assert - Try to get the deleted signal should throw exception
        await Should.ThrowAsync<Exception>(async () =>
        {
            await _canSignalAppService.GetAsync(created.Id);
        });
    }

    [Fact]
    public async Task GetBySystemTypeAsync_Should_Return_Signals_Of_Specified_Type()
    {
        // Arrange
        var systemType = CanSystemType.Engine;

        // Act
        var result = await _canSignalAppService.GetBySystemTypeAsync(systemType);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        
        // All returned items should have the specified system type
        foreach (var item in result.Items)
        {
            item.SystemType.ShouldBe(systemType);
        }
    }

    [Fact]
    public async Task SearchAsync_Should_Return_Matching_Signals()
    {
        // Arrange
        var searchTerm = "Engine";

        // Act
        var result = await _canSignalAppService.SearchAsync(searchTerm);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
    }

    [Fact]
    public async Task SearchAsync_With_Empty_Term_Should_Return_Empty_List()
    {
        // Act
        var result = await _canSignalAppService.SearchAsync("");

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        result.Items.Count.ShouldBe(0);
    }

    [Fact]
    public async Task CheckCanIdConflictsAsync_Should_Return_Conflicts()
    {
        // Arrange - Create a signal first
        var createInput = new CreateCanSignalDto
        {
            SignalName = "ConflictTestSignal",
            CanId = "CONF123",
            Description = "Signal for conflict test",
            SystemType = CanSystemType.Engine,
            StartBit = 0,
            Length = 8,
            DataType = SignalDataType.Unsigned,
            MinValue = 0,
            MaxValue = 255,
            Factor = 1.0,
            Offset = 0.0,
            Unit = "rpm",
            CycleTime = 100,
            TimeoutTime = 500,
            ByteOrder = SignalByteOrder.Motorola,
            OemCode = new OemCode("TEST", "Test OEM")
        };

        var created = await _canSignalAppService.CreateAsync(createInput);

        // Act
        var result = await _canSignalAppService.CheckCanIdConflictsAsync("CONF123");

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        result.Items.Count.ShouldBeGreaterThan(0);
        result.Items[0].CanId.ShouldBe("CONF123");
    }
}