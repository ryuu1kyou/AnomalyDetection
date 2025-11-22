using System.Text;
using System.Threading.Tasks;
using System.Linq;
using AnomalyDetection.CanSignals;
using AnomalyDetection.CanSpecification;
using Shouldly;
using Xunit;

namespace AnomalyDetection.CanSignals;

public class CanSignalAppService_Import_Tests : AnomalyDetectionApplicationTestBase<AnomalyDetectionApplicationTestModule>
{
    private readonly ICanSignalAppService _canSignalAppService;
    private readonly ICanSpecificationParser _parser;

    public CanSignalAppService_Import_Tests()
    {
        _canSignalAppService = GetRequiredService<ICanSignalAppService>();
        _parser = GetRequiredService<ICanSpecificationParser>();
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
        signal1.CanId.ShouldBe("0x7B_TestSignal"); // 123 decimal is 7B hex

        var signal2 = result.Items.FirstOrDefault(x => x.SignalName == "Signal2");
        signal2.ShouldNotBeNull();
        signal2.CanId.ShouldBe("0x7B_Signal2");
    }
}
