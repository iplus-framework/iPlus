Dokumentation Methoden (PWMethod...) und Stücklisten (PLPartslist...)
=====================================================================

Grundsätzlich ist der Aufbau von Klassen im Bereich Methoden und Stücklisten weitgehend identisch strukturiert.

Zuerst eine Aufstellung der Klassenarten:

1. Workflow-Klassen
In diesem Kapitel sind alle ACComponent-Klassen zu finden, die später auch zur Ausführung des Workflows
(zumindest beim Methoden) instanziiert werden.

1.1 Root-Workflow-Klassen
Die Root-Workflow-Klassen werden im (grafischen) Workflow (Methode/Stückliste) als äußertes Element platziert. 
Hierdurch wird die Art des Workflowsdefiniert. Im alten Variobatch gab es vier Arten von Methoden 
(Produktion, Einlagerung, Auslagerung, Umlagerung), welche mit verschiedenen Anwendungsdaten (Tabellen) in 
Beziehung standen.

Im neuen System werden diese Arten nun über die Root-Workflow-Klassen differenziert. Jede dieser Klassen hat konkrete
Eigenschaften für den Zugriff auf die Anwendungsdaten, so das untergeordnete Workflow-Klassen möglichst einfach
darauf zugreifen können.

Methoden (ACKinds = TPWMethod)
========
-PWMethodBase				Bassisklasse für alle Implementierungen
-PWMethod	
-PWMethodProduction
-PWMethodIncomming
-PWMethodOutgoing
-PWMethodRearrangement

Stücklisten (ACKinds = TPLPartslist)
===========
-PLPartslistBase			Bassisklasse für alle Implementierungen
-PLPartslistProduction
-PLPartslistPackaging

1.2 Untergeordnete Workflow-Klassen
Innerhalb der Root-Workflow-Klassen können Workflowklassen platziert werden.

Methoden (ACKinds = TPWGroup..TPWNodeEnd)
========
-PWGroup
-PWNodeEnd
-PWNodeOr
-etc.

Stücklisten (ACKinds = TPLPartslistStage..TPLPartslistEnd)
===========
-PLPartslistStage
-PLPartlistStart
-PLPartslistEnd


2. Darstellung und Bearbeitung
Grundsätzlich werden die Workflows in unterschiedlichen BSO´s dargestellt und teilweise auch bearbeitet. Um eine 
hohe Flexibilität zu erreichen, ist in den BSO´s (z.B. BSOProgram, BSOPartslist, BSOProcessControl) nur der Quellcode
implementiert, der notwendig ist, um einen Workflow zu laden oder ggf. einen neuen zu erzeugen.

Die eigentliche Funktionialität ist in verschiedene Unter-ACComponent verteilt, wodurch ein weitgehendes Customizing 
ermöglicht wird. Diese Klassen befinden sich alle in der gip.core.manager.dll.

2.1 Presenter
Die Presenter-ACComponent´s ermöglichen die Darstellung eines grafischen Workflows. Es gibt drei verschiedenen
Presenterimplementierungen unterschieden:
a) VBPresenterMethod
Darstellung von ACClassMethod- und ACProgram-Workflows
b) VBPresenterPartslist
Darstellung von Partslist-Workflows
c) VBPresenterTask
Darstellung von Task mit und ohne ACProgram

2.2 Designer
Die Designer werden zur Editierung des grafischen Workflows verwendet. Es gibt auch eine Implementierung für die
XAML-Editierung.

a) VBDesignerWorkflowMethod
Editierung von ACClassMethod-Workflows
b) VBDesignerWorkflowPartslist
Editierung von Partslist-Workflows
c) VBDesignerXAML
Editierung von XAML. 

2.3 BSO (Abgeleitet von IBSOWorkflow)
Zu jeder unter 1.1 aufgeführten Root-Workflow-Klassen gibt es genaue eine BSO-Implementierung, welche es ermöglicht
einen neuen Workflow zu initialisieren, zu löschen, zu laden und die Anwendungsdaten zu bearbeiten. 

Methoden 
========
-PWBSOMethod				für PWMethod	
-PWBSOMethodProduction		für PWMethodProduction
-PWBSOMethodIncomming		für PWMethodIncomming
-PWBSOMethodOutgoing		für PWMethodOutgoing
-PWBSOMethodRearrangement	für PWMethodRearrangement

Stücklisten 
===========
-PLBSOPartslistProduction		für PLPartslistProduction	
-PLBSOPartslistPackaging		für PLPartslistPackaging	

Für eine weitere Implementierung eines fachlichen Workflows (z.B. Verladung) sind primär also nur eine Root-Workflow-Klasse
(z.B. PWMethodVerladung) und eine BSO-Klasse (z.B. PWBSOMethodVerladung) zu implementieren. Dadurch das die neue Root-Workflow-
Klasse vom ACKinds TPWMethod ist, können im BSOWorkflow direkt Workflows und im BSOProgram direkt Programme dieser Art angelegt 
werden.