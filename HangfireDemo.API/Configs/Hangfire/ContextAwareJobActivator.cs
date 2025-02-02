using Hangfire;
using Hangfire.Server;

namespace HangfireDemo.API.Configs.Hangfire;

public class ContextAwareJobActivator : JobActivator
{
    private readonly IServiceProvider _serviceProvider;

    public ContextAwareJobActivator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override JobActivatorScope BeginScope(PerformContext context)
    {
        if (context.Items.TryGetValue("HangfireScope", out var scopeObj) &&
            scopeObj is IServiceScope existingScope)
        {
            // Existing scope from filter - don't dispose
            return new ExistingDependencyScope(existingScope, shouldDispose: false);
        }

        // New fallback scope - mark for disposal
        var newScope = _serviceProvider.CreateScope();
        return new ExistingDependencyScope(newScope, shouldDispose: true);
    }

    private class ExistingDependencyScope : JobActivatorScope
    {
        private readonly IServiceScope _scope;
        private readonly bool _shouldDispose;

        public ExistingDependencyScope(IServiceScope scope, bool shouldDispose)
        {
            _scope = scope;
            _shouldDispose = shouldDispose;
        }

        public override object Resolve(Type type)
        {
            return _scope.ServiceProvider.GetRequiredService(type);
        }

        public override void DisposeScope()
        {
            if (_shouldDispose)
            {
                _scope.Dispose();
            }
        }
    }
}