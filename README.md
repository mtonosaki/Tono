# Tono - Tools Of New Operation

for trying agile SoE(SoI) style development framework / library set

## Libraries

### Tono.Core
![](https://aqtono.com/tomarika/tono/TonoCoreIcon.png)  
Common library of Tono architecture contains below classes.  
* Type safe mechanism
  * Acceleration, Angle, Distance, Id, NamedId, Lon/Lat, Speed, Volume, Weight
* Caluclations
  * GeoEu, MathUtil
* Utility
  * CacheStack, Collection, DbUtil, Japanese language, Mutex, QueueOverProcess, StrUtil, Synonym, Time/Span
* Extend method
  * DictionaryExtensions, List
* Network
  * IpNetwork, MacAddress, MacOui

### Tono.Logic
![](https://aqtono.com/tomarika/tono/TonoLogicIcon.png)  
Algorithm Helpers / Solvers
* Goal Chasing Method
* Integer Distributer
* Interpolation (Ease-in / out)
* Least Squares N-Polynominal
* Route solver (Dijekstra)

### Tono.Gui.Jit
![](https://aqtono.com/tomarika/tono/TonoJitIcon.png)  
Simple objects to help design/implement business model with Just-in-time philosophy.
*  Stage
*  Work (Parts)
*  Process
*  Kanban
*  Constraints
*  Commands

### Tono.Excel
![](https://aqtono.com/tomarika/tono/TonoExcelIcon.png)  
Convert from Excel file to DataSet object


## MVFP Architecture Frameworks
Model - View - Feature - Parts Architecture is an original GUI framework considering team work, kaizen activity and agile style development.

### Tono.Gui
![](https://aqtono.com/tomarika/tono/TonoGuiIcon.png)  
Gui common library that is important to implement MVFP architecture.
* 3-coodinates concept
* Event token  mechanism

### Tono.Gui.Uwp
![](https://aqtono.com/tomarika/tono/TonoGuiUwpIcon.png)  
A framework of MVFP architecture for Windows UWP developments
* Data base classes to implement application data models
* View UWP control and additional UWP controls.
* Feature class
* Parts base class
* Some GUI support library like Color, Timer, Configulation

### Tono.GuiWinForm
![](https://aqtono.com/tomarika/tono/TonoGuiWinFormIcon.png)  
A framework of MVFP architecture for WinForm developments with .NET Framework version 4.7.1  
(legacy library. not recomended to use it)
