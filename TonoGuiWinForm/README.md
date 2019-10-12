# Tono - Tools Of New Operation

"Tools of new operation" library/framework is for agile SoE(SoI) style development.   
Expected a lot of developer create original simulators quickly and find business core rule of improvement continuously. That is why this is named "tools of new operation".

## MVFP Architecture Frameworks
Model - View - Feature - Parts Architecture is an original GUI framework considering team work, kaizen activity and agile style development.

### Tono.GuiWinForm
![](https://aqtono.com/tomarika/tono/TonoGuiWinFormIcon.png)  
A framework of MVFP architecture for WinForm developments with .NET Framework version 4.7.1 
(legacy library. not recommended to use it. This framework does not use Tono.Gui)


Function|Remarks
-|-
Model|**M**VFP : You can implement application data with this class.
View|M**V**FP : UWP control of this GUI architecture.
Feature|MV**F**P : Feature base class like plug-in function mechanism. Feature is possible to be a function, constraint and design controls.
Parts|MVF**P** : Parts base class to be the all graphical implementation of this architecture.
Library|Color, Timer, Configulation, Some UWP controls having MVFP linkage, File, Some useful Forms, 