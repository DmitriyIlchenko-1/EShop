using EShop.Infrastructure.Types;
using EShop.Tests.Framework;
using FluentAssertions;
using NUnit.Framework;

namespace EShop.Tests.Engine;

[TestFixture]
public class TypeScannerTests
{
    [Test]
    public void FindTypes_Benchmarks_Findings()
    {
        var scanner = new DefaultTypeScanner(new[] { typeof(ISomeInterface).Assembly });
        var type = scanner.FindClassesOfType<ISomeInterface>();
        type
            .Count()
            .ShouldEqual(1);
        typeof(ISomeInterface)
            .IsAssignableFrom(type
                .FirstOrDefault())
            .ShouldBeTrue();
    }

    public interface ISomeInterface
    {
    }

    public class SomeClass : ISomeInterface
    {
    }
}