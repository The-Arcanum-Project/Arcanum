---
id: "template"
title: "Navigable User Interface"
summary: "Docs for the NUI using the Location view as a template"
links: ["one"]
searchKeywords: ["template"]
category: "Debug"
level: "Panel"
scale: "Standard"
location: "Center"
status: "Beta"
iconPath: "Icon.Sync"
associatedScopes: ["Global"]
introducedIn: "v1.0.7"
---

The NUI (Navigable User Interface) reacts to whatever the current selection is. By default, the first time you come across it, you will see the Location page of the NUI. Each NUI page will be different based on the active item (Location, Culture, Religion, and so on) but there are usually variables, effects, or properties on each page related to the selection.

For demonstration purposes, the Location NUI page will be shown, but the core ideas can be used on other NUI pages.

![[NUI Overview.png]]

Here is an overview of the NUI when a location is selected. There are 



- 
- Where you can edit location properties such as what province it belongs to, the religion, topography, etc
- Changes depending on active selection
- Location:
	- Colour
		- The hexcode used by the location
	- Institution presence
		- Institutions active in the selected location
	- Pops
		- Shows an overview of the individual pop types in a location
		- Pop Rank (Peasants, Labourers, Nobles, etc)
	- Prosperity
		- Slider between 0-100
	- Province it is part of
	- Location Rank
	- Climate
	- Culture
	- Movement Assistance
	- Natural Harbour Suitability
	- Raw Material
	- Religion
	- Static Modifier
	- Topography
	- Unique ID
	- Vegetation
	- Town Setup