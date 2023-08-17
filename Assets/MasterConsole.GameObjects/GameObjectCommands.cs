namespace UnityEngine.Terminal
{
    using Microsoft.Extensions.Logging;
    using System;
    using Object = UnityEngine.Object;

    public static class GameObjectCommandsEx
    {
        public static void UseGameObjectCommands(this ICommandBuilder builder) => builder.Use<GameObjectCommands>();
    }

    public sealed class GameObjectCommands : CommandSilo
    {
        private readonly ILogger<GameObjectCommands> logger;
        public static IGameObjectFormatter GameObjectFormatter = new DefaultGameObjectFormatter();

        public GameObjectCommands(ILogger<GameObjectCommands> logger) 
            => this.logger = logger;

        public override void Create(CommandHandlerContext ctx)
        {
            ctx.Command<GameObject[]>("go.query", (x) => {
                foreach (var o in x) logger.LogInformation(GameObjectFormatter.FormatGameObject(o));
            });
            ctx.Command<GameObject[]>("go.destroy", (x) => {
                foreach (var o in x) Object.Destroy(o);
            });
            ctx.Command<GameObject, bool>("go.active", (x, state) => x.SetActive(state));
            ctx.Command<GameObject, string>("go.message", (x, method) => x.SendMessage(method));
            ctx.Command<GameObject>("go.hierarchy", x => "not implemented");
            ctx.Command<GameObject, Type>("go.inject", (x, y) => x.AddComponent(y));
            ctx.Command<GameObject, Type>("go.eject", (x, y) => Object.Destroy(x.GetComponent(y)));


            ctx.Variable("graphics.vsync", () => QualitySettings.vSyncCount, x => QualitySettings.vSyncCount = x);
            ctx.Variable("screen.dpi", () => Screen.dpi);
        }
    }

    public class DefaultGameObjectFormatter : IGameObjectFormatter
    {
        public string FormatGameObject(GameObject go)
        {
            var isActive = go.activeInHierarchy;

            return $"{(isActive ? 'A' : 'D')} {{{go.scene.name}}} pos:({go.transform.position}) [{go.name}] L:{LayerMask.LayerToName(go.layer)} T:{go.tag}";
        }
    }
    public interface IGameObjectFormatter 
    {
        string FormatGameObject(GameObject go);
    }
}
