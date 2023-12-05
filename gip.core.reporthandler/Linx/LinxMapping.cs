using System;
using System.Linq;
using System.Reflection;

namespace gip.core.reporthandler
{
    public class LinxMapping<T> where T : class
    {

        public static T Map(byte[] data)
        {
            T item = null;

            var attrs =
                typeof(T)
                .GetProperties()
                .Select(c => new { MapAttribute = (LinxByteMappingAttribute)c.GetCustomAttributes(typeof(LinxByteMappingAttribute)).FirstOrDefault(), Property = c })
                .ToArray();

            if (attrs != null && attrs.Any())
            {
                int completeLength = attrs.Select(c => c.MapAttribute.Length).Sum();
                if (completeLength >= data.Length)
                {
                    item = Activator.CreateInstance<T>();
                    foreach (var attr in attrs)
                    {
                        Console.WriteLine(attr.MapAttribute.Order);
                        //byte[] data = new byte[dataWithCheckSum.Length - 2];
                        //byte[] inputChecksum = new byte[2];

                        //Array.Copy(dataWithCheckSum, 0, data, 0, dataWithCheckSum.Length - 2);
                        //Array.Copy(dataWithCheckSum, 0, inputChecksum, dataWithCheckSum.Length - 2 - 1, 2);

                        byte[] dataPart = new byte[attr.MapAttribute.Length];
                        int sourceIndex = 0;
                        foreach (var tempAttr in attrs)
                        {
                            if (tempAttr.MapAttribute.Order < attr.MapAttribute.Order)
                            {
                                sourceIndex += tempAttr.MapAttribute.Length;
                            }
                        }

                        Array.Copy(data, sourceIndex, dataPart, 0, attr.MapAttribute.Length);

                        object value = null;
                        if (attr.MapAttribute.Length == 2)
                        {
                            value = BitConverter.ToInt16(dataPart, 0);
                        }
                        else if (attr.MapAttribute.Length == 4)
                        {
                            value = BitConverter.ToInt32(dataPart, 0);
                        }

                        attr.Property.SetValue(item, value, null);
                    }

                }
            }

            return item;
        }
    }
}
