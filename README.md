# MasterConsole

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
ConvertableFromString<Vector3>.Convertor // '[1.04, 15, 25]' or '(1.2, 55, 1)'
ConvertableFromString<float>.Convertor  // '43.2'
ConvertableFromString<long>.Convertor // '492384928349'
ConvertableFromString<int>.Convertor  // '5234534'
ConvertableFromString<bool>.Convertor // 'true', 'enabled', 'up', 'down'
ConvertableFromString<DateTimeOffset>.Convertor // any correct string for DateTimeOffset.Parse
ConvertableFromString<decimal>.Convertor // '532e15'
ConvertableFromString<Color>.Convertor // '[1, 0.5, 0.1, 1]' or '(0, 1, 1, 1)'
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