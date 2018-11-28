using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Reflection;

namespace KellermanSoftware.Common
{
    /// <summary>
    /// Helper methods for objects
    /// </summary>
    public static class ObjectUtil
    {
        private static CompareObjects _cmpShallow = null;
        private static List<string> _elementsToIgnore;

        /// <summary>
        /// Serialize an object to a file using binary serialization
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="myObject"></param>
        /// <param name="filePath"></param>
        public static void SerializeToFile<T>(T myObject, string filePath)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, myObject);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    Compression compression = new Compression();
                    compression.CompressStream(memoryStream, fileStream);
                }
            }
        }

        /// <summary>
        /// Deserialize an object from a file using binary serialization
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T DeserializeFromFile<T>(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    Compression compression = new Compression();
                    compression.DecompressStream(fileStream, memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    var formatter = new BinaryFormatter();
                    return (T)formatter.Deserialize(memoryStream);
                }
            }
        }
        
        /// <summary>
        /// Clone an object using binary serialization
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <returns></returns>
        public static T CloneWithSerialization<T>(T original) where T : class
        {
            T result;
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, original);
                stream.Seek(0, SeekOrigin.Begin);
                result = (T)binaryFormatter.Deserialize(stream);
            }

            return result;
        }

        /// <summary>
        /// Get the XSD Type for a .NET Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetXsdType(Type type)
        {
            bool nullable = false;
            Type underlyingType = type;
            string result = string.Empty;

            if (IsNullableType(type))
            {
                nullable = true;
                underlyingType = Nullable.GetUnderlyingType(type);
            }

            switch (underlyingType.Name)
            {
                case "Char":
                case "String":
                    result = "xs:string";
                    break;
                case "Boolean":
                    result = "xs:boolean";
                    break;
                case "DateTime":
                    result = "xs:dateTime";
                    break; 
                case "Int16":
                    result = "xs:short";
                    break;
                case "Int32":
                    result = "xs:int";
                    break;
                case "Int64":
                    result = "xs:long";
                    break;
                case "Double":
                    result = "xs:double";
                    break;
                case "Decimal":
                    result = "xs:decimal";
                    break;
                case "Single":
                    result = "xs:float";
                    break;
                case "Byte":
                    result = "xs:byte";
                    break;
                default:
                    result = underlyingType.Name;
                    break;
            }

            if (nullable)
                result = result + " nillable=\"true\" minOccurs=\"0\"";

            return result;
        }

        /// <summary>
        /// Get the CSharp Type for the passed in type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetCSharpTypeString(Type type)
        {
            bool nullable = false;
            Type underlyingType = type;
            string result= string.Empty;

            if (IsNullableType(type))
            {
                nullable = true;
                underlyingType = Nullable.GetUnderlyingType(type);
            }

            switch (underlyingType.Name)
            {
                case "String":
                    result = "string";
                    break;
                case "Boolean":
                    result = "bool";
                    break;
                case "Int16":
                    result = "short";
                    break;
                case "Int32":
                    result = "int";
                    break;
                case "Int64":
                    result = "long";
                    break;
                case "Double":
                    result = "double";
                    break;
                case "Decimal":
                    result = "decimal";
                    break;
                case "Single":
                    result = "float";
                    break;
                case "Byte":
                    result = "byte";
                    break;
                case "Byte[]":
                    result = "byte[]";
                    break;
                default:
                    result = underlyingType.Name;
                    break;
            }

            if (nullable)
                result = result + "?";

            return result;
        }

        /// <summary>
        /// Load all types that implement the passed interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> LoadFromAssemblyImplementsInterface<T>()
        {
            List<T> list = new List<T>();

            list.AddRange(
                typeof(T).Assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsGenericType)
                .Where(t => t.GetInterfaces().Any(i => i == typeof(T)))
                .Select(t => (T)Activator.CreateInstance(t)));

            return list;
        }

        /// <summary>
        /// Load all types that are subclasses of the passed class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> LoadFromAssemblyWhereSubclass<T>()
        {
            List<T> list = new List<T>();

            list.AddRange(
                typeof(T).Assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsGenericType)
                .Where(t => t.IsSubclassOf(typeof(T)))
                .Select(t => (T)Activator.CreateInstance(t)));

            return list;
        }

        /// <summary>
        /// Returns true if the type is nullable
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullableType(Type type)
        {
            return (type.IsGenericType &&
                    type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
        }

        /// <summary>
        /// Convert object to an enum value
        /// </summary>
        /// <typeparam name="TConvertTo"></typeparam>
        /// <param name="from"></param>
        /// <returns></returns>
        public static TConvertTo ConvertEnum<TConvertTo>(object from)
        {
            TConvertTo result = default(TConvertTo);

            bool inputIsNull = from == null;
            bool outputIsNullable = IsNullableType(typeof(TConvertTo));

            if (inputIsNull && !outputIsNullable)
            {
                throw new ApplicationException("Cannot convert from null to type that does not support null.");
            }
            Type underlyingResultEnumType;
            if (outputIsNullable)
            {
                NullableConverter converter = new NullableConverter(typeof(TConvertTo));
                underlyingResultEnumType = converter.UnderlyingType;
            }
            else
            {
                underlyingResultEnumType = typeof(TConvertTo);
            }

            string[] inputs = new string[0];
            if (!inputIsNull)
            {
                //string inputAsString = Enum.GetName(underlyingResultEnumType, (int)from);
                string inputAsString = from.ToString();
                inputs = inputAsString.Split(','); // flag enum values need to be split
                object resultAsObject = Enum.Parse(underlyingResultEnumType, inputAsString);
                result = (TConvertTo)resultAsObject;
            }

            if (result == null && from != null)
            {
                throw new ApplicationException("Conversion failed.");
            }
            else if (result != null && inputs.Length > 1)
            {
                // this is a flag enum, so verify that each value is defined
                foreach (string input in inputs)
                {
                    if (!Enum.IsDefined(underlyingResultEnumType, (TConvertTo)Enum.Parse(underlyingResultEnumType, input)))
                        throw new ApplicationException("Conversion failed to find valid value.");
                }
            }
            else if (result != null && !Enum.IsDefined(underlyingResultEnumType, result))
            {
                throw new ApplicationException("Conversion failed to find valid value.");
            }

            return result;
        }
		
        /// <summary>
        /// Get a Description from an enum value or just the enum value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetEnumDescription(Enum value)
        {
            string result= string.Empty;
    
            if (value != null)
            {
                FieldInfo info = value.GetType().GetField(value.ToString());
                DescriptionAttribute[] attributes =
                    (DescriptionAttribute[]) info.GetCustomAttributes(typeof (DescriptionAttribute), false);
                result = (attributes.Length > 0) ? attributes[0].Description : value.ToString();
            }

            return result;
        }


        /// <summary>
        /// Convert an enum to a list of string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<string> EnumToList<T>()
        {
            List<string> list = new List<string>();
            Type enumType = typeof (T);

            foreach (object value in Enum.GetValues(enumType))
            {
                Enum enumValue = value as Enum;
                string description = GetEnumDescription(enumValue);
                list.Add(description);
            }

            list.Sort();

            return list;

        }

        /// <summary>
        /// Convert an enum to a dictionary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Dictionary<string, int> EnumToDictionary<T>() where T : struct, IConvertible
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            foreach (object value in Enum.GetValues(typeof(T)))
            {
                Enum enumValue = value as Enum;

                result.Add(GetEnumDescription(enumValue), (int) value);
            }

            return result;
        }

        /// <summary>
        /// Convert a string or description to an enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumString"></param>
        /// <returns></returns>
        public static T StringToEnum<T>(string enumString)
        {
            Type enumType = typeof(T);

            foreach (object value in Enum.GetValues(enumType))
            {
                Enum enumValue = value as Enum;

                if (enumValue.ToString() == enumString)
                    return (T)value;

                if (GetEnumDescription(enumValue) == enumString)
                {
                    return (T)value;
                }
            }

            string msg = string.Format("Could not map {0} to enum type {1}",enumString,enumType.Name);
            throw new Exception(msg);
        }

        /// <summary>
        /// Clone an object for only the primitive public properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T ShallowClone<T>(T source) where T : class, new()
        {
            InitElementsToIgnore();
            T dest = new T();
            CopyProperties<T,T>(source, dest);
            return dest;
        }

        /// <summary>
        /// Copy a list
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="TD"></typeparam>
        /// <param name="sourceList"></param>
        /// <returns></returns>
        public static List<TD> CopyList<TS,TD>(List<TS> sourceList)
            where TD : class, new()
            where TS : class
        {
            List<TD> destList = new List<TD>();

            foreach (var sourceItem in sourceList)
            {
                TD destItem = new TD();
                CopyProperties(sourceItem, destItem);
                destList.Add(destItem);
            }
            return destList;
        }

        /// <summary>
        /// Copy an array
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="TD"></typeparam>
        /// <param name="sourceArray"></param>
        /// <returns></returns>
        public static TD[] CopyArray<TS,TD>(TS[] sourceArray) 
            where TD : class,new() 
            where TS : class
        {
            List<TD> destList = new List<TD>();

            foreach (var sourceItem in sourceArray)
            {
                TD destItem = new TD();  
                CopyProperties(sourceItem,destItem);
                destList.Add(destItem);
            }
            return destList.ToArray();
        }

        /// <summary>
        /// Copy public properties from one object to another of differing types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public static void CopyProperties<TS,TD>(TS source, TD dest) 
            where TD : class
            where TS : class
        {
            string name= string.Empty;

            if (source == null)
                throw new ArgumentNullException("source");

            if (dest == null)
                throw new ArgumentNullException("dest");

            InitElementsToIgnore();
            Type tsType = typeof (TS);
            Type tdType = typeof (TD);

            try
            {

                PropertyInfo[] tsProperties = tsType.GetProperties(); //Default is public instance

                foreach (PropertyInfo tsInfo in tsProperties)
                {
                    //If we can't read or write it, skip it
                    if (!tsInfo.CanRead || !tsInfo.CanWrite)
                        continue;

                    name = tsInfo.Name;

                    //Skip ignored elements
                    if (_elementsToIgnore.Contains(name))
                        continue;
                    
                    Type tsInfoType = tsInfo.PropertyType;

                    if (tsInfoType.IsPrimitive
                        || tsInfoType.IsEnum
                        || tsInfoType.IsValueType
                        || tsInfoType == typeof(DateTime)
                        || tsInfoType == typeof(decimal)
                        || tsInfoType == typeof(string)
                        || tsInfoType == typeof(Guid)
                        || tsInfoType == typeof(TimeSpan))
                    {
                        object value = tsInfo.GetValue(source, null);

                        PropertyInfo[] tdProperties = tdType.GetProperties();

                        foreach (PropertyInfo tdInfo in tdProperties)
                        {
                            if (tsInfo.Name == tdInfo.Name)
                            {
                                if (value == null)
                                    tdInfo.SetValue(dest, null, null);
                                else
                                    tdInfo.SetValue(dest,
                                        ConvertUtil.ChangeType(value, tdInfo.PropertyType),
                                        null);

                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("{0}; Source Type: {1}, Dest Type: {2}, Property: {3}",
                                           ex.Message, tsType.FullName, tdType.FullName, name);
                throw new Exception(msg);

            }

        }

        /// <summary>
        /// Copy primitive public properties from one object to another of the same type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public static void CopyProperties<T>(T source, T dest) where T : class
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (dest == null)
                throw new ArgumentNullException("dest");

            InitElementsToIgnore();
            Type t = typeof(T);

            PropertyInfo[] currentProperties = t.GetProperties(); //Default is public instance

            foreach (PropertyInfo info in currentProperties)
            {
                //If we can't read or write it, skip it
                if (!info.CanRead || !info.CanWrite)
                    continue;

                if (_elementsToIgnore.Contains(info.Name))
                    continue;  

                Type tInfoType = info.PropertyType;

                if (tInfoType.IsPrimitive
                    || tInfoType.IsEnum
                    || tInfoType.IsValueType
                    || tInfoType == typeof(DateTime)
                    || tInfoType == typeof(decimal)
                    || tInfoType == typeof(string)
                    || tInfoType == typeof(Guid)
                    || tInfoType == typeof(TimeSpan))
                {
                    object value = info.GetValue(source, null);

                    if (value == null)
                        info.SetValue(dest, null, null);
                    else
                        info.SetValue(dest,
                            ConvertUtil.ChangeType(value, info.PropertyType),
                            null);
                }
            }
        }

        /// <summary>
        /// Performs a shallow comparison of the objects but not the child objects
        /// </summary>
        /// <param name="object1"></param>
        /// <param name="object2"></param>
        /// <returns></returns>
        public static bool ShallowCompare(object object1, object object2)
        {
            if (_cmpShallow == null)
            {
                InitElementsToIgnore();
                _cmpShallow = new CompareObjects();
                _cmpShallow.CompareChildren = false;
                _cmpShallow.ElementsToIgnore.AddRange(_elementsToIgnore);
            }

            return _cmpShallow.Compare(object1, object2);
        }

        /// <summary>
        /// Get the hash code for the current object, not accounting for the child objects
        /// </summary>
        /// <param name="object1"></param>
        /// <returns></returns>
        public static int ShallowHashCode(object object1)
        {
            StringBuilder sb = new StringBuilder(4096);
            if (object1 == null)
            {
                return 0;
            }

            InitElementsToIgnore();

            Type t1 = object1.GetType();

            PropertyInfo[] currentProperties = t1.GetProperties();

            foreach (PropertyInfo info in currentProperties)
            {
                if (IsChildType(info.PropertyType))
                    continue;

                if (_elementsToIgnore.Contains(info.Name))
                    continue;   

                object value = info.GetValue(object1, null);

                if (value == null)
                {
                    sb.Append("null");
                }
                else
                {
                    sb.Append(value.ToString());
                }
                sb.Append("-");
            }

            return sb.ToString().GetHashCode();
        }

        /// <summary>
        /// Returns true if the type can have children
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsChildType(Type t)
        {
            return IsClass(t)
                || IsArray(t)
                || IsIDictionary(t)
                || IsIList(t)
                || IsStruct(t);
        }

        /// <summary>
        /// Returns true if the type is a timespan
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsTimespan(Type t)
        {
            return t == typeof(TimeSpan);
        }

        /// <summary>
        /// Returns true if the type is an enum
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsEnum(Type t)
        {
            return t.IsEnum;
        }

        /// <summary>
        /// Returns true if the type is a struct
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsStruct(Type t)
        {
            return t.IsValueType;
        }

        /// <summary>
        /// Returns true if a value is a simple type
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsSimpleType(Type t)
        {
            return t.IsPrimitive
                || t == typeof(DateTime)
                || t == typeof(decimal)
                || t == typeof(string)
                || t == typeof(Guid);

        }

        /// <summary>
        /// Returns true if the type is an array
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsArray(Type t)
        {
            return t.IsArray;
        }

        /// <summary>
        /// Returns true if a type is a class
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsClass(Type t)
        {
            return t.IsClass;
        }

        /// <summary>
        /// Returns true if a type is a dictionary
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsIDictionary(Type t)
        {
            return t.GetInterface("System.Collections.IDictionary", true) != null;
        }

        /// <summary>
        /// Returns true if a type is a list
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsIList(Type t)
        {
            return t.GetInterface("System.Collections.IList", true) != null;
        }

        #region Private Methods
        private static void InitElementsToIgnore()
        {
            if (_elementsToIgnore == null)
            {
                _elementsToIgnore = new List<string>();
                _elementsToIgnore.Add("IsDirty");
                _elementsToIgnore.Add("IsNew");
            }
        }

        #endregion

    }
}
