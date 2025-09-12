# Capabilities Ceasar Parser

# Output
A list of Blocks containing any list of subblocks or contents starting from the file root.
A list of contents containing any list of key or key-value pairs starting from the file root.

Each block and content should provide a line number and char index for each element that carries information so not for example the opening and closing brackets of a block or the separators.

## Comments
A comment starts with a # and ends at the end of the line.
There are no multi-line comments.

## Blocking
The code is split up into blocks and contents.

Valid characters for block names are:
- a-z
- A-Z
- 0-9
- _.
But numbers and dots cannot be the first character.

The opening brackets can be on the same line as the block name or on the next line.

Default block:
```csharp
default_block = {
    <content>
}
```
Array Definition:
```csharp
{
    default_block = {
        <content>
    }    
}
```

## Contents
Valid characters for content keys are:
- a-z
- A-Z
- 0-9
- _.:
But numbers, : and dots cannot be the first character.

A content is just a collection of key or key-value pairs which are separated by different separators. Otherwise it is a key collection if none of the separators are present.
```csharp
= 
<=
>=
?=
```

This is also treated as a content.

Inline Math at file root:
```csharp
@example_height = @[ 1 / 13 * 7 ]
```
Inline Math in blocks:
```csharp
block = {
    position = { 
        @[1/3/2] 
        0.5 
    }
}

Special cases:
```csharp
@key = hsv { 0.5, 0.5, 0.5 }
@key = rgb { 255, 255, 255 }
```

