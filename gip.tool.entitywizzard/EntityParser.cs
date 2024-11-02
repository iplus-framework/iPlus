// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace EntityWizzard
{
    class EntityParser
    {
        XDocument _XDocument;
        XElement _StorageModels = null;
        XElement _ConceptualModels = null;
        XElement _Mappings = null;

        public void UpdateMapping(string path)
        {
            File.Copy(path, path + ".bak",true);
            _XDocument = XDocument.Load(path);

            FindModels();

            if (_StorageModels == null || _ConceptualModels == null || _Mappings == null)
            {
                throw new Exception("Nicht alle Models wurden gefunden!");
            }

            CheckAllNavigationProperties();
            _XDocument.Save(path);
        }

        void FindModels()
        {
            foreach (var element in _XDocument.Elements())
            {
                foreach (var element2 in element.Elements())
                {
                    if (element2.Name == "NavigationProperty")
                    {
                        ;
                    }
                    foreach (var element3 in element2.Elements())
                    {
                        if (element3.Name.LocalName == "StorageModels")
                        {
                            _StorageModels = element3;
                        }
                        if (element3.Name.LocalName == "ConceptualModels")
                        {
                            _ConceptualModels = element3;
                        }
                        if (element3.Name.LocalName == "Mappings")
                        {
                            _Mappings = element3;
                        }
                    }
                }
            }
        }

        void CheckAllNavigationProperties()
        {
            XElement schema = null;
            // Schema suchen
            foreach (var schema1 in _ConceptualModels.Elements())
            {
                if (schema1.Name.LocalName == "Schema")
                {
                    schema = schema1;
                    break;
                }
            }
            if ( schema == null )
                return;

            foreach(var entityType in schema.Elements())
            {

                if ( entityType.Name.LocalName == "EntityType" )
                {
                    string entityName = (from c in entityType.Attributes()
                                         where c.Name.LocalName == "Name"
                                         select c.Value).First();
                    ;
                    foreach (var navProperty in entityType.Elements())
                    {
                        if (navProperty.Name.LocalName == "NavigationProperty")
                        {
                            CheckNavigationProperty(entityName, navProperty);
                        }
                        else if (navProperty.Name.LocalName == "Property")
                        {
                            short isRowVersion = 0;
                            XAttribute xAttribConc = null;
                            foreach (XAttribute xAttribute in navProperty.Attributes())
                            {
                                if ((xAttribute.Name == "Name" && xAttribute.Value == "RowVersion")
                                    || (xAttribute.Name == "Type" && xAttribute.Value == "Binary"))
                                    isRowVersion++;
                                else if (xAttribute.Name == "ConcurrencyMode")
                                    xAttribConc = xAttribute;
                            }
                            if (isRowVersion == 2)
                            {
                                if (xAttribConc == null)
                                    navProperty.Add(new XAttribute("ConcurrencyMode","Fixed"));
                                else
                                    xAttribConc.Value = "Fixed";
                            }
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityName">Das ist die Entität deren NavProperties zu untersuchen sind</param>
        /// <param name="navProperty"></param>
        void CheckNavigationProperty(string entityName, XElement navProperty)
        {
            string relationship = "";
            XAttribute name = null;     // Der Name muß geändert werden
            string fromRole = "";
            string toRole = "";
            foreach(var attribute in navProperty.Attributes())
            {
                if ( attribute.Name == "Name" )
                {
                    name = attribute;
                }
                if ( attribute.Name == "Relationship" )
                {
                    relationship = attribute.Value;
                }
                if (attribute.Name == "FromRole")
                {
                    fromRole = attribute.Value;
                }
                if (attribute.Name == "ToRole")
                {
                    toRole = attribute.Value;
                }
            }
            if ( name == null || relationship == "" || fromRole == "" || toRole == "" )
            {
                throw new Exception("Ungültiges NavigationProperty");
            }

            // Das Constraint ist die Verknüpfung zum Mapping
            string constraint = relationship;
            int pos1 = constraint.IndexOf('.');
            if (pos1 != -1)
            {
                constraint = constraint.Substring(pos1+1);
            }

            XElement associationSetMapping = GetAssociationSetMapping(constraint);
            string principalRole = "";
            string principalRef = "";
            string dependentRole = "";
            string dependentRef = "";

            foreach (var element in associationSetMapping.Elements())
            {
                if (element.Name.LocalName != "ReferentialConstraint")
                {
                    continue;
                }
                foreach (var element2 in element.Elements())
                {
                    if (element2.Name.LocalName == "Principal")
                    {
                        principalRole = (from c in element2.Attributes()
                                         where c.Name == "Role"
                                         select c).First().Value;

                        var principal = (from c in element2.Elements()
                                         where c.Name.LocalName == "PropertyRef"
                                         select c).First();
                        principalRef = (from c in principal.Attributes()
                                        where c.Name == "Name"
                                        select c).First().Value;
                    }
                    if (element2.Name.LocalName == "Dependent")
                    {
                        dependentRole = (from c in element2.Attributes()
                                         where c.Name == "Role"
                                         select c).First().Value;
                        var dependent = (from c in element2.Elements()
                                        where c.Name.LocalName == "PropertyRef"
                                        select c).First();
                        dependentRef = (from c in dependent.Attributes()
                                        where c.Name == "Name"
                                        select c).First().Value;
                    }
                }
            }
            if (principalRole == "" || principalRef == "" || dependentRole == "" || dependentRef == "")
            {
                throw new Exception("Keine gültige \"Association\" vorhanden!");
            }

            if ((fromRole == "ACClassConfig" || fromRole == "ACClassConfig1") && (toRole == "ACClassConfig" || toRole == "ACClassConfig1"))
            {
                ;
            }

            string column = dependentRef;
            if (column.EndsWith("ID"))
            {
                column = column.Substring(0, column.Length - 2);
            }

            // Sonderfall rekursiver Verweis auf sich selbst
            if (fromRole == toRole + "1" || fromRole + "1" == toRole)
            {
                name.Value = fromRole + "_" + column;
            }
            else
            {
                // Hier nun die entscheidende Zuweisung
                if (entityName == principalRole)
                {
                    name.Value = dependentRole + "_" + column;
                }
                else
                {
                    name.Value = column;
                }
            }
            name.Value = name.Value.Replace("Solution", "");
        }

        XElement GetAssociationSetMapping(string constraint)
        {
            foreach (var element in _StorageModels.Elements())
            {
                if (element.Name.LocalName != "Schema")
                    continue;
                foreach (var element2 in element.Elements())
                {
                    if (element2.Name.LocalName != "Association")
                        continue;
                    var x = (from y in element2.Attributes()
                            where y.Name == "Name" 
                            select y).First();
                    if (x.Value == constraint)
                    {
                        return element2;
                    }
                }
                return null;
            }


//                Mapping
//                EntityContainerMapping
//                AssociationSetMapping

            return null;
        }


    }
}
