namespace UnityEngine.Terminal
{
    using Microsoft.Extensions.Logging;
    using System;
    public interface IDynamicCommand
    {
        void Create(Action<CommandHandlerContext> setup);
    }

    public class DynamicCommand : IDynamicCommand
    {
        private readonly ILogger<CommandHandlerContext> loggerForCtx;
        private readonly ILogger<IDynamicCommand> logger;
        private readonly CommandShell shell;

        public DynamicCommand(ILogger<CommandHandlerContext> loggerForCtx, ILogger<IDynamicCommand> logger, CommandShell shell)
        {
            this.loggerForCtx = loggerForCtx;
            this.logger = logger;
            this.shell = shell;
        }

        public void Create(Action<CommandHandlerContext> setup)
        {
            var ctx = new CommandHandlerContext(this.loggerForCtx);
            try
            {
                setup(ctx);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed dynamic registration command");
            }

            foreach (var data in ctx.GetCommands())
            {
                shell.AddDynamicCommand(data.Key, data.Value);
                logger.LogInformation("Success registered '{cmd}' dynamic command!", data.Key);
            }
        }
    }
}