using FluentAssertions;
using Loca.Services.Venue.Commands;
using Loca.Services.Venue.Validators;

namespace Loca.Tests.Unit.Validators;

public class CheckInValidatorTests
{
    private readonly CheckInValidator _validator = new();

    [Fact]
    public void Should_Fail_When_QrPayloadEmpty()
    {
        var cmd = new CheckInCommand("", 40.4, 49.8, "device")
        {
            UserId = Guid.NewGuid()
        };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "QrPayload");
    }

    [Fact]
    public void Should_Fail_When_LatOutOfRange()
    {
        var cmd = new CheckInCommand("qr", 100, 49.8, "device")
        {
            UserId = Guid.NewGuid()
        };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_Fail_When_LngOutOfRange()
    {
        var cmd = new CheckInCommand("qr", 40.4, 200, "device")
        {
            UserId = Guid.NewGuid()
        };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_Pass_When_AllFieldsValid()
    {
        var cmd = new CheckInCommand("valid-qr-code", 40.4093, 49.8671, "device-fingerprint-123")
        {
            UserId = Guid.NewGuid()
        };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }
}
