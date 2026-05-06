using System.Reflection;
using FluentValidation;
using JewelryFactory.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace JewelryFactory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR — auto-register all handlers in this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Validation pipeline behavior — runs before every handler
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // FluentValidation — auto-register all validators
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
