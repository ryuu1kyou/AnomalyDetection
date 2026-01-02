using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using AnomalyDetection.CanSignals.Dtos;
using AnomalyDetection.CanSpecification; // Required for ParseResult, CanSpecMessage
using Shouldly;
using Xunit;
using Volo.Abp.Domain.Repositories;
using NSubstitute;

namespace AnomalyDetection.CanSignals;

public class CanSignalAppService_Import_Tests : AnomalyDetectionApplicationTestBase<AnomalyDetectionApplicationTestModule>
{
    private readonly ICanSignalAppService _canSignalAppService;
    private readonly IRepository<CanSignal, Guid> _canSignalRepository;

    public CanSignalAppService_Import_Tests()
    {
        _canSignalAppService = GetRequiredService<ICanSignalAppService>();
        _canSignalRepository = GetRequiredService<IRepository<CanSignal, Guid>>();

        // Configure Default Return
        _canSignalRepository.GetListAsync(Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(Task.FromResult(new List<CanSignal>()));

        _canSignalRepository.GetListAsync(Arg.Any<Expression<Func<CanSignal, bool>>>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
             .Returns(Task.FromResult(new List<CanSignal>()));

        // Also Mock InsertAsync to return the item
        _canSignalRepository.InsertAsync(Arg.Any<CanSignal>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<CanSignal>()));

        // Configure Parser Mock
        var parser = GetRequiredService<ICanSpecificationParser>();

        var message = new CanSpecMessage(0x7B, "TestMsg", 8);
        var signal1 = new CanSpecSignal("TestSignal", 0, 8)
        {
            Factor = 1.0,
            Offset = 0.0,
            Min = 0,
            Max = 255,
            Unit = "unit"
        };
        var signal2 = new CanSpecSignal("Signal2", 8, 16)
        {
            Factor = 1.0,
            Offset = 0.0,
            Min = 0,
            Max = 65535,
            Unit = "unit"
        };
        message.Signals.Add(signal1);
        message.Signals.Add(signal2);

        var parseResult = new ParseResult();
        parseResult.Messages.Add(message);

        parser.Parse(Arg.Any<byte[]>(), Arg.Any<string>())
            .Returns(parseResult);
    }

    [Fact]
    public async Task ImportFromFileAsync_Should_Import_Signals_From_CSV()
    {
        // Arrange
        var csvContent = "123,TestMsg,8,TestSignal,0,8\n123,TestMsg,8,Signal2,8,16";
        var fileContent = Encoding.UTF8.GetBytes(csvContent);
        var fileName = "test.csv";

        // Act
        var result = await _canSignalAppService.ImportFromFileAsync(fileContent, fileName);

        // Assert
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(2);

        var signal1 = result.Items.FirstOrDefault(x => x.SignalName == "TestSignal");
        signal1.ShouldNotBeNull();
        signal1.CanId.ShouldBe("7B"); // Valid Hex ID

        var signal2 = result.Items.FirstOrDefault(x => x.SignalName == "Signal2");
        signal2.ShouldNotBeNull();
        signal2.CanId.ShouldBe("7B");
    }
}
