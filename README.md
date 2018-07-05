# Winton.Extensions.Serialization.Json

[![Build status](https://ci.appveyor.com/api/projects/status/avnm0rp56l5u1isw?svg=true)](https://ci.appveyor.com/project/wintoncode/winton-extensions-serialization-json/branch/master)
[![Travis Build Status](https://travis-ci.org/wintoncode/Winton.Extensions.Serialization.Json.svg?branch=master)](https://travis-ci.org/wintoncode/Winton.Extensions.Serialization.Json)
[![NuGet version](https://img.shields.io/nuget/v/Winton.Extensions.Serialization.Json.svg)](https://www.nuget.org/packages/Winton.Extensions.Serialization.Json)
[![NuGet version](https://img.shields.io/nuget/vpre/Winton.Extensions.Serialization.Json.svg)](https://www.nuget.org/packages/Winton.Extensions.Serialization.Json)

Implementations of [JsonConverter](https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_JsonConverter.htm) providing useful serialization overrides.

## Converters

### SingleValueConverter

A converter for serializing types with a single backing field as the value of that field, and deserializing back to the original type. This can be useful, for example, when using [strong typed](https://tech.winton.com/2017/06/strong-typing-a-pattern-for-more-robust-code/) IDs. Consider a simple `AccountId` type

```csharp
public struct AccountId : IEquatable<AccountId>
{
    private readonly string _value;

    public AccountId(string value)
    {
        _value = value;
    }

    public static explicit operator string(AccountId id)
    {
        return id._value;
    }

    public static explicit operator AccountId(string value)
    {
        return new AccountId(value);
    }

    public static bool operator ==(AccountId left, AccountId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AccountId left, AccountId right)
    {
        return !left.Equals(right);
    }

    public bool Equals(AccountId other)
    {
        return string.Equals(_value, other._value);
    }

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return obj is AccountId id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value?.GetHashCode() ?? 0;
    }

    public override string ToString()
    {
        return _value;
    }
}
```

which is simply a strong typed wrapper around a `string`, providing expected cast, equality, and formatting members. It could be used, for example, on an `Account` type, say

```csharp
public sealed class Account
{
    public Account(
        AccountId id,
        AccountName name,
        ...)
    {
        AccountId = id,
        Name = name;
        ...
    }

    public AccountId Id { get; }

    public AccountName Name { get; }

    ...
}
```

to ensure that the `AccountId` and `AccountName` (which is presumably also string-like) could never be mixed up. However, since these wrapper types do not expose any public properties, by default they would serialize as empty objects, and the `Account` would always serialize as

```json
{
    "accountId": {},
    "accountName": {},
    ...
}
```

then deserialize as default values. Simply adding a `JsonConverter` attribute of type `SingleValueConverter` to the `AccountId` type (and similar for the `AccountName`), like so

```csharp
[JsonConverter(typeof(SingleValueConverter))]
public struct AccountId : IEquatable<AccountId>
{
    ...
}
```

will fix this issue, and cause the strong typed fields to serialize and deserialize as desired, such as

```json
{
    "accountId": "AZ001",
    "accountName": "WXX",
    ...
}
```