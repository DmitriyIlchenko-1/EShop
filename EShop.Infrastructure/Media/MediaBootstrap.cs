using Autofac;
using EShop.Infrastructure.Media.Images;

namespace EShop.Infrastructure.Media;

public class MediaModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder
            .RegisterType<DefaultImageProcessor>()
            .As<IImageProcessor>()
            .SingleInstance();
        builder
            .RegisterType<ImageCache>()
            .As<IImageCache>()
            .SingleInstance();
        builder
            .RegisterType<DefaultMediaAccessor>()
            .As<IMediaAccessor>()
            .InstancePerDependency();
        builder
            .RegisterType<DefaultImageFactory>()
            .As<IImageFactory>()
            .SingleInstance();
    }
}