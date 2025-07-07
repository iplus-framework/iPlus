// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACClassInfo.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.datamodel
{

    /// <summary>
    /// Class ACClassInfo
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Enum)]
    public class ACClassInfo : Attribute
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ACClassInfo"/> class.
        /// </summary>
        public ACClassInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACClassInfo"/> class.
        /// </summary>
        /// <param name="acPackageName">Assignment to a package name. The package name is used for software licensing with the end customer.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the class. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="acType">The category serves on the one hand to simplify the use of the class in the development environment by showing or hiding certain classes depending on the context in which the class is needed, and on the other hand to program for project-specific logics to handle classes that are not yet instantiated at that time.</param>
        /// <param name="acStorableType">Specifies whether the state of this instance (perilizable properties) may be saved or not. This only works for classes instantiated in the application tree. Classes instantiated in the root project are usually not perilizable. Multiple instantiatable classes are derivatives of ACBSO (business objects) in the rules. In this case, isMultiInstance must have the value True.</param>
        /// <param name="isMultiInstance">Indicates whether the class can be instantiated more than once as a child object. Since the ACIdentifier must be unique in the member list/child list, the iPlus runtime automatically assigns a sequential instance number when a new instance has to be created. This instance number is appended to the ACIdentifier string by means of brackets. Example: "BSOMaterial(1), BSOMaterial(2)".</param>
        /// <param name="isRightmanagement">Flag whether this class is responsible for rights management and whether rights for properties, methods and designs can be assigned in group management.</param>
        /// <param name="acClassChilds">Array to define relationships to other entity objects.</param>
        /// <param name="qryConfig">Only relevant for classes that are derivatives of ACBSONav (Navigable Business Objects). qryConfig specifies the name of the ACQuery definition to be loaded by default to filter the records displayed in Explorer. You can use the iPlus development environment to look up which queries exist in the path \iPlusMES\Queries. New queries are automatically entered by declaring the ACQueryInfoPrimary attribute using an entity class and then starting the program with Ctrl-Login.</param>
        /// <param name="bsoConfig">Only relevant for classes that are entity classes. bsoConfig specifies the name of the business object class that is responsible for managing this entity class. If this entity class is displayed in an "item control" such as a combo box, you can use the context menu to navigate to the relevant business object and display or manage the corresponding entity.</param>
        /// <param name="sortIndex">Sort sequence for presenting the class in the iPlus development environment.</param>
        public ACClassInfo(string acPackageName, string acCaptionTranslation, Global.ACKinds acType, Global.ACStorableTypes acStorableType, bool isMultiInstance, bool isRightmanagement, object[] acClassChilds, string qryConfig = "", string bsoConfig = "", Int16 sortIndex = 9999) :
            this(acPackageName, acCaptionTranslation, acType, acStorableType, isMultiInstance, isRightmanagement, qryConfig, bsoConfig, sortIndex)
        {
            InitChilds(acClassChilds);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACClassInfo"/> class.
        /// </summary>
        /// <param name="acPackageName">Assignment to a package name. The package name is used for software licensing with the end customer.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the class. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="acType">The category serves on the one hand to simplify the use of the class in the development environment by showing or hiding certain classes depending on the context in which the class is needed, and on the other hand to program for project-specific logics to handle classes that are not yet instantiated at that time.</param>
        /// <param name="acStorableType">Specifies whether the state of this instance (perilizable properties) may be saved or not. This only works for classes instantiated in the application tree. Classes instantiated in the root project are usually not perilizable. Multiple instantiatable classes are derivatives of ACBSO (business objects) in the rules. In this case, isMultiInstance must have the value True.</param>
        /// <param name="isMultiInstance">Indicates whether the class can be instantiated more than once as a child object. Since the ACIdentifier must be unique in the member list/child list, the iPlus runtime automatically assigns a sequential instance number when a new instance has to be created. This instance number is appended to the ACIdentifier string by means of brackets. Example: "BSOMaterial(1), BSOMaterial(2)".</param>
        /// <param name="isRightmanagement">Flag whether this class is responsible for rights management and whether rights for properties, methods and designs can be assigned in group management.</param>
        /// <param name="qryConfig">Only relevant for classes that are derivatives of ACBSONav (Navigable Business Objects). qryConfig specifies the name of the ACQuery definition to be loaded by default to filter the records displayed in Explorer. You can use the iPlus development environment to look up which queries exist in the path \iPlusMES\Queries. New queries are automatically entered by declaring the ACQueryInfoPrimary attribute using an entity class and then starting the program with Ctrl-Login.</param>
        /// <param name="bsoConfig">Only relevant for classes that are entity classes. bsoConfig specifies the name of the business object class that is responsible for managing this entity class. If this entity class is displayed in an "item control" such as a combo box, you can use the context menu to navigate to the relevant business object and display or manage the corresponding entity.</param>
        /// <param name="sortIndex">Sort sequence for presenting the class in the iPlus development environment.</param>
        public ACClassInfo(string acPackageName, string acCaptionTranslation, Global.ACKinds acType, Global.ACStorableTypes acStorableType = Global.ACStorableTypes.NotStorable, bool isMultiInstance = false, bool isRightmanagement = false, string qryConfig = "", string bsoConfig = "", Int16 sortIndex = 9999)
        {
            ACPackageName = acPackageName;
            ACCaptionTranslation = acCaptionTranslation;
            ACKind = acType;
            ACStorableType = acStorableType;
            IsMultiInstance = isMultiInstance;
            IsRightmanagement = isRightmanagement;
            QRYConfig = qryConfig;
            BSOConfig = bsoConfig;
            SortIndex = sortIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACClassInfo"/> class.
        /// </summary>
        /// <param name="acPackageName">Assignment to a package name. The package name is used for software licensing with the end customer.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the class. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="acType">The category serves on the one hand to simplify the use of the class in the development environment by showing or hiding certain classes depending on the context in which the class is needed, and on the other hand to program for project-specific logics to handle classes that are not yet instantiated at that time.</param>
        /// <param name="acStorableType">Specifies whether the state of this instance (perilizable properties) may be saved or not. This only works for classes instantiated in the application tree. Classes instantiated in the root project are usually not perilizable. Multiple instantiatable classes are derivatives of ACBSO (business objects) in the rules. In this case, isMultiInstance must have the value True.</param>
        /// <param name="isMultiInstance">Indicates whether the class can be instantiated more than once as a child object. Since the ACIdentifier must be unique in the member list/child list, the iPlus runtime automatically assigns a sequential instance number when a new instance has to be created. This instance number is appended to the ACIdentifier string by means of brackets. Example: "BSOMaterial(1), BSOMaterial(2)".</param>
        /// <param name="pwInfoACClass">Only relevant for classes that are derivatives of PAProcessFunction. pwInfoACClass specifies the class name of the workflow class (a derivative of PWNodeProcessMethod) that is compatible with this PAProcessFunction. Other wording: Specifies the workflow class that can call this function and knows its parameter list.</param>
        /// <param name="isRightmanagement">Flag whether this class is responsible for rights management and whether rights for properties, methods and designs can be assigned in group management.</param>
        /// <param name="acClassChilds">Array to define relationships to other entity objects.</param>
        /// <param name="qryConfig">Only relevant for classes that are derivatives of ACBSONav (Navigable Business Objects). qryConfig specifies the name of the ACQuery definition to be loaded by default to filter the records displayed in Explorer. You can use the iPlus development environment to look up which queries exist in the path \iPlusMES\Queries. New queries are automatically entered by declaring the ACQueryInfoPrimary attribute using an entity class and then starting the program with Ctrl-Login.</param>
        /// <param name="bsoConfig">Only relevant for classes that are entity classes. bsoConfig specifies the name of the business object class that is responsible for managing this entity class. If this entity class is displayed in an "item control" such as a combo box, you can use the context menu to navigate to the relevant business object and display or manage the corresponding entity.</param>
        /// <param name="sortIndex">Sort sequence for presenting the class in the iPlus development environment.</param>
        public ACClassInfo(string acPackageName, string acCaptionTranslation, Global.ACKinds acType, Global.ACStorableTypes acStorableType, bool isMultiInstance, string pwInfoACClass, bool isRightmanagement, object[] acClassChilds, string qryConfig = "", string bsoConfig = "", Int16 sortIndex = 9999) :
            this(acPackageName, acCaptionTranslation, acType, acStorableType, isMultiInstance, pwInfoACClass, isRightmanagement, qryConfig, bsoConfig, sortIndex)
        {
            InitChilds(acClassChilds);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACClassInfo"/> class.
        /// </summary>
        /// <param name="acPackageName">Assignment to a package name. The package name is used for software licensing with the end customer.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the class. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="acType">The category serves on the one hand to simplify the use of the class in the development environment by showing or hiding certain classes depending on the context in which the class is needed, and on the other hand to program for project-specific logics to handle classes that are not yet instantiated at that time.</param>
        /// <param name="acStorableType">Specifies whether the state of this instance (perilizable properties) may be saved or not. This only works for classes instantiated in the application tree. Classes instantiated in the root project are usually not perilizable. Multiple instantiatable classes are derivatives of ACBSO (business objects) in the rules. In this case, isMultiInstance must have the value True.</param>
        /// <param name="isMultiInstance">Indicates whether the class can be instantiated more than once as a child object. Since the ACIdentifier must be unique in the member list/child list, the iPlus runtime automatically assigns a sequential instance number when a new instance has to be created. This instance number is appended to the ACIdentifier string by means of brackets. Example: "BSOMaterial(1), BSOMaterial(2)".</param>
        /// <param name="pwInfoACClass">Only relevant for classes that are derivatives of PAProcessFunction. pwInfoACClass specifies the class name of the workflow class (a derivative of PWNodeProcessMethod) that is compatible with this PAProcessFunction. Other wording: Specifies the workflow class that can call this function and knows its parameter list.</param>
        /// <param name="isRightmanagement">Flag whether this class is responsible for rights management and whether rights for properties, methods and designs can be assigned in group management.</param>
        /// <param name="qryConfig">Only relevant for classes that are derivatives of ACBSONav (Navigable Business Objects). qryConfig specifies the name of the ACQuery definition to be loaded by default to filter the records displayed in Explorer. You can use the iPlus development environment to look up which queries exist in the path \iPlusMES\Queries. New queries are automatically entered by declaring the ACQueryInfoPrimary attribute using an entity class and then starting the program with Ctrl-Login.</param>
        /// <param name="bsoConfig">Only relevant for classes that are entity classes. bsoConfig specifies the name of the business object class that is responsible for managing this entity class. If this entity class is displayed in an "item control" such as a combo box, you can use the context menu to navigate to the relevant business object and display or manage the corresponding entity.</param>
        /// <param name="sortIndex">Sort sequence for presenting the class in the iPlus development environment.</param>
        public ACClassInfo(string acPackageName, string acCaptionTranslation, Global.ACKinds acType, Global.ACStorableTypes acStorableType, bool isMultiInstance, string pwInfoACClass, bool isRightmanagement, string qryConfig = "", string bsoConfig = "", Int16 sortIndex = 9999)
        {
            ACPackageName = acPackageName;
            ACCaptionTranslation = acCaptionTranslation;
            ACKind = acType;
            ACStorableType = acStorableType;
            IsMultiInstance = isMultiInstance;
            PWInfoACClass = pwInfoACClass;
            IsRightmanagement = isRightmanagement;
            QRYConfig = qryConfig;
            BSOConfig = bsoConfig;
            SortIndex = sortIndex;
        }

        /// <summary>
        /// Initializes the childs that are handed over in the array.
        /// </summary>
        /// <param name="acClassChilds">Array to define relationships to other entity objects.</param>
        /// <exception cref="System.Exception"></exception>
        private void InitChilds(object[] acClassChilds)
        {
            if (acClassChilds == null)
                return;
            foreach (var child in acClassChilds)
            {
                object[] ochild = child as object[];
                switch (ochild.Count())
                {
                    case 3:
                        ACClassChild acClassChild = new ACClassChild(ochild[0] as string, ochild[1] as string, ochild[2] as string);
                        if (ACClassChilds == null)
                            ACClassChilds = new List<ACClassChild>();
                        ACClassChilds.Add(acClassChild);
                        break;
                    case 4:
                        ACClassChild acClassChild1 = new ACClassChild(ochild[0] as string, ochild[1] as string, ochild[2] as string, ochild[3] as object[]);
                        if (ACClassChilds == null)
                            ACClassChilds = new List<ACClassChild>();
                        ACClassChilds.Add(acClassChild1);
                        break;
                    default:
                        throw new Exception();
                }
            }
        }

        #region public Member
        /// <summary>
        /// Gets or sets the name of the AC package.
        /// </summary>
        /// <value>The name of the AC package.</value>
        public string ACPackageName { get; set; }

        /// <summary>
        /// Gets or sets the AC caption translation.
        /// </summary>
        /// <value>The AC caption translation.</value>
        public string ACCaptionTranslation { get; set; }

        /// <summary>
        /// Description of what the class does and how it should be used. It primarily serves to help understand language models when using the MCP API.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the kind of the AC.
        /// </summary>
        /// <value>The kind of the AC.</value>
        public Global.ACKinds ACKind { get; set; }

        /// <summary>
        /// Gets or sets the type of the AC storable.
        /// </summary>
        /// <value>The type of the AC storable.</value>
        public Global.ACStorableTypes ACStorableType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is multi instance.
        /// </summary>
        /// <value><c>true</c> if this instance is multi instance; otherwise, <c>false</c>.</value>
        public bool IsMultiInstance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is rightmanagement.
        /// </summary>
        /// <value><c>true</c> if this instance is rightmanagement; otherwise, <c>false</c>.</value>
        public bool IsRightmanagement { get; set; }

        /// <summary>
        /// Gets or sets the QRY config.
        /// </summary>
        /// <value>The QRY config.</value>
        public string QRYConfig { get; set; }

        /// <summary>
        /// Gets or sets the BSO config.
        /// </summary>
        /// <value>The BSO config.</value>
        public string BSOConfig { get; set; }

        /// <summary>
        /// Gets or sets the PW info AC class.
        /// </summary>
        /// <value>The PW info AC class.</value>
        public string PWInfoACClass { get; set; }

        /// <summary>
        /// Gets or sets the AC class childs.
        /// </summary>
        /// <value>The AC class childs.</value>
        public List<ACClassChild> ACClassChilds
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index of the sort.
        /// </summary>
        /// <value>The index of the sort.</value>
        public Int16 SortIndex { get; set; }
        #endregion
    }

    /// <summary>
    /// Class ACClassChild
    /// </summary>
    public class ACClassChild
    {
        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="ACClassChild"/> class.
        /// </summary>
        /// <param name="acNameIdentifier">Unique name of the component.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the class. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="baseAssemblyQualifiedName">The assembly-qualified name of the Type, which includes the name of the assembly from which the Type was loaded.</param>
        /// <param name="acClassChilds">Array to define relationships to other entity objects.</param>
        /// <exception cref="System.Exception"></exception>
        public ACClassChild(string acNameIdentifier, string acCaptionTranslation, string baseAssemblyQualifiedName, object[] acClassChilds = null)
        {
            ACIdentifier = acNameIdentifier;
            ACCaptionTranslation = acCaptionTranslation;
            BaseAssemblyQualifiedName = baseAssemblyQualifiedName;
            if (acClassChilds == null)
                return;
            foreach (var child in acClassChilds)
            {
                object[] ochild = child as object[];
                switch (ochild.Count())
                {
                    case 3:
                        ACClassChild acClassChild = new ACClassChild(ochild[0] as string, ochild[1] as string, ochild[2] as string);
                        if (ACClassChilds == null)
                            ACClassChilds = new List<ACClassChild>();
                        ACClassChilds.Add(acClassChild);
                        break;
                    case 4:
                        ACClassChild acClassChild1 = new ACClassChild(ochild[0] as string, ochild[1] as string, ochild[2] as string, ochild[3] as object[]);
                        if (ACClassChilds == null)
                            ACClassChilds = new List<ACClassChild>();
                        ACClassChilds.Add(acClassChild1);
                        break;
                    default:
                        throw new Exception();
                }
            }
        }
        #endregion

        #region public Member
        /// <summary>
        /// Gets or sets the unique name of the component.
        /// </summary>
        /// <value>The AC identifier.</value>
        public string ACIdentifier { get; set; }
        /// <summary>
        /// Gets or sets the AC caption translation.
        /// </summary>
        /// <value>The AC caption translation.</value>
        public string ACCaptionTranslation { get; set; }
        /// <summary>
        /// Gets or sets the name of the base assembly qualified.
        /// </summary>
        /// <value>The name of the base assembly qualified.</value>
        public string BaseAssemblyQualifiedName { get; set; }

        /// <summary>
        /// Gets or sets the AC class childs.
        /// </summary>
        /// <value>The AC class childs.</value>
        public List<ACClassChild> ACClassChilds
        {
            get;
            set;
        }
        #endregion
    }

    //[ACClassInfo(Const.PackName_VarioAutomation, "en{'Scanner'}de{'Scanner'}", Global.ACKinds.TPAEquipment, false, true)]

    // Beispiel
    //[ACClassInfo(Const.PackName_VarioAutomation, "en{'Engine'}de{'Motor'}", Global.ACKinds.TPAEquipment, false, true, new object[] 
    //     {
    //         new object[] {"Sensor1", "en{'Sensor1'}de{'Sensor1'}", "gip.core.processapplication.PAESensorAnalog"},
    //         new object[] {"Sensor2", "en{'Sensor2'}de{'Sensor2'}", "gip.core.processapplication.PAESensorAnalog", new object[]
    //            {
    //                new object[] {"Sensor2_1", "en{'Sensor2_1'}de{'Sensor2_1'}", "gip.core.processapplication.PAESensorDigital"}
    //            }
    //        }
    //     }
    //)]
}
