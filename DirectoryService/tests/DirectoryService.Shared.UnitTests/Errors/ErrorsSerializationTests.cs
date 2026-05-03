using Xunit;

namespace DirectoryService.Shared.UnitTests.Errors;

public sealed class ErrorsSerializationTests
{
    [Fact]
    public void Serialize_ProducesJsonArray()
    {
        var errors = new Shared.Errors(Shared.Error.Validation(
            "code", "message", "field"));

        var json = errors.Serialize();

        Assert.StartsWith("[", json, StringComparison.InvariantCulture);
        Assert.EndsWith("]", json, StringComparison.InvariantCulture);
    }

    [Fact]
    public void Serialize_ErrorType_IsString_NotNumber()
    {
        var errors = new Shared.Errors(Shared.Error.Validation("code", "message", "field"));

        var json = errors.Serialize();

        Assert.Contains("\"Validation\"", json, StringComparison.InvariantCulture);
        Assert.DoesNotContain("\"ErrorType\":1", json, StringComparison.InvariantCulture);
    }

    [Fact]
    public void Roundtrip_PreservesCode()
    {
        var original = new Shared.Errors(Shared.Error.Validation("val.code", "message", "field"));

        var restored = Shared.Errors.Deserialize(original.Serialize());

        Assert.NotNull(restored);
        Assert.Equal("val.code", restored.Single().ErrorMessage.Code);
    }

    [Fact]
    public void Roundtrip_PreservesMessage()
    {
        var original = new Shared.Errors(Shared.Error.NotFound("not.found", "Record was not found"));

        var restored = Shared.Errors.Deserialize(original.Serialize());

        Assert.NotNull(restored);
        Assert.Equal("Record was not found", restored.Single().ErrorMessage.Message);
    }

    [Fact]
    public void Roundtrip_PreservesErrorType()
    {
        var original = new Shared.Errors(Shared.Error.Conflict("conflict", "Already exists"));

        var restored = Shared.Errors.Deserialize(original.Serialize());

        Assert.NotNull(restored);
        Assert.Equal(ErrorType.Conflict, restored.Single().ErrorType);
    }

    [Fact]
    public void Roundtrip_NullInvalidField_PreservesNull()
    {
        var original = new Shared.Errors(Shared.Error.NotFound("code", "message"));

        var restored = Shared.Errors.Deserialize(original.Serialize());

        Assert.NotNull(restored);
        Assert.Null(restored.Single().ErrorMessage.InvalidField);
    }

    [Fact]
    public void Roundtrip_WithInvalidField_PreservesValue()
    {
        var original = new Shared.Errors(Shared.Error.Validation("code", "message", "PostalCode"));

        var restored = Shared.Errors.Deserialize(original.Serialize());

        Assert.NotNull(restored);
        Assert.Equal("PostalCode", restored.Single().ErrorMessage.InvalidField);
    }

    [Fact]
    public void Roundtrip_MultipleErrors_PreservesCountAndTypes()
    {
        var original = new Shared.Errors([
            Shared.Error.Validation("val.code", "Validation failed", "Name"),
            Shared.Error.NotFound("not.found", "Not found"),
            Shared.Error.Conflict("conflict", "Conflict")
        ]);

        var restored = Shared.Errors.Deserialize(original.Serialize());

        Assert.NotNull(restored);
        var list = restored.ToList();
        Assert.Equal(3, list.Count);
        Assert.Equal(ErrorType.Validation, list[0].ErrorType);
        Assert.Equal(ErrorType.NotFound, list[1].ErrorType);
        Assert.Equal(ErrorType.Conflict, list[2].ErrorType);
    }

    [Fact]
    public void Deserialize_JsonNullLiteral_ReturnsNull()
    {
        var result = Shared.Errors.Deserialize("null");

        Assert.Null(result);
    }

    [Fact]
    public void Deserialize_EmptyArray_ReturnsEmptyErrors()
    {
        var result = Shared.Errors.Deserialize("[]");

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}