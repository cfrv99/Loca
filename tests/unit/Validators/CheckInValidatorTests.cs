using FluentAssertions;
using Loca.Services.Venue.Commands;
using Loca.Services.Venue.Validators;

namespace Loca.Tests.Unit.Validators;

public class CheckInValidatorTests
{
    private readonly CheckInValidator _validator = new();

    [Fact]
    public void Should_BeValid_When_AllFieldsProvided()
    {
        var cmd = new CheckInCommand("qr-payload", 40.4093, 49.8671, "device-123")
        {
            UserId = Guid.NewGuid()
        };

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Fail_When_QrPayloadIsEmpty()
    {
        var cmd = new CheckInCommand("", 40.4093, 49.8671, "device-123")
        {
            UserId = Guid.NewGuid()
        };

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "QrPayload");
    }

    [Fact]
    public void Should_Fail_When_LatitudeOutOfRange()
    {
        var cmd = new CheckInCommand("qr-payload", 91, 49.8671, "device-123")
        {
            UserId = Guid.NewGuid()
        };

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Lat");
    }

    [Fact]
    public void Should_Fail_When_LongitudeOutOfRange()
    {
        var cmd = new CheckInCommand("qr-payload", 40.4093, 181, "device-123")
        {
            UserId = Guid.NewGuid()
        };

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Lng");
    }

    [Fact]
    public void Should_Fail_When_UserIdIsEmpty()
    {
        var cmd = new CheckInCommand("qr-payload", 40.4093, 49.8671, "device-123")
        {
            UserId = Guid.Empty
        };

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Fail_When_DeviceFingerprintIsEmpty()
    {
        var cmd = new CheckInCommand("qr-payload", 40.4093, 49.8671, "")
        {
            UserId = Guid.NewGuid()
        };

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DeviceFingerprint");
    }
}
