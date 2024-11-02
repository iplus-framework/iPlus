// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace gip.core.reporthandlerwpf
{
    public class LinxMapping<T> where T : class
    {
        public static (MsgWithDetails, T) Map(byte[] data)
        {
            T item = null;
            MsgWithDetails msgWithDetails = new MsgWithDetails();
            List<MappingAttributeWrapper> attrs =
                typeof(T)
                .GetProperties()
                .Select(c => new MappingAttributeWrapper()
                {
                    PropertyInfo = c,
                    LinxByteMapping = (LinxByteMappingAttribute)c.GetCustomAttributes(typeof(LinxByteMappingAttribute)).FirstOrDefault()
                })
                .Where(c => c.LinxByteMapping != null)
                .ToList();

            if (attrs != null && attrs.Any())
            {
                int expectedLength = attrs.Select(c => c.LinxByteMapping.Length).Sum();
                if (expectedLength <= data.Length)
                {
                    item = Activator.CreateInstance<T>();
                    foreach (var attr in attrs)
                    {
                        Console.WriteLine(attr.LinxByteMapping.Order);
                        //byte[] data = new byte[dataWithCheckSum.Length - 2];
                        //byte[] inputChecksum = new byte[2];

                        //Array.Copy(dataWithCheckSum, 0, data, 0, dataWithCheckSum.Length - 2);
                        //Array.Copy(dataWithCheckSum, 0, inputChecksum, dataWithCheckSum.Length - 2 - 1, 2);

                        byte[] dataPart = new byte[attr.LinxByteMapping.Length];
                        int sourceIndex = 0;
                        foreach (var tempAttr in attrs)
                        {
                            if (tempAttr.LinxByteMapping.Order < attr.LinxByteMapping.Order)
                            {
                                sourceIndex += tempAttr.LinxByteMapping.Length;
                            }
                        }

                        Array.Copy(data, sourceIndex, dataPart, 0, attr.LinxByteMapping.Length);

                        object value = 0;
                        if (attr.LinxByteMapping.Length == 1)
                        {
                            value = dataPart[0];
                        }
                        else if (attr.LinxByteMapping.Length == 2)
                        {
                            value = BitConverter.ToInt16(dataPart, 0);
                        }
                        else if (attr.LinxByteMapping.Length == 4)
                        {
                            value = BitConverter.ToInt32(dataPart, 0);
                        }

                        if (attr.LinxByteMapping._DefaultValue != null)
                        {
                            if (attr.LinxByteMapping._DefaultValue.Value != int.Parse(value.ToString()))
                            {
                                Msg badDefaultValue = new Msg()
                                {
                                    MessageLevel = eMsgLevel.Error,
                                    Message = $"For element {typeof(T).Name}.{attr.PropertyInfo.Name} default value is{attr.LinxByteMapping._DefaultValue.Value}. Value {value} loaded!"
                                };
                                msgWithDetails.AddDetailMessage(badDefaultValue);
                            }
                        }

                        attr.PropertyInfo.SetValue(item, value, null);
                    }
                }
                else
                {
                    Msg badLengthErr = new Msg()
                    {
                        MessageLevel = eMsgLevel.Error,
                        Message = $"Message length not match! Expected length:{expectedLength} Actual length: {data.Length}"
                    };
                    msgWithDetails.AddDetailMessage(badLengthErr);
                    msgWithDetails.MessageLevel = eMsgLevel.Error;
                }
            }

            return (msgWithDetails, item);
        }
    }
}
