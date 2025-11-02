# AGS: Automatic Generative Saving System

AGS is a conde generator-based system designed to serialize objects into EU5 Jomini syntax. It provides a simple,
customizable, declarative syntax for defining how objects should be saved.

## Table of Contents

1. [Core Concept: The `IAgs` Interface](#core-concept-the-iags-interface)
2. [Getting Started: Basic Usage](#getting-started-basic-usage)
    - [Making a Class Saveable](#1-making-a-class-saveable)
    - [Defining the Object's Structure: `[ObjectSaveAs]`](#2-defining-the-objects-structure-objectsaveas)
    - [Marking Properties for Saving: `[SaveAs]`](#3-marking-properties-for-saving-saveas)
    - [Ignoring Properties: `[SuppressAgs]`](#4-ignoring-properties-suppressags)
3. [Attribute Reference](#attribute-reference)
    - [`[ObjectSaveAsAttribute]`](#objectsaveasattribute)
    - [`[SaveAsAttribute]`](#saveasattribute)
4. [Advanced Customization](#advanced-customization)
    - [Custom Saving Logic](#custom-saving-logic)
    - [Custom Comments](#custom-comments)
    - [Custom Collection Item Keys](#custom-collection-item-keys)
5. [Controlling Output Format with `AgsSettings`](#controlling-output-format-with-agssettings)
6. [How It Works (Under the Hood)](#how-it-works-under-the-hood)
7. [Full Examples](#full-examples)
    - [InlineTestObject.cs](#inlinetestobjectcs)
    - [TestOb2j.cs](#testob2jcs)

## Core Concept: The `IAgs` Interface

The cornerstone of the system is the `IAgs` interface. Any class you want to make saveable with AGS **must** implement
this interface.

```csharp
public interface IAgs : INexus
{
   // Settings to control output formatting; customizable by user.
   AgsSettings Settings { get; }

   // A list of all properties marked with [SaveAs]. (Generated automatically)
   IReadOnlyList<PropertySavingMetadata> SaveableProps { get; }

   // Metadata for the class itself. (Generated automatically)
   ClassSavingMetadata ClassMetadata { get; }

   // The key used for this object
   string SavingKey { get; }
}
```

## Getting Started: Basic Usage

### 1. Making a Class Saveable

First, implement the `IAgs` interface. A source generator handles the implementation of `SaveableProps` and
`ClassMetadata` for you, so you only need to provide `Settings` and `SavingKey`.

```csharp
// The 'partial' keyword is required for the source generator.
public partial class MyObject : IAgs
{
    // The root key for this object in the output file.
    public string SavingKey => "my_object";

    // Provides settings for formatting. Can be customized.
    public AgsSettings Settings { get; } = new();

    // ... your properties go here
}
```

### 2. Defining the Object's Structure: `[ObjectSaveAs]`

Use the `[ObjectSaveAs]` attribute on the class to define the tokens used for its block structure. By default, it uses
`=` as the separator and `{ ... }` for the block.

```csharp
[ObjectSaveAs] // Uses default: my_object = { ... }
public partial class MyObject : IAgs
{
    // ...
}
```

### 3. Marking Properties for Saving: `[SaveAs]`

Apply the `[SaveAs]` attribute to any property you want to include in the saved output. The property's name will be
automatically used as the keyword.

```csharp
[ObjectSaveAs]
public partial class MyObject : IAgs
{
    public string SavingKey => "my_object";
    public AgsSettings Settings { get; } = new();

    [SaveAs] // Will be saved as 'name = "Player One"'
    public string Name { get; set; } = "Player One";

    [SaveAs] // Will be saved as 'level = 10'
    public int Level { get; set; } = 10;
}
```

### 4. Ignoring Properties: `[SuppressAgs]`

If a property should be excluded from saving, either omit the `[SaveAs]` attribute or, for clarity, use the
`[SuppressAgs]` attribute.

```csharp
public partial class MyObject : IAgs
{
    [SaveAs]
    public int Level { get; set; } = 10;

    [SuppressAgs] // This property will NOT be saved.
    public bool IsDirty { get; set; }
}
```

## Attribute Reference

### `[ObjectSaveAsAttribute]`

Defines how the class block is written.

**Parameters:**

- `separator`: `TokenType` - The separator between the `SavingKey` and the opening brace. Default: `TokenType.Equals` (
  `=`).
- `openingToken`: `TokenType` - The token to start the object block. Default: `TokenType.LeftBrace` (`{`).
- `closingToken`: `TokenType` - The token to end the object block. Default: `TokenType.RightBrace` (`}`).
- `commentMethod`: `string` - Name of a method in [`SavingCommentProvider`](#custom-comments) to generate a comment for
  the object.
- `savingMethod`: `string` - Name of a method in [`SavingActionProvider`](#custom-saving-logic) to override the entire
  object saving logic.

### `[SaveAsAttribute]`

Defines how a property is written. Placed on properties.

**Parameters:**

- `valueType`: `SavingValueType` - Specifies how to format the value (e.g., `String`, `Int`, `Identifier`). Default:
  `Auto` (inferred from the property type).
- `separator`: `TokenType` - The separator between the property name and its value. Default: `TokenType.Equals` (`=`).
- `isCollection`: `bool` - Manually marks the property as a collection to be saved inside a block. Useful for types that
  are `IEnumerable` but shouldn't be treated that way. Default: `false`.
- `savingMethod`: `string` - Name of a method in [`SavingActionProvider`](#custom-saving-logic) to provide custom saving
  logic for this specific property.
- `commentMethod`: `string` - Name of a method in [`SavingCommentProvider`](#custom-comments) to generate a comment
  preceding this property.
- `collectionKeyMethod`: `string` - Name of a method in [`CustomItemKeyProvider`](#custom-collection-item-keys) to
  format each item in a collection.

**Example Usage:**

```csharp
[SaveAs(separator: TokenType.LessOrEqual)]
public int Id { get; set; } // Output: id <= 123

[SaveAs(valueType: SavingValueType.Identifier)]
public List<string> Tags { get; set; } // Output: tags = { tag1 tag2 }

[SaveAs(savingMethod: "ExampleCustomSavingMethod")]
public string Key { get; set; } // Uses custom logic to save
```

## Advanced Customization

AGS allows you to override default behaviors by providing custom methods in static "Provider" classes. You reference
these methods by name (as a string) in the attributes.

### Custom Saving Logic

**Provider Class:** `SavingActionProvider.cs`

If you need complete control over how a property (or even a whole object) is written, you can specify a `savingMethod`.
This method receives the object instance, its metadata, and the string builder.

**Example:**

1. Define a method in `SavingActionProvider.cs`:
   ```csharp
   public static class SavingActionProvider
   {
      public static void ExampleCustomSavingMethod(IAgs target, PropertySavingMetadata metadata, IndentedStringBuilder sb)
      {
         object value = null!;
         Nx.ForceGet(target, metadata.NxProp, ref value);
         sb.AppendLine($"# Custom saving for property: {metadata.Keyword}");
         sb.AppendLine($"# Value: {value}");
         sb.AppendLine($"{metadata.Keyword} = {value}");
      }
   }
   ```
2. Reference it in the `[SaveAs]` attribute:
   ```csharp
   [SaveAs(savingMethod: "ExampleCustomSavingMethod")]
   public string Key { get; set; } = "some_value";
   ```
   **Output:**
   ```plaintext
   # Custom saving for property: key
   # Value: some_value
   key = some_value
   ```

### Custom Comments

**Provider Class:** `SavingCommentProvider.cs`

You can add dynamic comments before a property or object is written by specifying a `commentMethod`.

**Example:**

1. Define a method in `SavingCommentProvider.cs`:
   ```csharp
   public static class SavingCommentProvider
   {
      public static string DefaultCommentProvider(object value, Enum nxProp)
      {
         return $"# Saving property: {nxProp}";
      }
   }
   ```
2. Reference it:
   ```csharp
   // (Note: The default generator might already add a comment method,
   // this is how you would specify one manually)
   [SaveAs(commentMethod: "DefaultCommentProvider")]
   public string Name { get; set; }
   ```

### Custom Collection Item Keys

**Provider Class:** `CustomItemKeyProvider.cs`

When saving a collection, the default behavior is to format each item based on its type. You can override this by
providing a `collectionKeyMethod`.

**Example:**

1. Define a method in `CustomItemKeyProvider.cs`:
   ```csharp
   public static class CustomItemKeyProvider
   {
      public static string GetPlayerId(object value)
      {
         // Assume 'value' is a Player object
         var player = (Player)value;
         return $"player_{player.Id}";
      }
   }
   ```
2. Reference it:
   ```csharp
   [SaveAs(collectionKeyMethod: "GetPlayerId")]
   public List<Player> Players { get; set; }
   ```

## Controlling Output Format with `AgsSettings`

The `AgsSettings` class, implemented as part of `IAgs`, provides high-level control over the final output.

```csharp
public class AgsSettings
{
   // If true, save properties in the order specified in `SaveOrder`.
   // If false, saves alphabetically by keyword.
   public bool CustomSaveOrder { get; set; } = false;
   public List<Enum> SaveOrder { get; set; }

   // If true, write an object's header even if the collection is empty.
   public bool WriteEmptyCollectionHeader { get; set; } = true;

   // Controls spacing between properties and within blocks.
   // Options: Compact, Default, Spacious
   public SavingFormat Format { get; set; } = SavingFormat.Default;
}
```

You can configure these settings in the constructor of your saveable object:

```csharp
public partial class MyOrderedObject : IAgs
{
    public string SavingKey => "ordered_object";
    public AgsSettings Settings { get; }

    public MyOrderedObject()
    {
        Settings = new AgsSettings
        {
            CustomSaveOrder = true,
            // Defines the exact order properties will be written in.
            SaveOrder = [Props.Id, Props.Name, Props.Description],
            Format = SavingFormat.Spacious
        };
    }

    // ... properties
}
```

## How It Works (Under the Hood)

The saving process follows these steps:

1. A source generator runs at compile time, reading the `[ObjectSaveAs]` and `[SaveAs]` attributes on your
   `partial class`.
2. It generates the implementations for `SaveableProps` and `ClassMetadata`, creating `PropertySavingMetadata` and
   `ClassSavingMetadata` objects that contain the pre-compiled instructions from your attributes.
3. When you call `ags.ToAgsContext(commentChar).BuildContext(sb)`, an `AgsObjectSavingContext` is created.
4. This context sorts the `PropertySavingMetadata` based on your `AgsSettings` (alphabetically or custom order).
5. It then iterates through each property's metadata, calling its `Format` method.
6. The `Format` method uses the instructions (separator, value type, custom methods) to write the correctly formatted
   string to the `IndentedStringBuilder`.

## Full Examples

### InlineTestObject.cs

This object demonstrates different separators, value types, and a simple string collection.

**Code:**

```csharp
[ObjectSaveAs]
public partial class InlineTestObject : IAgs
{
   [SaveAs(separator: TokenType.LessOrEqual)]
   public int Id { get; set; } = Random.Shared.Next(0, 140);
   [SaveAs]
   public string Name { get; set; } = "Inline Test Object";
   [SaveAs]
   public string Description { get; set; } = "This is a test object for inline saving.";
   [SaveAs]
   public float SomeFloat { get; set; } = 3.14f;
   [SaveAs]
   public bool SomeBool { get; set; } = true;
   [SaveAs(separator: TokenType.Greater, valueType: SavingValueType.Identifier)]
   public List<string> SomeStrings { get; set; } = ["One", "Two", "Three"];
   [SaveAs]
   public SavingFormat Format { get; set; } = SavingFormat.Spacious;

   public AgsSettings Settings { get; } = new();
   public string SavingKey => "inline_this";
}
```

**Example Output:**

```plaintext
inline_this = {
	description = "This is a test object for inline saving."
	floating = 3.14
	formation = Spacious
	id <= 103
	name = "Inline Test Object"
	some_bool = yes
	some_strings > {
		One
		Two
		Three
	}
}
```

### TestOb2j.cs

This object demonstrates nesting another `IAgs` object, using a custom saving method, and handling an empty collection.

**Code:**

```csharp
[ObjectSaveAs]
public partial class TestOb2j : IAgs
{
   [SaveAs(separator: TokenType.LessOrEqual)]
   public List<bool> IsProp { get; set; } = new();

   [SaveAs(savingMethod: "ExampleCustomSavingMethod")]
   public string Key { get; set; } = "boh_hussite_king";

   [SaveAs]
   public InlineTestObject Inline { get; set; } = new();

   public AgsSettings Settings { get; } = new();
   public string SavingKey => "test_obj";
}
```

**Example Output:**

```plaintext
test_obj = {
	inline = {
		description = "This is a test object for inline saving."
		floating = 3.14
		formation = Spacious
		id <= 104
		name = "Inline Test Object"
		some_bool = yes
		some_strings > {
			One
			Two
			Three
		}
	}
	is_prop <= {
		# Empty Collection
	}
	# Custom saving for property: key
	# Value: boh_hussite_king
	key = boh_hussite_king
}
```