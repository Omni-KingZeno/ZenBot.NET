using System.ComponentModel;

namespace SysBot.Pokemon;

public class DisplayedInfoArrayConverter : TypeConverter
{
    public override bool GetPropertiesSupported(ITypeDescriptorContext? context) => true;

    public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes)
    {
        if (value is DisplayedInfo[] array)
        {
            var props = new PropertyDescriptor[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                props[i] = new ArrayElementPropertyDescriptor(array, i);
            }
            return new PropertyDescriptorCollection(props);
        }
        return PropertyDescriptorCollection.Empty;
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is DisplayedInfo[])
        {
            return $"Pokémon Info Displayed";
        }
        return base.ConvertTo(context, culture, value, destinationType);
    }
}

public class ArrayElementPropertyDescriptor(Array array, int index) : PropertyDescriptor($"Line {index + 1}", null)
{
    public override Type ComponentType => array.GetType();
    public override bool IsReadOnly => false;
    public override Type PropertyType => array.GetType().GetElementType()!;
    public override TypeConverter Converter => TypeDescriptor.GetConverter(PropertyType);

    public override object? GetValue(object? component) => array.GetValue(index);
    public override void SetValue(object? component, object? value) => array.SetValue(value, index);
    public override bool CanResetValue(object component) => false;
    public override void ResetValue(object component) { }
    public override bool ShouldSerializeValue(object component) => true;
}

public class SortedDisplayedInfoConverter : EnumConverter
{
    public SortedDisplayedInfoConverter() : base(typeof(DisplayedInfo)) { }

    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context)
    {
        var values = Enum.GetValues<DisplayedInfo>().Cast<object>().OrderBy(v => v.ToString()).ToArray();
        return new StandardValuesCollection(values);
    }
}
