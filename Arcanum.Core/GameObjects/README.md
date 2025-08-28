# Game Objects

Every object we create from the game-files is classified as a game object. 
They are classified into **2** main categories:
- **Readonly**: These objects are created from the game files and are not meant to be modified. They are used to represent the data as it is in the game files.
- **Writable**: These objects are created from the game files but are meant to be modified. They are used to represent the data as it is in the game files but with the ability to modify it.

## Readonly Game Objects
This one is really simple, just create a class / struct. If this object should be iterable for GUI purposes, implement the `ICollectionProvider<T>` interface.

Example:
```csharp
public class MyGameObject : ICollectionProvider<MyGameObject>
{
    // Example properties
    public string Id { get; set; }
    public string Name { get; set; }
    
    // Implementing ICollectionProvider
    public static IEnumerable<MyGameObject> GetGlobalItems() => Globals.MyGameObjects;
}
```

## Writable Game Objects
Writable game objects require at least the `INexus` interface BUT with only this implemented no UI can be generated. To have auto-generated UI in any `ContentPresenter` you need to implement `INUI` interface. More about `NUI` and how it works in the [NUI documentation](../CoreSystems/NUI/README.md). 

`INUI` bring `INexus` with it along with all handles for the UI generation.
Example:
```csharp
public class MyWritableGameObject : INUI, ICollectionProvider<MyWritableGameObject>
{
    // Example properties
    public string Id { get; set; }
    [IgnoreModifiable] // This property will be ignored by the UI generation
    public string Name { get; set; }

    // Implementing ICollectionProvider
    public static IEnumerable<MyWritableGameObject> GetGlobalItems() => Globals.MyWritableGameObjects
    
    public bool IsReadonly => false; // If the ui is readonly or not
    public NUISetting Settings { get; } = Config.Settings.NUIObjectSettings.MyWritableGameObjectSettings;
    public INUINavigation[] Navigations { get; } = [];
}
```
