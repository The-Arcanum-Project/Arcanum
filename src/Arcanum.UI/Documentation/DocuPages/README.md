---
id: "editor.map" # The id of the feature the documentaiton is about. Ids can be browsed in debug mode.
title: "Preferences and Settings" # Title of the page.
summary: "Manage your app preferences, themes, and account data here." # Short summary of what this page is about.
links: ["security.md", "profile.md"] # Links to e.g. the wiki page for this page.
searchKeywords: ["preferences", "settings", "account", "themes", "privacy"] # Any number of keywords that are associated with this page for search purposes.
category: "SpecializedEditor" # Possible options: SpecializedEditor, Editor, Debug, Configuration, EditorMap
level: "Module" # Possible options: System, Module, Panel, Widget, Action
scale: "Standard" # Possible options: Compact, Standard, Major, Full
location: "Center" # Possible options: Center, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left, TopLeft
status: "Beta" # Possible options: Stable, Beta, Experimental, Legacy
---

# Full Documentation

This is the long-form text that shows in the main help window.

[Jump to Alerts](#alerts-demonstration)
## Sub-Headings

### Sub-Sub-Headings

#### Sub-Sub-Sub-Headings

You can have images, gifs, and long paragraphs here.

To have a link in the text execute a command:
Click here to [Open the Calculator](cmd:OpenCalc).

To Reference another feature reference it's id:
[Queastor](id:editor.queastor)

To have an expanding tooltip on an element:
[Hover over me](tt: "This is the tooltip text")

#### Showing an image
scale Options: None, Fill, Uniform, UniformToFill
Align Options: Left, Center, Right

Centered, 200px wide 
![Cat](cat.png){.center width=200}

Right-aligned, fixed height, uniform fill
![Cat](cat.png){.right height=150 scale=UniformToFill}

Stretched to fill the page entirely
![Cat](cat.png){.stretch scale=Fill}

To have a normal link:
[Click here](https://github.com/The-Arcanum-Project/Arcanum)

To have a gifs:
![A gif](cat.gif)

```csharp
Console.WriteLine("Code blocks are also supported!");
```


Here is some simle inline code: `var x = 10;`

# Alerts Demonstration

> [!NOTE]
> Highlights information that users should take into account, even when skimming.

> [!TIP]
> Optional information to help a user be more successful.

> [!IMPORTANT]
> Crucial information necessary for users to succeed.

> [!WARNING]
> Critical content demanding immediate user attention due to potential risks.

> [!CAUTION]
> Negative potential consequences of an action.

# Tabs Demonstration

:::: tabs
::: tab C#
```csharp
var x = 10;
Console.WriteLine(x);
```
:::
::: tab VB
```vb
Dim x = 10
Console.WriteLine(x)
```
:::
::: tab Info
You can put any markdown inside a tab, including images or other links!
Click Here
:::
::::

---
A separator

Just some inline highlight
> Double-clicking a recent project will open it directly


---section:Debug---
This part only shows up for 'Advanced' users or in specific tech-popups.