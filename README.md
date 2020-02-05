[![licence badge]][licence]
[![stars badge]][stars]
[![forks badge]][forks]
[![issues badge]][issues]

[licence badge]:https://img.shields.io/badge/license-MIT-blue.svg
[stars badge]:https://img.shields.io/github/stars/mtonosaki/Markdown.svg
[forks badge]:https://img.shields.io/github/forks/mtonosaki/Markdown.svg
[issues badge]:https://img.shields.io/github/issues/mtonosaki/Markdown.svg

[licence]:https://github.com/hey-red/Markdown/blob/master/LICENSE.md
[stars]:https://github.com/mtonosaki/Markdown/stargazers
[forks]:https://github.com/mtonosaki/Markdown/network
[issues]:https://github.com/mtonosaki/Markdown/issues

# Tono - Tools Of New Operation

"Tools of new operation" library/framework is for agile SoE(SoI) style development.   
Expected a lot of developer create original simulators quickly and find business core rule of improvement continuously. That is why this is named "tools of new operation".

## Libraries

### Tono.Core
![](https://aqtono.com/tomarika/tono/TonoCoreIcon.png)  
Common library of Tono architecture contains below classes.

Category|Objects
-|-
Type safe helper|Acceleration, Angle, Distance, Id, NamedId, Lon/Lat, Speed, Volume, Weight
Caluclations|GeoEu, MathUtil
Utility|CacheStack, Collection, DbUtil, Japanese language, Mutex, QueueOverProcess, StrUtil, Synonym, Time/Span
Extend method|DictionaryExtensions, List
Network|IpNetwork, MacAddress, MacOui

### Tono.Logic
![](https://aqtono.com/tomarika/tono/TonoLogicIcon.png)  
Algorithm Helpers / Solvers

Function|Remarks
-|-
Goal Chasing Method|Simple Heijunka(average) logic
Integer Distributer|You can get integer values that total is same from distributed numbers.
Interpolation|Ease-in / Ease-out / Ease-in-out curve function
Least Squares N-Polynominal|Prospect values from some input data using least squares method.
Route solver|To find a low-cost link connection with Dijekstra algorithm.

### Tono.Jit
![](https://aqtono.com/tomarika/tono/TonoJitIcon.png)  
Simple objects to help design/implement business model with Just-in-time philosophy. Making "Object", "Signal" and "Flow" with below objects.

Function|Object|Comment
-|-|-
Stage|Simulation world|Object of core modeling driver
Object|Work|Moving target that make a simulation result. Not only physical target.
Signal|Kanban|Pull signal between processes. Means a "Work" stop mechanism at a process.
Flow|Process|Node of work flow for graph theory.
Flow rule|Constraints|For example, MaxCount constraint describes like a transpotation road because road can have limited object at one time.
Flow control|Commands|Another constraint model like "Delay" time to simulate work through speed.
JAC|JacInterpreter|JAC (Just-in-time model As a Code) helps you to implement Undo/Redo as a "Compensating Transaction Pattern"

### Tono.Excel
![](https://aqtono.com/tomarika/tono/TonoExcelIcon.png)  
Convert from Excel file to DataSet object


## MVFP Architecture Frameworks
Model - View - Feature - Parts Architecture is an original GUI framework considering team work, kaizen activity and agile style development.

### Tono.Gui
![](https://aqtono.com/tomarika/tono/TonoGuiIcon.png)  
Gui common library that is important to implement MVFP architecture.

Function|Remarks
-|-
3-coodinates concept|Let your mind free from complicated screen pixel values especially debugging activity. You can just place parts with your logical value like clock-time.
Event token  mechanism|Let your team free from complicated development activity between functions and functions. This is a function of message of object-oriented programming.

### Tono.Gui.Uwp
![](https://aqtono.com/tomarika/tono/TonoGuiUwpIcon.png)  
A framework of MVFP architecture for Windows UWP developments

Function|Remarks
-|-
Model|**M**VFP : You can implement application data with this class.
View|M**V**FP : UWP control of this GUI architecture.
Feature|MV**F**P : Feature base class like plug-in function mechanism. Feature is possible to be a function, constraint and design controls.
Parts|MVF**P** : Parts base class to be the all graphical implementation of this architecture.
Library|Color, Timer, Configulation, Some UWP controls having MVFP linkage.

### Tono.GuiWinForm
![](https://aqtono.com/tomarika/tono/TonoGuiWinFormIcon.png)  
A framework of MVFP architecture for WinForm developments with .NET Framework version 4.7.1  
(legacy library. not recommended to use it)

### Tono.AspNetCore
![](https://aqtono.com/tomarika/tono/TonoAspNetCoreIcon.png)  
Utility for Web application of ASP.NET Core   
ex. Client Certificate Authentication.



