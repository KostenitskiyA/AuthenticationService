using Castle.DynamicProxy;

namespace Authentication.API.Extensions;

public static class InterceptorServiceExtensions
{
    private static readonly ProxyGenerator ProxyGenerator = new();
    
    public static IServiceCollection AddTransient<TInterface, TImplementation, TInterceptor>(
        this IServiceCollection services)
        where TImplementation : class, TInterface
        where TInterface : class
        where TInterceptor : class, IInterceptor
    {
        services.AddTransient<TInterceptor>();
        services.AddTransient<TImplementation>();
        
        services.AddTransient<TInterface>(provider =>
        {
            var target = provider.GetRequiredService<TImplementation>();
            var interceptor = provider.GetRequiredService<TInterceptor>();
            return ProxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(target, interceptor);
        });

        return services;
    }

    public static IServiceCollection AddScoped<TInterface, TImplementation, TInterceptor>(
        this IServiceCollection services)
        where TImplementation : class, TInterface
        where TInterface : class
        where TInterceptor : class, IInterceptor
    {
        services.AddScoped<TInterceptor>();
        services.AddScoped<TImplementation>();
        
        services.AddScoped<TInterface>(provider =>
        {
            var target = provider.GetRequiredService<TImplementation>();
            var interceptor = provider.GetRequiredService<TInterceptor>();
            return ProxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(target, interceptor);
        });

        return services;
    }

    public static IServiceCollection AddSingleton<TInterface, TImplementation, TInterceptor>(
        this IServiceCollection services)
        where TImplementation : class, TInterface
        where TInterface : class
        where TInterceptor : class, IInterceptor
    {
        services.AddSingleton<TInterceptor>();
        services.AddSingleton<TImplementation>();
        
        services.AddSingleton<TInterface>(provider =>
        {
            var target = provider.GetRequiredService<TImplementation>();
            var interceptor = provider.GetRequiredService<TInterceptor>();
            return ProxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(target, interceptor);
        });

        return services;
    }
}