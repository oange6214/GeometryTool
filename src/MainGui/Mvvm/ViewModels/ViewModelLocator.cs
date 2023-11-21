using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MainGui.Mvvm.ViewModels;

internal class ViewModelLocator
{
    public ViewModelLocator()
    {
        ConfigureServices();
    }

    private IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // View models
        services.AddTransient<MainWindowViewModel>();

        var serviceProvider = services.BuildServiceProvider();
        Ioc.Default.ConfigureServices(serviceProvider);

        return serviceProvider;
    }

    public MainWindowViewModel? MainVM => Ioc.Default.GetService<MainWindowViewModel>();
}
