# Nexus System

The Nexus System is meant to replace the `Property` system with a more efficient and type-safe approach. It is designed
to be used in conjunction with the `NUI` system [(NUI Documentation)](../Arcanum.Core/CoreSystems/NUI/README.md) to
provide a seamless and fully generated user interface for editing game objects.

Nexus can be customized by attributes to modify the behavior of the generated code and UI.

## How to create a Nexus object

Create an object and inherit from `INexus`. This will trigger the source generator to create the necessary code for the
object.
Mark any property you want to be part of the Nexus system with the `[AddModifiable]` attribute or blacklist it with
`[IgnoreModifiable]`. By default, all properties are added, but the `[ExplicitProperties]` attribute can override that
behavior.

Example:

```csharp
[ExplicitProperties] // Only properties marked with [AddModifiable] will be added
public class MyNexusObject : INexus
{
    [AddModifiable] // This property will be added to the Nexus system
    public string Name { get; set; }    
}
```

```csharp
public class MyNexusObject2 : INexus
{
    [IgnoreModifiable] // This property will be ignored by the Nexus system
    public string Name { get; set; }
    public int Id { get; set; } // Property is added  by default 
}
```

## Generated Code

The source generator will create a partial class with customized and typesafe setters and getters for each property.

Example:

```csharp
public enum Field
    {
        [ExpectedType(typeof(string))]
        Example,
    }

    public void _setValue(Enum property, object value)
    {
        switch (property)
        {
            case Field.Example:
                Debug.Assert(value is string, "Example needs to be a string");
                this.Example = (string)value;
                break;
        }
    }

    public object _getValue(Enum property)
    {
        switch (property)
        {
            case Field.Example:
                return this.Example;
            default:
                throw new ArgumentOutOfRangeException(nameof(property));
        }
    }

    public object this[Enum key]
    {
        get => _getValue(key);
        set => _setValue(key, value);
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {        
        return true;
    }
```

To leverage this generated code the `Nx` class provides the necessary extension methods along with analyzers to ensure
type safety.

While the default methods have very strict rules in some cases you might want to bypass them. For this purpose the
`ForceGet` and `ForceSet` methods are provided.
Setter Example:

```csharp
var myNexusObject = new MyNexusObject();
myNexusObject.Set(Field.Example, "Hello World"); // Sets the Example property to "Hello World"
```

Getter Example:

```csharp
var myNexusObject = new MyNexusObject();
var exampleValue = myNexusObject.Get<string>(Field.Example); // Gets the Example property value as a string
```

Type Getter Example:

```csharp
var myNexusObject = new MyNexusObject();
var exampleType = myNexusObject.TypeOf(myNexusObject, Field.Example); // Gets the type
```