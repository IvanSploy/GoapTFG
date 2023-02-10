using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;

public static class PropertyManager
{
    //CONFIGURACIÓN DE LAS PROPIEDADES DEL EJEMPLO
    [System.Serializable]
    public enum PropertyList {
        WoodCount,
        StoneCount,
        GoldCount,
        IsAlive,
        Target
    }

    private static Dictionary<PropertyList, PropertyType> ProperTypes = new()
    {
        { PropertyList.WoodCount, PropertyType.Integer },
        { PropertyList.StoneCount, PropertyType.Integer },
        { PropertyList.GoldCount, PropertyType.Float },
        { PropertyList.IsAlive, PropertyType.Boolean },
        { PropertyList.Target, PropertyType.String }
    };
    
    //LÓGICA INTERNA DEL PROGRAMA
    [System.Serializable]
    public struct Property {
        public PropertyList name;
        public string value;

        public Property(PropertyList name, string value)
        {
            this.name = name;
            this.value = value;
        }
    }
    
    [System.Serializable]
    private enum PropertyType
    {
        Boolean = 0,
        Integer = 1,
        Float = 2,
        String = 3
    }

    private static PropertyType GetType(PropertyList property)
    {
        return ProperTypes[property];
    }
    
    private static object ParseValue(PropertyType propertyType, string value)
    {
        object result;
        switch (propertyType)
        {
            case PropertyType.Boolean:
                try
                {
                    result = bool.Parse(value);
                }
                catch (System.FormatException e)
                {
                    Debug.LogError(e.Message);
                    result = false;
                }
                break;
            case PropertyType.Integer:
                try
                {
                    result = int.Parse(value);
                }
                catch (System.FormatException e)
                {
                    Debug.LogError(e.Message);
                    result = 0;
                }
                break;
            case PropertyType.Float:
                try
                {
                    result = float.Parse(value.Replace(".", ","));
                }
                catch(System.FormatException e)
                {
                    Debug.LogError(e.Message);
                    result = 0f;
                }
                break;
            case PropertyType.String:
            default:
                result = value;
                break;
        }
        Debug.Log(result.GetType() + " | " + result);
        return result;
    }

    /// <summary>
    /// Converts a Property into a value inside a PropertyGroup.
    /// </summary>
    /// <param name="property">Property to be converted.</param>
    /// <param name="pg">PropertyGroup that will include the new Property.</param>
    public static void ApplyProperty(Property property, ref PropertyGroup<string, object> pg,
        System.Func<object, object, bool> predicate = null)
    {
        pg.Set(property.name.ToString(), ParseValue(GetType(property.name), property.value), predicate);
    }
}
