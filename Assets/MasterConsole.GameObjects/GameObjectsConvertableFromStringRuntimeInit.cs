namespace UnityEngine.Terminal
{
    // ReSharper disable InconsistentNaming

    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Unity.GoQL;
    using UnityEngine;

    public static class GameObjectsConvertableFromStringRuntimeInit
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            ConvertableFromString<GameObject>.Convertor = new Convertable_GameObject();
            ConvertableFromString<GameObject[]>.Convertor = new Convertable_GameObjectArray();
            ConvertableFromString<List<GameObject>>.Convertor = new Convertable_GameObjectList();
        }
    }

    public class Convertable_GameObject : IConvertableFromString<GameObject>
    {
        private GoQLExecutor _executor;

        public GameObject ConvertFromString(string str)
            => Execute(str).First();

        public GameObject[] Execute(string code)
        {
            this._executor ??= new GoQLExecutor();
            this._executor.Code = code;
            var objects = this._executor.Execute();

            return objects;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object IConvertableFromString.ConvertFromString(string str)
            => this.ConvertFromString(str);
    }

    public class Convertable_GameObjectArray : Convertable_GameObject, IConvertableFromString<GameObject[]>
    {
        public new GameObject[] ConvertFromString(string str)
            => Execute(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object IConvertableFromString.ConvertFromString(string str)
            => this.ConvertFromString(str);
    }

    public class Convertable_GameObjectList : Convertable_GameObjectArray, IConvertableFromString<List<GameObject>>
    {
        public new List<GameObject> ConvertFromString(string str)
            => Execute(str).ToList();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object IConvertableFromString.ConvertFromString(string str)
            => this.ConvertFromString(str);
    }
}
