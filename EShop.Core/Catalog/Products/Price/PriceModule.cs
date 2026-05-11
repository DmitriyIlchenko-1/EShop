using Autofac;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure.Utilities;

namespace EShop.Core.Catalog.Products.Price;

public class PriceModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var scanner = Singleton<ITypeScanner>.Instance;
        var calculators = scanner.FindClassesOfType<IPriceCalculator>(onlyConcreteClasses: true);
        foreach (var calculator in calculators)
        {
            builder
                .RegisterType(calculator)
                .As<IPriceCalculator>()
                .InstancePerDependency();
        }
        
    }
}