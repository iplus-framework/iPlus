using System;

namespace gip.core.datamodel
{
    /// <summary>
    ///   <para>
    /// The IACPropertyBase interface allows type-neutral access to a property if the data type is not known to the application programmer. The object Value property can also be used to access the value via Boxing/Unboxing. However, we recommend that you always work with the generic variant, because this prevents type runtime errors.</para>
    ///   <para>Therefore cast the IACPropertyBase into an IACContainerT&lt;T&gt; reference and always work with it!
    /// </para>
    /// </summary>
    public interface IACPropertyBase : IACMember
    {
        /// <summary>
        /// This method is called automatically inside ACComponent.ACInit() when a new instance is createad.
        /// </summary>
        /// <param name="startChildMode">The start child mode.</param>
        void ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic);


        /// <summary>
        /// This method is called automatically inside ACComponent.ACDeInit() when a new instance is destroyed.
        /// </summary>
        /// <param name="deleteACClassTask">if set to <c>true</c> [delete AC class task].</param>
        void ACDeInit(bool deleteACClassTask = false);


        /// <summary>
        /// The first time you access LiveLog, a property logging mechanism is automatically activated. Every time the property value changes, the new value is written to a ring buffer. You access the values ​​of this ring buffer using the LiveLogList property  . The standard capacity of the ring buffer is 500 values. However, you can change this value in the iPlus development environment in the "Size of the log buffer" field.
        /// </summary>
        /// <value>The live log.</value>
        PropertyLogListInfo LiveLog { get; }


        /// <summary>
        /// Restores the stored value from the database into this persistable property
        /// </summary>
        /// <param name="IsInit">if set to <c>true</c> [is init].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        bool ReStoreFromDB(bool IsInit);


        /// <summary>
        /// Inidcates if this property is currently in the Init-Phase and the persisted value isn't still read from the database.
        /// </summary>
        /// <value><c>true</c> if [in restore phase]; otherwise, <c>false</c>.</value>
        bool InRestorePhase { get; }


        /// <summary>Returns the Value-Property of this insntance as a serialized string (XML).</summary>
        /// <param name="xmlIndented">if set to <c>true</c> the XML is indented.</param>
        /// <returns>XML</returns>
        string ValueSerialized(bool xmlIndented = false);


        /// <summary>iPlus-Type (Metadata) of this property.</summary>
        /// <value>ACClassProperty</value>
        ACClassProperty PropertyInfo { get; }


        /// <summary>.NET-Type of the Value of this property.</summary>
        Type PropertyType { get; }

        bool IsValueType { get; }

        /// <summary>
        /// Checks if Property is in valid Range
        /// </summary>
        Global.ControlModesInfo IsMinMaxValid { get; }


        /// <summary>Writes the current value to the database.
        /// It this property is persistable, then the current value is serialized (invokes ValueSerialized()) and set to the XMLValue-Property.</summary>
        /// <returns>
        ///   <c>true</c> if sucessful</returns>
        bool Persist();


        /// <summary>
        /// Resets the Value-Property to a default value.
        /// </summary>
        void ResetToDefaultValue();
    }
}
