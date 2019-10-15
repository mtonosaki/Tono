# Tono - Tools Of New Operation

"Tools of new operation" library/framework is for agile SoE(SoI) style development.   
Expected a lot of developer create original simulators quickly and find business core rule of improvement continuously. That is why this is named "tools of new operation".

## Libraries

### Tono.Gui.Jit
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