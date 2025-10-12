using System;
using System.Collections.Generic;
using System.Reflection;

namespace NutsLib;

public static class AttributeUtilities
{
    private static Dictionary<string, List<Type>> ClassAnnotations { get; } = [];

    public static (Type, T)[] GetAllAnnotatedClasses<T>() where T : Attribute
    {
        static void AddType(Type type, Type attribute)
        {
            if (!ClassAnnotations.TryGetValue(attribute.Name, out List<Type>? list))
            {
                list = [];
                ClassAnnotations[attribute.Name] = list;
            }

            list.Add(type);
        }

        // Retrieve all annotations if not done yet. Hopefully after loading everything.
        if (ClassAnnotations.Count == 0)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        try
                        {
                            ClassAttribute[] classAttributes = (ClassAttribute[])type.GetCustomAttributes(typeof(ClassAttribute), false);

                            if (classAttributes.Length > 0)
                            {
                                foreach (ClassAttribute attribute in classAttributes)
                                {
                                    AddType(type, attribute.GetType());
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error loading types from assembly {assembly.FullName}. {e}");
                }
            }

            // Sort the lists by their type name alphabetically.
            foreach (KeyValuePair<string, List<Type>> pair in ClassAnnotations)
            {
                pair.Value.Sort((a, b) => a.Name.CompareTo(b.Name));
            }
        }

        if (ClassAnnotations.TryGetValue(typeof(T).Name, out List<Type>? list))
        {
            (Type, T)[] result = new (Type, T)[list.Count];

            int index = 0;
            foreach (Type type in list)
            {
                T? attribute = (T?)type.GetCustomAttribute(typeof(T)) ?? throw new Exception($"{type.Name} has an invalid attribute of type {typeof(T).Name}!");
                result[index++] = (type, attribute);
            }

            return result;
        }

        return Array.Empty<(Type, T)>();
    }

    public static void ReloadAttributes()
    {
        ClassAnnotations.Clear();
    }
}