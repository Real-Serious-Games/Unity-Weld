# Unity-Weld
*[MVVM-style](https://msdn.microsoft.com/en-us/library/hh848246.aspx) data-binding system for Unity.*

Unity-Weld is a library for Unity 2019+ that enables two-way data binding between Unity UI widgets and game/business logic code. This reduces boiler-plate code that would otherwise be necessary for things like updating the UI when a property changes, removes the need for messy links between objects in the scene that can be broken easily, and allows easier unit testing of code by providing a layer of abstraction between the UI and your core logic code.

A series of articles on Unity Weld has been published on [What Could Possibly Go Wrong](http://www.what-could-possibly-go-wrong.com/bringing-mvvm-to-unity-part-1-about-mvvm-and-unity-weld).

FOR ORIGINAL FORK: Example Unity project can be found here: [https://github.com/Real-Serious-Games/Unity-Weld-Examples](https://github.com/Real-Serious-Games/Unity-Weld-Examples). 

## Installation

To install Unity-Weld in a new or existing Unity project:
 Option 1: use package.json to locally install as UPM package using UPM package mananger
 Option 2: generate upm package using npm tool (npm pack), upload to your feed and add your feed to Unity Package Manager

Alternatively, just copy the `Editor` to `Scripts/UnityWeld/Editor` and `Runtime` to `Scripts/UnityWeld/Runtime` into your `Assets` directory in your Unity project.
