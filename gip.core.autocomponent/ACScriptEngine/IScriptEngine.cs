using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{

    public interface IScriptEngine 
    {
        /// <summary>
        /// Registrierung eines Assemblies, das zur Ausführung des Scripts notwendig ist
        /// </summary>
        /// <param name="useAssembly"></param>
        void RegisterAssembly(string useAssembly);

        /// <summary>
        /// Registrierung eines Assemblies aufheben
        /// </summary>
        /// <param name="useAssembly"></param>
        void UnregisterAssembly(string useAssembly);

        /// <summary>
        /// Registrierung eines Namespaces, damit die entsprechenden Klassen verwendet werden können
        /// </summary>
        /// <param name="useNamespace"></param>
        void RegisterNamespace(string useNamespace);

        /// <summary>
        /// Registrierung eines Namespaces aufheben
        /// </summary>
        /// <param name="useNamespace"></param>
        void UnregisterNamespace(string useNamespace);


        /// <summary>Registrieren einer normalen Funktion.
        /// Die Funktionssignatur ist im "sourcecode" enthalten.
        /// Die aufrufende Stelle ist für die richtige Parametrisierung verantwortlich.</summary>
        /// <param name="acMethodName"></param>
        /// <param name="sourcecode"></param>
        /// <param name="continueByError"></param>
        void RegisterScript(string acMethodName, string sourcecode, bool continueByError);

        /// <summary>Registrierung eines Scripts aufheben</summary>
        /// <param name="acMethodName"></param>
        void UnregisterScript(string acMethodName);

        /// <summary>Existiert das Script</summary>
        /// <param name="acMethodName"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        bool ExistsScript(string acMethodName);

        /// <summary>Ausführen eines Scripts
        /// Hier darf es einen beliebigen Rückgabetyp geben, der von
        /// object auf den konkreten Typ zu casten ist</summary>
        /// <param name="acMethodName"></param>
        /// <param name="parameters"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        object Execute(string acMethodName, object[] parameters);

        /// <summary>
        /// Führt alle Scripte aus, die für das Event registriert sind
        /// Diese Funktion gibt nur einen bool zurück, weil es ansonsten
        /// bei den Scripten zu uneinheitlichem Handling kommt.
        /// Alternativ kann auch eine Exception geworfen werden,
        /// wenn es zu einem harten Fehler kommt.
        /// </summary>
        /// <param name="triggerType"></param>
        /// <param name="methodNamePostFix"></param>
        /// <param name="parameters"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        object TriggerScript(ScriptTrigger.Type triggerType, String methodNamePostFix, object[] parameters);

        /// <summary>
        /// Liste der Compileerrors
        /// </summary>
        List<Msg> CompileErrors { get; }
    }
}
