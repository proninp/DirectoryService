using System.Text.Json;
using Xunit;

namespace DirectoryService.Shared.UnitTests.Error;

public sealed class ErrorSerializationTests
{
    [Fact]
    public void Serialize_ProducesJsonObject()
    {
        var error = Shared.Error.Validation("code", "message", "field");

        var json = error.Serialize();

        Assert.StartsWith("{", json, StringComparison.InvariantCulture);
        Assert.EndsWith("}", json, StringComparison.InvariantCulture);
    }

    [Fact]
    public void Serialize_ErrorType_IsString_NotNumber()
    {
        var error = Shared.Error.Validation("code", "message", "field");

        var json = error.Serialize();

        Assert.Contains("\"Validation\"", json, StringComparison.InvariantCulture);
        Assert.DoesNotContain("\"ErrorType\":1", json, StringComparison.InvariantCulture);
    }

    [Fact]
    public void ToString_ReturnsSameAsSerialize()
    {
        var error = Shared.Error.NotFound("code", "message");

        Assert.Equal(error.Serialize(), error.ToString());
    }

    [Fact]
    public void Roundtrip_PreservesCode()
    {
        var original = Shared.Error.Validation("val.code", "message", "field");

        var restored = Shared.Error.Deserialize(original.Serialize());

        Assert.NotNull(restored);
        Assert.Equal("val.code", restored.ErrorMessage.Code);
    }

    [Fact]
    public void Roundtrip_PreservesMessage()
    {
        var original = Shared.Error.NotFound("not.found", "Record was not found");

        var restored = Shared.Error.Deserialize(original.Serialize());

        Assert.NotNull(restored);
        Assert.Equal("Record was not found", restored.ErrorMessage.Message);
    }

    [Fact]
    public void Roundtrip_PreservesErrorType()
    {
        var original = Shared.Error.Conflict("conflict", "Already exists");

        var restored = Shared.Error.Deserialize(original.Serialize());

        Assert.NotNull(restored);
        Assert.Equal(ErrorType.Conflict, restored.ErrorType);
    }

    [Fact]
    public void Roundtrip_NullInvalidField_PreservesNull()
    {
        var original = Shared.Error.NotFound("code", "message");

        var restored = Shared.Error.Deserialize(original.Serialize());

        Assert.NotNull(restored);
        Assert.Null(restored.ErrorMessage.InvalidField);
    }

    [Fact]
    public void Roundtrip_WithInvalidField_PreservesValue()
    {
        var original = Shared.Error.Validation("code", "message", "PostalCode");

        var restored = Shared.Error.Deserialize(original.Serialize());

        Assert.NotNull(restored);
        Assert.Equal("PostalCode", restored.ErrorMessage.InvalidField);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Failure)]
    [InlineData(ErrorType.None)]
    public void Serialize_ErrorType_IsSerializedAsEnumName(ErrorType errorType)
    {
        var json = JsonSerializer.Serialize(errorType);

        Assert.DoesNotMatch(@"^\d+$", json.Trim('"'));
    }

    [Fact]
    public void Deserialize_JsonNullLiteral_ReturnsNull()
    {
        var result = Shared.Error.Deserialize("null");

        Assert.Null(result);
    }

    [Fact]
    public void Failure_Roundtrip_PreservesErrorType()
    {
        var original = Shared.Error.Failure("failure.code", "Something went wrong");

        var restored = Shared.Error.Deserialize(original.Serialize());

        Assert.NotNull(restored);
        Assert.Equal(ErrorType.Failure, restored.ErrorType);
    }

    [Fact]
    public void None_Roundtrip_ProducesNoneErrorType()
    {
        var original = Shared.Error.None();

        var restored = Shared.Error.Deserialize(original.Serialize());

        Assert.NotNull(restored);
        Assert.Equal(ErrorType.None, restored.ErrorType);
    }
}