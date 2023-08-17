# MasterConsole

![image](https://github.com/0xF6/master.console/assets/13326808/04a2c747-5df6-4c27-a87e-35a9b4a69e51)


### Dependency
`com.cysharp.unitask`   
`org.nuget.system.runtime.compilerservices.unsafe`   
`org.nuget.microsoft.extensions.logging.abstractions`  
`jp.hadashikick.vcontainer`   


### Install 

Install dependencies over `openupm`

```sh
# in project root folder
openupm add org.nuget.microsoft.extensions.logging.abstractions
openupm add org.nuget.system.runtime.compilerservices.unsafe
openupm add com.cysharp.unitask
openupm add jp.hadashikick.vcontainer
```


### Command & Variables Registration



#### Register variable

First you are need create command class
```C#
public class UtilsCommands : CommandSilo
{
    public override void Create(CommandHandlerContext context)
    {
        // context.Command<int>(...)
    }
}
```

after you are need to register your command class in VContainer flow

```C#
public class GameLifeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register...


        builder.UseTerminal(Settings, x => {
            x.UseGameObjectCommands(); // with masterconsole.gameobjects package
            x.Use<GraphicsCommands>();
            x.Use<SteamCommands>();
            x.Use<GameStateFeature.Commands>();
            x.Use<LobbyFeature.LobbyCommand>();
        });

        base.Configure(builder);
    }
}
```
And describe command...

```C#
context.Variable("graphic.camera.distance", 
    ( ) => this.camera.farClipPlane,
    (x) => this.camera.farClipPlane = Math.Clamp(x, 300f, 5000f));
context.Variable("graphic.camera.depth.clear", 
    ( ) => this.hdrpSettings.clearDepth,
    (x) => this.hdrpSettings.clearDepth = x);
```

#### Use variable

```js
>graphic.camera.distance
<500

>graphic.camera.distance 1200
<1200

>graphic.camera.distance 10
<300

>graphic.camera.depth.clear
<true

>-graphic.camera.depth.clear
<false

>graphic.camera.depth.clear true
<true
```

#### Register command

```C#
ctx.Command<SteamId>("net.connect", (id) => client.ConnectTo(id));
```


#### Use command

```js
>net.connect 0000000000000000
...
```


### About generic parameter in `ctx.Command<>`

This method uses `ConvertableFromString<T>.Convertor` to convert input or output data   
For enumeration types (type.IsEnum), `EnumConverter<T>` is automatically registered


List of registered handlers:
```C#
ConvertableFromString<byte>.Convertor // '25'
ConvertableFromString<sbyte>.Convertor // '-25'
ConvertableFromString<short>.Convertor // '4884'
ConvertableFromString<string>.Convertor // 'the string'
ConvertableFromString<Vector3>.Convertor // '[1.04, 15, 25]' or '(1.2, 55, 1)', can disable by DISABLE_DEFAULT_UNITY_CONVERTERS swtich
ConvertableFromString<float>.Convertor  // '43.2'
ConvertableFromString<long>.Convertor // '492384928349'
ConvertableFromString<int>.Convertor  // '5234534'
ConvertableFromString<bool>.Convertor // 'true', 'enabled', 'up', 'down'
ConvertableFromString<DateTimeOffset>.Convertor // any correct string for DateTimeOffset.Parse
ConvertableFromString<decimal>.Convertor // '532e15'
ConvertableFromString<Color>.Convertor // '[1, 0.5, 0.1, 1]' or '(0, 1, 1, 1)', can disable by DISABLE_DEFAULT_UNITY_CONVERTERS swtich

ConvertableFromString<TimeSpan>.Convertor // any correct string for TimeSpan.Parse 
ConvertableFromString<Guid>.Convertor // guid
ConvertableFromString<Index>.Convertor // '^15' or '15'
ConvertableFromString<Range>.Convertor // '0.15'
ConvertableFromString<Type>.Convertor // UnityEngine.GameObject or etc correct string for Type.Find


// in masterconsole.gameobjects package
ConvertableFromString<GameObject>.Convertor // GoQL query and take first object
ConvertableFromString<GameObject[]>.Convertor // GoQL query and get all queried objects
ConvertableFromString<List<GameObject>>.Convertor // same but list type
```

#### Custom converter

```C#
public class Convertable_SteamId : IConvertableFromString<SteamId>
{
    public SteamId ConvertFromString(string str)
        => ulong.Parse(str);

    object IConvertableFromString.ConvertFromString(string str)
        => this.ConvertFromString(str);
}

public static class SteamIdAutoRegistration
{
    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        ConvertableFromString<SteamId>.Convertor = new Convertable_SteamId();
    }
}
```



### About camera resolving


There is an `ICameraResolver` interface, it describes the method of getting the camera in the right way.    
In the static class a `CameraResolver.Resolver` field, its default value is `CameraResolverFromContainer`.   
`CameraResolverFromContainer` tries to extract an instance of `Camera` from `IObjectResolver`, if it fails, it accesses `CameraResolverFromContainer.FallbackResolver` which by default is `MainCameraResolver`.    
`MainCameraResolver` gets the camera via `Camera.main`



### About default commands


#### default commands
```bash
clear # clear console
exit # call application.quit()
```

Disabled by DISABLE_DEFAULT_COMMANDS directive switch

#### commands in masterconsole.gameobjects

GameObject commands has been use GoQL language for querying, [learn more](https://docs.unity3d.com/Packages/com.unity.selection-groups@0.8/manual/goql.html)

```bash
go.query *Head # return information about all quered objects
go.destroy * # destroy all quered objects
go.active * true # set active all objects
go.message <t:Camera> FooBarMethod # call SendMessage in all quered objects
go.inject "Traps/[0,1,5]" Foobar.GameComponent # add component to quered objects by type name
```


#### about formatting gameObject

Formatter interface is `IGameObjectFormatter`, there is a `GameObjectFormatter` field in `GameObjectCommands` class with default value of `DefaultGameObjectFormatter`, you can implement custom formatter and set to this field.
