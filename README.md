# Unity-Weld
*[MVVM-style](https://msdn.microsoft.com/en-us/library/hh848246.aspx) data-binding system for Unity.*

Unity-Weld is a library for Unity 5+ that enables two-way data binding between Unity UI widgets and game/business logic code. This reduces boiler-plate code that would otherwise be necessary for things like updating the UI when a property changes, removes the need for messy links between objects in the scene that can be broken easily, and allows easier unit testing of code by providing a layer of abstraction between the UI and your core logic code.

## Installation

To install Unity-Weld in a new or existing Unity project:
 - Load `Unity-Weld.sln` in Visual Studio and build it
 - Copy `UnityUI.dll` into your Unity project and place in any directory within `Assets`
 - Copy `UnityUI_Editor.dll` into your Unity project and place it inside an `Editor` folder within `Assets`

Alternatively, just copy the `UnityUI/Binding` and `UnityUI/Widgets` folders into your `Assets` directory in your Unity project, and copy all the .cs files in `UnityUI_Editor` to a folder named `Editor` inside your `Assets` directory.


## Getting started

Check out the [Unity-Weld-Examples](https://github.com/Real-Serious-Games/Unity-Weld-Examples) repository for some examples of how to use Unity-Weld.
