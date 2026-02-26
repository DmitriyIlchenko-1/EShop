namespace EShop.Tests.Framework;

public static class TestExtensions
{
    public static T ShouldEqual<T>(this T actual, object expected)
    {
        Assert.That(actual, Is.EqualTo(expected));
        return actual;
    }

    public static void ShouldBeTrue(this bool boolean)
    {
        Assert.That(boolean, Is.True);
    }
}