# Unity-Weld
[![NuGet](https://img.shields.io/nuget/dt/RSG.UnityWeld.svg)](https://www.nuget.org/packages/RSG.UnityWeld/)
[![NuGet](https://img.shields.io/nuget/v/RSG.UnityWeld.svg)](https://www.nuget.org/packages/RSG.UnityWeld/)
[![Build Status](https://travis-ci.org/Real-Serious-Games/Unity-Weld.svg?branch=master)](https://travis-ci.org/Real-Serious-Games/Unity-Weld)


*[MVVM-style](https://msdn.microsoft.com/en-us/library/hh848246.aspx) data-binding system for Unity.*

Unity-Weld is a library for Unity 5+ that enables two-way data binding between Unity UI widgets and game/business logic code. This reduces boiler-plate code that would otherwise be necessary for things like updating the UI when a property changes, removes the need for messy links between objects in the scene that can be broken easily, and allows easier unit testing of code by providing a layer of abstraction between the UI and your core logic code.

A series of articles on Unity Weld has been published on [What Could Possibly Go Wrong](http://www.what-could-possibly-go-wrong.com/bringing-mvvm-to-unity-part-1-about-mvvm-and-unity-weld).

Example Unity project can be found here: [https://github.com/Real-Serious-Games/Unity-Weld-Examples](https://github.com/Real-Serious-Games/Unity-Weld-Examples).

## Installation

To install Unity-Weld in a new or existing Unity project:
 - Load `Unity-Weld.sln` in Visual Studio and build it
 - Copy `UnityWeld.dll` into your Unity project and place in any directory within `Assets`
 - Copy `UnityWeld_Editor.dll` into your Unity project and place it inside an `Editor` folder within `Assets`

Alternatively, just copy the `UnityWeld/Binding` and `UnityWeld/Widgets` folders into your `Assets` directory in your Unity project, and copy all the .cs files in `UnityWeld_Editor` to a folder named `Editor` inside your `Assets` directory.


## Getting started

Check out the [Unity-Weld-Examples](https://github.com/Real-Serious-Games/Unity-Weld-Examples) repository for some examples of how to use Unity-Weld.

[API docmentation](https://github.com/Real-Serious-Games/Unity-Weld/wiki) is on our wiki.

If you're interested in getting involved feel free to check out the [roadmap on Trello](https://trello.com/b/KVFUvGR0), or submit a pull request. Make sure to read our [contributing guide](CONTRIBUTING) first.
