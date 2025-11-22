using System;
using System.Text;
using AnomalyDetection.CanSpecification;
using Shouldly;
using Xunit;

namespace AnomalyDetection.Application.Tests.CanSpecification;

/// <summary>
/// Pure unit tests for CanSpecificationParser (no ABP dependencies)
/// </summary>
public class CanSpecificationParserTests
{
    #region CSV Parsing Tests

    [Fact]
    public void Parse_Should_Handle_Empty_CSV()
    {
        // Arrange
        var parser = new CanSpecificationParser();
        var csvContent = "MessageId,SignalName,StartBit,BitLength,IsSigned,IsBigEndian,Min,Max,Factor,Offset,Unit\n";
        var bytes = Encoding.UTF8.GetBytes(csvContent);

        // Act
        var result = parser.Parse(bytes, "CSV");

        // Assert
        result.ShouldNotBeNull();
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void Parse_Should_Parse_Valid_CSV_With_Single_Signal()
    {
        // Arrange
        var parser = new CanSpecificationParser();
        var csvContent = "MessageId,SignalName,StartBit,BitLength,IsSigned,IsBigEndian,Min,Max,Factor,Offset,Unit\n" +
                        "0x100,TestSignal,0,16,false,true,0,100,1,0,unit\n";
        var bytes = Encoding.UTF8.GetBytes(csvContent);

        // Act
        var result = parser.Parse(bytes, "CSV");

        // Assert
        result.ShouldNotBeNull();
        result.Messages.ShouldNotBeEmpty();
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].MessageId.ShouldBe((uint)0x100);
        result.Messages[0].Signals.Count.ShouldBe(1);
        result.Messages[0].Signals[0].Name.ShouldBe("TestSignal");
    }

    [Fact]
    public void Parse_Should_Group_Signals_By_MessageId()
    {
        // Arrange
        var parser = new CanSpecificationParser();
        var csvContent = "MessageId,SignalName,StartBit,BitLength,IsSigned,IsBigEndian,Min,Max,Factor,Offset,Unit\n" +
                        "0x100,Signal1,0,8,false,true,0,100,1,0,unit\n" +
                        "0x100,Signal2,8,8,false,true,0,100,1,0,unit\n" +
                        "0x200,Signal3,0,16,false,true,0,200,1,0,unit\n";
        var bytes = Encoding.UTF8.GetBytes(csvContent);

        // Act
        var result = parser.Parse(bytes, "CSV");

        // Assert
        result.Messages.Count.ShouldBe(2);
        result.Messages[0].Signals.Count.ShouldBe(2);
        result.Messages[1].Signals.Count.ShouldBe(1);
    }

    [Fact]
    public void Parse_Should_Handle_Hex_MessageId()
    {
        // Arrange
        var parser = new CanSpecificationParser();
        var csvContent = "MessageId,SignalName,StartBit,BitLength,IsSigned,IsBigEndian,Min,Max,Factor,Offset,Unit\n" +
                        "0x1AB,TestSignal,0,16,false,true,0,100,1,0,unit\n";
        var bytes = Encoding.UTF8.GetBytes(csvContent);

        // Act
        var result = parser.Parse(bytes, "CSV");

        // Assert
        result.Messages[0].MessageId.ShouldBe((uint)0x1AB);
    }

    [Fact]
    public void Parse_Should_Handle_Decimal_MessageId()
    {
        // Arrange
        var parser = new CanSpecificationParser();
        var csvContent = "MessageId,SignalName,StartBit,BitLength,IsSigned,IsBigEndian,Min,Max,Factor,Offset,Unit\n" +
                        "256,TestSignal,0,16,false,true,0,100,1,0,unit\n";
        var bytes = Encoding.UTF8.GetBytes(csvContent);

        // Act
        var result = parser.Parse(bytes, "CSV");

        // Assert
        result.Messages[0].MessageId.ShouldBe((uint)256);
    }

    [Fact]
    public void Parse_Should_Parse_Signal_Properties()
    {
        // Arrange
        var parser = new CanSpecificationParser();
        var csvContent = "MessageId,SignalName,StartBit,BitLength,IsSigned,IsBigEndian,Min,Max,Factor,Offset,Unit\n" +
                        "0x100,TestSignal,8,16,true,false,-100,100,0.5,10,km/h\n";
        var bytes = Encoding.UTF8.GetBytes(csvContent);

        // Act
        var result = parser.Parse(bytes, "CSV");

        // Assert
        var signal = result.Messages[0].Signals[0];
        signal.Name.ShouldBe("TestSignal");
        signal.StartBit.ShouldBe(8);
        signal.BitLength.ShouldBe(16);
        signal.IsSigned.ShouldBeTrue();
        signal.IsBigEndian.ShouldBeFalse();
        signal.Factor.ShouldBe(0.5);
        signal.Offset.ShouldBe(10);
        signal.Unit.ShouldBe("km/h");
    }

    [Fact]
    public void Parse_Should_Handle_UTF8_Encoding()
    {
        // Arrange
        var parser = new CanSpecificationParser();
        var csvContent = "MessageId,SignalName,StartBit,BitLength,IsSigned,IsBigEndian,Min,Max,Factor,Offset,Unit\n" +
                        "0x100,エンジン回転数,0,16,false,true,0,8000,0.25,0,rpm\n";
        var bytes = Encoding.UTF8.GetBytes(csvContent);

        // Act
        var result = parser.Parse(bytes, "CSV");

        // Assert
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].Signals[0].Name.ShouldBe("エンジン回転数");
    }

    #endregion

    #region JSON Parsing Tests

    [Fact]
    public void Parse_Should_Parse_Valid_JSON()
    {
        // Arrange
        var parser = new CanSpecificationParser();
        var jsonContent = @"{
            ""messages"": [
                {
                    ""messageId"": 256,
                    ""signals"": [
                        {
                            ""name"": ""TestSignal"",
                            ""startBit"": 0,
                            ""bitLength"": 16,
                            ""isSigned"": false,
                            ""isBigEndian"": true,
                            ""min"": 0,
                            ""max"": 100,
                            ""factor"": 1,
                            ""offset"": 0,
                            ""unit"": ""unit""
                        }
                    ]
                }
            ]
        }";
        var bytes = Encoding.UTF8.GetBytes(jsonContent);

        // Act
        var result = parser.Parse(bytes, "JSON");

        // Assert
        result.ShouldNotBeNull();
        result.Messages.ShouldNotBeEmpty();
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].MessageId.ShouldBe((uint)256);
        result.Messages[0].Signals.Count.ShouldBe(1);
    }

    [Fact]
    public void Parse_Should_Handle_Multiple_Messages_In_JSON()
    {
        // Arrange
        var parser = new CanSpecificationParser();
        var jsonContent = @"{
            ""messages"": [
                {
                    ""messageId"": 256,
                    ""signals"": [
                        {
                            ""name"": ""Signal1"",
                            ""startBit"": 0,
                            ""bitLength"": 8,
                            ""isSigned"": false,
                            ""isBigEndian"": true,
                            ""min"": 0,
                            ""max"": 100,
                            ""factor"": 1,
                            ""offset"": 0,
                            ""unit"": ""unit""
                        }
                    ]
                },
                {
                    ""messageId"": 512,
                    ""signals"": [
                        {
                            ""name"": ""Signal2"",
                            ""startBit"": 0,
                            ""bitLength"": 16,
                            ""isSigned"": false,
                            ""isBigEndian"": true,
                            ""min"": 0,
                            ""max"": 200,
                            ""factor"": 1,
                            ""offset"": 0,
                            ""unit"": ""unit""
                        }
                    ]
                }
            ]
        }";
        var bytes = Encoding.UTF8.GetBytes(jsonContent);

        // Act
        var result = parser.Parse(bytes, "JSON");

        // Assert
        result.Messages.Count.ShouldBe(2);
        result.Messages[0].MessageId.ShouldBe((uint)256);
        result.Messages[1].MessageId.ShouldBe((uint)512);
    }

    #endregion

    #region Format Detection Tests

    [Fact]
    public void Parse_Should_Detect_CSV_Format_Case_Insensitive()
    {
        // Arrange
        var parser = new CanSpecificationParser();
        var csvContent = "MessageId,SignalName,StartBit,BitLength,IsSigned,IsBigEndian,Min,Max,Factor,Offset,Unit\n" +
                        "0x100,Test,0,16,false,true,0,100,1,0,unit\n";
        var bytes = Encoding.UTF8.GetBytes(csvContent);

        // Act
        var result1 = parser.Parse(bytes, "csv");
        var result2 = parser.Parse(bytes, "CSV");
        var result3 = parser.Parse(bytes, "Csv");

        // Assert
        result1.Messages.ShouldNotBeEmpty();
        result2.Messages.ShouldNotBeEmpty();
        result3.Messages.ShouldNotBeEmpty();
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void Parse_Should_Handle_Large_MessageId()
    {
        // Arrange
        var parser = new CanSpecificationParser();
        var csvContent = "MessageId,SignalName,StartBit,BitLength,IsSigned,IsBigEndian,Min,Max,Factor,Offset,Unit\n" +
                        "0x7FFFFFFF,TestSignal,0,16,false,true,0,100,1,0,unit\n";
        var bytes = Encoding.UTF8.GetBytes(csvContent);

        // Act
        var result = parser.Parse(bytes, "CSV");

        // Assert
        result.Messages[0].MessageId.ShouldBe((uint)0x7FFFFFFF);
    }

    [Fact]
    public void Parse_Should_Handle_Special_Characters_In_SignalName()
    {
        // Arrange
        var parser = new CanSpecificationParser();
        var csvContent = "MessageId,SignalName,StartBit,BitLength,IsSigned,IsBigEndian,Min,Max,Factor,Offset,Unit\n" +
                        "0x100,Test_Signal-123,0,16,false,true,0,100,1,0,unit\n";
        var bytes = Encoding.UTF8.GetBytes(csvContent);

        // Act
        var result = parser.Parse(bytes, "CSV");

        // Assert
        result.Messages[0].Signals[0].Name.ShouldBe("Test_Signal-123");
    }

    [Fact]
    public void Parse_Should_Handle_Empty_Unit()
    {
        // Arrange
        var parser = new CanSpecificationParser();
        var csvContent = "MessageId,SignalName,StartBit,BitLength,IsSigned,IsBigEndian,Min,Max,Factor,Offset,Unit\n" +
                        "0x100,TestSignal,0,16,false,true,0,100,1,0,\n";
        var bytes = Encoding.UTF8.GetBytes(csvContent);

        // Act
        var result = parser.Parse(bytes, "CSV");

        // Assert
        var signal = result.Messages[0].Signals[0];
        signal.Unit.ShouldNotBeNull();
    }

    [Fact]
    public void Parse_Should_Handle_Whitespace_In_Fields()
    {
        // Arrange
        var parser = new CanSpecificationParser();
        var csvContent = "MessageId,SignalName,StartBit,BitLength,IsSigned,IsBigEndian,Min,Max,Factor,Offset,Unit\n" +
                        " 0x100 , TestSignal , 0 , 16 , false , true , 0 , 100 , 1 , 0 , unit \n";
        var bytes = Encoding.UTF8.GetBytes(csvContent);

        // Act
        var result = parser.Parse(bytes, "CSV");

        // Assert
        result.Messages.ShouldNotBeEmpty();
        result.Messages[0].Signals[0].Name.ShouldBe("TestSignal");
    }

    #endregion
}
