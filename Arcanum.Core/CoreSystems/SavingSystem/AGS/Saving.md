# Automated Saving System

Each object which can use this also has to use the `ParsingGenerator` as they share attributes and data.

Via the `ParseAs` attribute we know whether a property is a `content` or a `block` node. We also know the name of the
identifier of the node. \
We can also get the value for it by just using `Nx.Get<>()`

## New Attributes:

### `SavingComment`

This attribute takes a string which should be a method name to a class in the `SavingCommentProvider` class or just a
static string as a comment.

### `CustomSaving`

This attribute takes a method name to a class in the `CustomSavingProvider` class. This is then invoked to create the
string for the node.

## New Classes:

### `SavingCommentProvider`

This class is used to provide all methods providing comments for the nodes.

### `CustomSavingProvider`

This class is used to provide all custom saving methods for the nodes.

### `HeaderProvider`

This class is used to provide all methods providing the header for the nodes.

### `FooterProvider`

This class is used to provide all methods providing the footer for the nodes.

### `SettingsProvider`

This class is used to provide all methods providing the settings for the nodes.

## Settings:

- `HasSavingComment`
- `AlphabeticOrder`
- `CustomOrderedList`
- `SaveEmptyCollections`

## Usage:

The code generator will generate a class containing 2 main features:

- Generate a string for an `INexus` property
- Generate a string for the entire `INexus` object

In the class we want to use the strategy pattern to generate a

```c#
dictionary<nxProperty, Func<string>> 
```

which holds the methods to generate the saving strings for the nodes. \
Each `nxProperty` will have a method generated containing the call to the comment string if enabled, then the call to
the saving string.

Each object will have a method to retrieve the settings on how it should be save.